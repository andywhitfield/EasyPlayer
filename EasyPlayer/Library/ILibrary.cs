using System;
using System.Collections.ObjectModel;

namespace EasyPlayer.Library
{
    public interface ILibrary
    {
        ObservableCollection<MediaItem> MediaItems { get; }
        MediaItem AddNewMediaItem(string name, Uri originalUri);
    }
}
