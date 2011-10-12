using System.Linq;
using EasyPlayer.Persistence;
using EasyPlayer.Widgets.PodcastPlay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets.PodcastPlay
{
    [TestClass]
    public class PodcastPlayPersisterTest
    {
        [TestMethod]
        public void Given_saved_playlists_should_be_able_load_again()
        {
            var persistence = new Mock<IPersistence>();
            var savedContents = "";
            persistence.Setup(p => p.WriteTextFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((f, c) => savedContents = c);
            persistence.Setup(p => p.ReadTextFile(It.IsAny<string>())).Returns(() => savedContents);

            var persister = new PodcastPlayPersister(persistence.Object);
            persister.Save(new[] { "item-1", "item-2" }, new[] { "item-3" });
            
            Assert.AreEqual(2, persister.PlaylistItems.Count());
            Assert.AreEqual("item-1", persister.PlaylistItems.ElementAt(0));
            Assert.AreEqual("item-2", persister.PlaylistItems.ElementAt(1));

            Assert.AreEqual(1, persister.ExcludedLibraryItems.Count());
            Assert.AreEqual("item-3", persister.ExcludedLibraryItems.ElementAt(0));
        }
    }
}
