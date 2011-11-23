using System;
using System.Collections.ObjectModel;

namespace EasyPlayer.Widgets.Feeds
{
    public interface IFeedRepository
    {
        ObservableCollection<Feed> Feeds { get; }
        void Add(Uri feedUrl);
        void Delete(Feed item);
        void Update(Feed feed);
    }
}
