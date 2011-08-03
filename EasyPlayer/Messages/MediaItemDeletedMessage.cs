using EasyPlayer.Library;

namespace EasyPlayer.Messages
{
    public class MediaItemDeletedMessage
    {
        public readonly MediaItem Media;

        public MediaItemDeletedMessage(MediaItem media)
        {
            this.Media = media;
        }
    }
}
