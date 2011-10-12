using EasyPlayer.Library;
using EasyPlayer.Library.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Library
{
    [TestClass]
    public class LibraryTest
    {
        [TestMethod]
        public void When_mediaitem_is_updated_then_collection_should_notify_of_change()
        {
            var mediaItems = new[] {
                new MediaItem { Id = "item-1", Name = "item 1" },
                new MediaItem { Id = "item-2", Name = "item 2" }
            };

            var persister = new Mock<IMediaItemPersister>();
            persister.Setup(p => p.LoadAll()).Returns(mediaItems);

            var library = new EasyPlayer.Library.Library(persister.Object);

            var collectionChanged = false;

            library.MediaItems.CollectionChanged += (s, e) => collectionChanged = true;
            library.Update(library.MediaItems[0]);

            Assert.IsTrue(collectionChanged);
        }
    }
}
