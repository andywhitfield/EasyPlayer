using System.Collections.Generic;

namespace EasyPlayer.Library.Persistence
{
    public interface IMediaItemPersister
    {
        void Save(MediaItem mediaItem);
        IEnumerable<MediaItem> LoadAll();
    }
}
