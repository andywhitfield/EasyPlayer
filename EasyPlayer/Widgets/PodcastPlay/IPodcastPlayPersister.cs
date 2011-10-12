using System.Collections.Generic;

namespace EasyPlayer.Widgets.PodcastPlay
{
    public interface IPodcastPlayPersister
    {
        bool IsEmpty { get; }
        IEnumerable<string> PlaylistItems { get; }
        IEnumerable<string> ExcludedLibraryItems { get; }
        void Save(IEnumerable<string> playlistItems, IEnumerable<string> excludedLibraryItems);
    }
}
