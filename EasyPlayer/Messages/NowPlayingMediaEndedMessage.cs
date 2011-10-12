using EasyPlayer.Library;

namespace EasyPlayer.Messages
{
    public class NowPlayingMediaEndedMessage
    {
        private MediaItem media;

        public NowPlayingMediaEndedMessage(MediaItem media)
        {
            this.media = media;
        }

        public MediaItem Media { get { return media; } }
    }
}
