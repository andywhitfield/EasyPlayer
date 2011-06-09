using System.Collections.Generic;
using System.Reflection;

namespace EasyPlayer.Library
{
    public class Library : ILibrary
    {
        private readonly IEnumerable<IMediaItem> dummyItems;

        public Library()
        {
            dummyItems = new List<IMediaItem> {
                new MediaItem { Name = "Kalimba", DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Kalimba.mp3") },
                new MediaItem { Name = "Maid with the Flaxen Hair", DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Maid with the Flaxen Hair.mp3") }
            };
        }

        public IEnumerable<IMediaItem> MediaItems
        {
            get { return dummyItems; }
        }
    }
}
