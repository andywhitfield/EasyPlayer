﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Library.Persistence;
using EasyPlayer.Messages;

namespace EasyPlayer.MediaControl
{
    public class NowPlayingViewModel : Screen, IHandle<PlayRequestMessage>
    {
        private static ILog log = Logger.Log<NowPlayingViewModel>();

        private readonly IMediaItemPersister mediaItemPersister;
        private readonly IEventAggregator eventAgg;
        private MediaElement mediaElement;
        private MediaItem currentlyPlaying;
        private PlayerState mediaPlayerState = PlayerState.Stopped;
        private DispatcherTimer updateProgressTimer;
        private bool draggingSlider;

        public NowPlayingViewModel(IEventAggregator eventAgg, IMediaItemPersister mediaItemPersister)
        {
            this.mediaItemPersister = mediaItemPersister;
            this.eventAgg = eventAgg;
            eventAgg.Subscribe(this);
            updateProgressTimer = new DispatcherTimer();
            updateProgressTimer.Interval = TimeSpan.FromMilliseconds(300);
            updateProgressTimer.Tick += (o, s) => UpdateProgress();
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            var viewEl = view as FrameworkElement;
            mediaElement = viewEl.FindName("m_MediaPlayer") as MediaElement;
            //need to do some funky stuff to capture mouse down/up events on the slider
            var slider = viewEl.FindName("MediaPosition") as UIElement;
            if (slider != null)
            {
                slider.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler((s, e) => SliderMouseDown()), true);
                slider.AddHandler(FrameworkElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler((s, e) => SliderMouseUp()), true);
            }
        }

        public bool IsCurrentlyPlaying { get { return currentlyPlaying != null; } }
        public string CurrentlyPlaying { get { return currentlyPlaying == null ? "(Nothing)" : currentlyPlaying.Name; } }
        public Stream MediaStream { get { return currentlyPlaying == null ? null : currentlyPlaying.DataStream(); } }
        public void CurrentlyPlayingDeleted(object sender, EventArgs e) { Handle(new PlayRequestMessage(null)); }

        public void MediaOpened()
        {
            log.Info("Opened {0}", CurrentlyPlaying);
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                var ts = mediaElement.NaturalDuration.TimeSpan;
                MediaPositionMax = ts.TotalSeconds;
                MediaPositionLargeChange = Math.Min(10, ts.Seconds / 10);
                MediaPositionText = string.Format("00:00:00 / {0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds);

                log.Info("{0} has length {1}", CurrentlyPlaying, ts);
            }
            SliderMouseDown();
            SliderPosition = currentlyPlaying.MediaPosition;
            SliderMouseUp();
        }

        public void MediaEnded()
        {
            log.Info("Ended {0}", CurrentlyPlaying);
            Stop();
            eventAgg.Publish(new NowPlayingMediaEndedMessage(currentlyPlaying));
        }

        public void MediaFailed()
        {
            log.Warn("Failed to load {0}", CurrentlyPlaying);
            throw new Exception("Could not load media: " + CurrentlyPlaying);
        }

        public PlayerState MediaPlayerState
        {
            get { return mediaPlayerState; }
            set
            {
                if (!IsCurrentlyPlaying) return;
                mediaPlayerState = value;

                log.Info("Player is now in state {0}", mediaPlayerState);

                NotifyOfPropertyChange(() => MediaPlayerState);
                NotifyOfPropertyChange(() => PlayPauseText);
                NotifyOfPropertyChange(() => CanStop);

                if (mediaPlayerState == PlayerState.Playing) updateProgressTimer.Start();
                else
                {
                    updateProgressTimer.Stop();
                    if (mediaPlayerState == PlayerState.Stopped)
                        SliderPosition = 0;
                }
            }
        }

        public string PlayPauseText { get { return MediaPlayerState == PlayerState.Playing ? "Pause" : "Play"; } }

        public bool CanPlayPause { get { return IsCurrentlyPlaying; } }
        public void PlayPause()
        {
            MediaPlayerState = MediaPlayerState == PlayerState.Playing ? PlayerState.Paused : PlayerState.Playing;
        }

        public bool CanStop { get { return IsCurrentlyPlaying && MediaPlayerState != PlayerState.Stopped; } }
        public void Stop()
        {
            MediaPlayerState = PlayerState.Stopped;
        }

        private double sliderPosition;
        public double SliderPosition
        {
            get { return sliderPosition; }
            set
            {
                sliderPosition = value;
                if (mediaElement != null)
                {
                    var duration = mediaElement.NaturalDuration.TimeSpan;
                    var current = TimeSpan.FromSeconds(sliderPosition);

                    MediaPositionText = string.Format("{0:00}:{1:00}:{2:00} / {3:00}:{4:00}:{5:00}",
                        current.Hours, current.Minutes, current.Seconds, duration.Hours, duration.Minutes, duration.Seconds);
                }

                NotifyOfPropertyChange(() => SliderPosition);
            }
        }

        private string mediaPositionText;
        public string MediaPositionText { get { return mediaPositionText; } set { mediaPositionText = value; NotifyOfPropertyChange(() => MediaPositionText); } }

        private double mediaPositionMax;
        public double MediaPositionMax { get { return mediaPositionMax; } set { mediaPositionMax = value; NotifyOfPropertyChange(() => MediaPositionMax); } }

        private int mediaPositionLargeChange;
        public int MediaPositionLargeChange { get { return mediaPositionLargeChange; } set { mediaPositionLargeChange = value; NotifyOfPropertyChange(() => MediaPositionLargeChange); } }

        public void Handle(PlayRequestMessage message)
        {
            if (currentlyPlaying == message.Media)
            {
                MediaPlayerState = PlayerState.Playing;
                return;
            }

            if (currentlyPlaying != null)
            {
                currentlyPlaying.IsDeletedChanged -= CurrentlyPlayingDeleted;
                currentlyPlaying.MediaPosition = SliderPosition;
                mediaItemPersister.Save(currentlyPlaying);

                log.Info("Saved previous media position: {0}", currentlyPlaying.Name);
            }

            MediaPlayerState = PlayerState.Stopped;
            currentlyPlaying = message.Media;

            NotifyOfPropertyChange(() => CurrentlyPlaying);
            NotifyOfPropertyChange(() => MediaStream);
            NotifyOfPropertyChange(() => CanPlayPause);
            NotifyOfPropertyChange(() => CanStop);

            if (currentlyPlaying != null)
            {
                currentlyPlaying.IsDeletedChanged += CurrentlyPlayingDeleted;
                MediaPlayerState = PlayerState.Playing;
            }
            else
            {
                MediaPositionText = "";
                MediaPositionMax = 0;
            }

            log.Info("Started playing item: {0}", currentlyPlaying != null ? currentlyPlaying.Name : "(Nothing)");
        }

        public void SliderMouseDown() { if (!CanPlayPause) return; draggingSlider = true; }
        public void SliderMouseUp()
        {
            if (!CanPlayPause) return;
            if (mediaElement != null) mediaElement.Position = TimeSpan.FromSeconds(SliderPosition);
            draggingSlider = false;
            UpdateProgress();
        }

        public void UpdateProgress()
        {
            if (draggingSlider || mediaElement == null) return;
            SliderPosition = mediaElement.Position.TotalSeconds;
        }

        public void OnClose()
        {
            if (currentlyPlaying == null) return;
            currentlyPlaying.MediaPosition = SliderPosition;
            mediaItemPersister.Save(currentlyPlaying);
        }
    }
}
