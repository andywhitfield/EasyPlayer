using System.Collections.Generic;

namespace EasyPlayer.Library
{
    public interface ILibrary
    {
        IEnumerable<IMediaItem> MediaItems { get; }
    }
}
