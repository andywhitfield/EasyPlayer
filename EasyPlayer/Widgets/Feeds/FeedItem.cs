using System;

namespace EasyPlayer.Widgets.Feeds
{
    public class FeedItem
    {
        public FeedItem(string guid, string title, Uri enclosureUrl)
        {
            this.Guid = guid;
            this.Title = title;
            this.EnclosureUrl = enclosureUrl;
        }

        public string Guid { get; private set; }
        public string Title { get; private set; }
        public Uri EnclosureUrl { get; private set; }
    }
}
