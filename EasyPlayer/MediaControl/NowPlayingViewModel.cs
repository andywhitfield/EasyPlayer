using System.IO;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;

namespace EasyPlayer.MediaControl
{
    public class NowPlayingViewModel : Screen, IHandle<PlayRequestMessage>
    {
        private IMediaItem currentlyPlaying;
        private PlayerState mediaPlayerState = PlayerState.Stopped;

        public NowPlayingViewModel(IEventAggregator eventAgg)
        {
            eventAgg.Subscribe(this);
        }

        public string CurrentlyPlaying { get { return currentlyPlaying == null ? "(Nothing)" : currentlyPlaying.Name; } }
        public Stream MediaStream { get { return currentlyPlaying == null ? null : currentlyPlaying.DataStream; } }
        public void MediaOpened() { }
        public void MediaEnded() { }
        public void MediaFailed() { }

        public PlayerState MediaPlayerState
        {
            get { return mediaPlayerState; }
            set
            {
                mediaPlayerState = value;
                NotifyOfPropertyChange(() => MediaPlayerState);
                NotifyOfPropertyChange(() => PlayPauseText);
            }
        }
        
        public string PlayPauseText { get { return MediaPlayerState == PlayerState.Playing ? "Pause" : "Play"; } }
        
        public void PlayPause()
        {
            MediaPlayerState = MediaPlayerState == PlayerState.Playing ? PlayerState.Paused : PlayerState.Playing;
        }

        public void Stop()
        {
            MediaPlayerState = PlayerState.Stopped;
        }

        public void Handle(PlayRequestMessage message)
        {
            MediaPlayerState = PlayerState.Stopped;
            currentlyPlaying = message.Media;
            NotifyOfPropertyChange(() => CurrentlyPlaying);
            NotifyOfPropertyChange(() => MediaStream);
            MediaPlayerState = PlayerState.Playing;
        }
    }
}
