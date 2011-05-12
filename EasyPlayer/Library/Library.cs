using System.Collections.Generic;

namespace EasyPlayer.Library
{
    public class Library : ILibrary
    {
        private readonly IEnumerable<IMediaItem> dummyItems;

        public Library()
        {
            dummyItems = new List<IMediaItem> {
                new MediaItem { Name = "My first podcast" },
                new MediaItem { Name = "Another exciting podcast" }
            };
        }

        public IEnumerable<IMediaItem> MediaItems
        {
            get { return dummyItems; }
        }
    }
}
