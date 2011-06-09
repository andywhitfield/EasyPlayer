using System.IO;

namespace EasyPlayer.Library
{
    public class MediaItem : IMediaItem
    {
        public string Name { get; set; }
        public Stream DataStream { get; set; }
    }
}
