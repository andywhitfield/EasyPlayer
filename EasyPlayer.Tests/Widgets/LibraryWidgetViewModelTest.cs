using System;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;
using EasyPlayer.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets
{
    [TestClass]
    public class LibraryWidgetViewModelTest
    {
        [TestMethod]
        public void When_playing_media_should_publish_item_onto_event_aggregator()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var library = new Mock<ILibrary>();
            var libraryItems = new ObservableCollection<MediaItem> { new MediaItem { Name = "media1" }, new MediaItem { Name = "media2" } };
            library.Setup(x => x.MediaItems).Returns(libraryItems);

            var libraryWidget = new LibraryWidgetViewModel(eventAgg.Object, library.Object);
            libraryWidget.PlayMediaItem(libraryItems[0]);
            eventAgg.Verify(x => x.Publish(It.Is<PlayRequestMessage>(r => object.ReferenceEquals(r.Media, libraryItems[0])), null));
        }

        [TestMethod]
        public void Given_a_url_should_add_media_item_with_name_of_mp3_file()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var library = new Mock<ILibrary>();
            library.Setup(x => x.MediaItems).Returns(new ObservableCollection<MediaItem>());

            var libraryWidget = new LibraryWidgetViewModel(eventAgg.Object, library.Object);
            libraryWidget.NavigateTo("http://someserver/path/to/media.mp3");

            library.Verify(l => l.AddNewMediaItem("media", new Uri("http://someserver/path/to/media.mp3")));
        }


        [TestMethod]
        public void Given_a_local_file_should_add_media_item_with_name_of_mp3_file()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var library = new Mock<ILibrary>();
            library.Setup(x => x.MediaItems).Returns(new ObservableCollection<MediaItem>());

            var libraryWidget = new LibraryWidgetViewModel(eventAgg.Object, library.Object);
            libraryWidget.NavigateTo(@"C:\Users\me\My Music\the media.mp3");

            library.Verify(l => l.AddNewMediaItem("the media", new Uri(@"file://C:\Users\me\My Music\the media.mp3")));
        }
    }
}
