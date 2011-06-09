using System.IO;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;

namespace EasyPlayer.MediaControl
{
    public class NowPlayingViewModel : Screen, IHandle<PlayRequestMessage>
    {
        private IMediaItem currentlyPlaying;

        public NowPlayingViewModel(IEventAggregator eventAgg)
        {
            eventAgg.Subscribe(this);
        }

        public string CurrentlyPlaying { get { return currentlyPlaying == null ? "(Nothing)" : currentlyPlaying.Name; } }
        public Stream MediaStream { get { return currentlyPlaying == null ? null : currentlyPlaying.DataStream; } }
        public void MediaOpened() { }
        public void MediaEnded() { }
        public void MediaFailed() { }

        public void Handle(PlayRequestMessage message)
        {
            currentlyPlaying = message.Media;
            NotifyOfPropertyChange(() => CurrentlyPlaying);
            NotifyOfPropertyChange(() => MediaStream);
        }
    }
}
