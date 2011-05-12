using EasyPlayer.Library;

namespace EasyPlayer.Messages
{
    public class PlayRequestMessage
    {
        public readonly IMediaItem Media;

        public PlayRequestMessage(IMediaItem media)
        {
            this.Media = media;
        }
    }
}
