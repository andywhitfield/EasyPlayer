using System;
using System.Collections.ObjectModel;
using EasyPlayer.Library;
using EasyPlayer.Widgets.Feeds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets.Feeds
{
    [TestClass]
    public class FeedsViewModelTest
    {
        [TestMethod]
        public void When_not_connected_to_the_network_should_not_download_feeds()
        {
            FeedsViewModel.IsNetworkAvailable = () => false;

            var gotNewItems = false;
            var feed = new Mock<Feed>();
            feed.Setup(f => f.GetNewItemsInFeed()).Callback(() => gotNewItems = true);
            var feedRepository = new Mock<IFeedRepository>();
            feedRepository.Setup(f => f.Feeds).Returns(new ObservableCollection<Feed> { feed.Object });

            var viewModel = new FeedsViewModel(new Mock<ILibrary>().Object, feedRepository.Object);
            viewModel.RefreshFeeds(null);

            Assert.IsFalse(gotNewItems);
        }

        [TestMethod]
        public void When_connected_to_the_network_should_check_feeds_and_download_new_item()
        {
            FeedsViewModel.IsNetworkAvailable = () => true;

            var gotNewItems = false;
            var feed = new Mock<Feed>();
            feed.CallBase = true;
            feed.Setup(f => f.Name).Returns("The Feed");
            feed.Object.DownloadedGuids = new[] { "item-1", "item-2" };

            var mediaUri = new Uri("http://blockedcontent/media.mp3");
            var feedItem = new FeedItem("new-item", "The Media", mediaUri);
            feed.Setup(f => f.GetNewItemsInFeed()).Callback(() => gotNewItems = true).Returns(new [] { feedItem });
            var feedRepository = new Mock<IFeedRepository>();
            feedRepository.Setup(f => f.Feeds).Returns(new ObservableCollection<Feed> { feed.Object });

            var library = new Mock<ILibrary>();

            var viewModel = new FeedsViewModel(library.Object, feedRepository.Object);
            viewModel.RefreshFeeds(null);

            Assert.IsTrue(gotNewItems);
            library.Verify(l => l.AddNewMediaItem("The Feed: The Media", mediaUri));
            feedRepository.Verify(fr => fr.Update(feed.Object));
            Assert.AreEqual(string.Join("|", new[] { "item-1", "item-2", "new-item" }), string.Join("|", feed.Object.DownloadedGuids));
        }
    }
}
