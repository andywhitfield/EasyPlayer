using System.IO;

namespace EasyPlayer.Library
{
    public interface IMediaItem
    {
        string Name { get; }
        Stream DataStream { get; }
    }
}
