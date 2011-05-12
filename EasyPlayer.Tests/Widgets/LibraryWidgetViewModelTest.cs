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
            var libraryItems = new[] { new MediaItem { Name = "media1" }, new MediaItem { Name = "media2" } };
            library.Setup(x => x.MediaItems).Returns(libraryItems);

            var libraryWidget = new LibraryWidgetViewModel(eventAgg.Object, library.Object);
            libraryWidget.PlayMediaItem(libraryItems[0]);
            eventAgg.Verify(x => x.Publish(It.Is<PlayRequestMessage>(r => object.ReferenceEquals(r.Media, libraryItems[0]))));
        }
    }
}
