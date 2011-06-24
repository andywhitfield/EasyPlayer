using System.IO;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;
using System.Windows.Threading;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyPlayer.MediaControl
{
    public class NowPlayingViewModel : Screen, IHandle<PlayRequestMessage>
    {
        private MediaElement mediaElement;
        private IMediaItem currentlyPlaying;
        private PlayerState mediaPlayerState = PlayerState.Stopped;
        private DispatcherTimer updateProgressTimer;
        private bool draggingSlider;

        public NowPlayingViewModel(IEventAggregator eventAgg)
        {
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
        public Stream MediaStream { get { return currentlyPlaying == null ? null : currentlyPlaying.DataStream; } }

        public void MediaOpened()
        {
            if (mediaElement != null && mediaElement.NaturalDuration.HasTimeSpan)
            {
                var ts = mediaElement.NaturalDuration.TimeSpan;
                MediaPositionMax = ts.TotalSeconds;
                MediaPositionLargeChange = Math.Min(10, ts.Seconds / 10);
                MediaPositionText = string.Format("00:00:00 / {0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds);
            }
        }

        public void MediaEnded()
        {
            Stop();
        }

        public void MediaFailed() { }

        public PlayerState MediaPlayerState
        {
            get { return mediaPlayerState; }
            set
            {
                if (!IsCurrentlyPlaying) return;
                mediaPlayerState = value;
                NotifyOfPropertyChange(() => MediaPlayerState);
                NotifyOfPropertyChange(() => PlayPauseText);

                if (mediaPlayerState == PlayerState.Playing) updateProgressTimer.Start();
                else
                {
                    updateProgressTimer.Stop();
                    if (mediaPlayerState == PlayerState.Stopped)
                    {
                        SliderPosition = 0;
                        NotifyOfPropertyChange(() => SliderPosition);
                    }
                }
            }
        }

        public string PlayPauseText { get { return MediaPlayerState == PlayerState.Playing ? "Pause" : "Play"; } }

        public bool CanPlayPause { get { return IsCurrentlyPlaying; } }
        public void PlayPause()
        {
            MediaPlayerState = MediaPlayerState == PlayerState.Playing ? PlayerState.Paused : PlayerState.Playing;
        }

        public bool CanStop { get { return IsCurrentlyPlaying; } }
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
            MediaPlayerState = PlayerState.Stopped;
            currentlyPlaying = message.Media;
            NotifyOfPropertyChange(() => CurrentlyPlaying);
            NotifyOfPropertyChange(() => MediaStream);
            NotifyOfPropertyChange(() => CanPlayPause);
            NotifyOfPropertyChange(() => CanStop);
            MediaPlayerState = PlayerState.Playing;
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
    }
}
