using System.Collections.ObjectModel;
using System;

namespace EasyPlayer.Library
{
    public interface ILibrary
    {
        ObservableCollection<MediaItem> MediaItems { get; }
        MediaItem AddNewMediaItem(string name, Uri originalUri);
    }
}
