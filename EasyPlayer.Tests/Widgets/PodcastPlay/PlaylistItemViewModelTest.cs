using System.Collections.ObjectModel;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Library.DefaultView;
using EasyPlayer.Widgets.PodcastPlay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets.PodcastPlay
{
    [TestClass]
    public class PlaylistItemViewModelTest
    {
        [TestMethod]
        public void When_is_already_in_playlist_should_not_be_able_to_include()
        {
            var playlistItem = Create(true, 0, 1);
            Assert.IsTrue(playlistItem.CanExcludeFromPlaylist);
            Assert.IsFalse(playlistItem.CanIncludeInPlaylist);

            playlistItem = Create(false, 0, 1);
            Assert.IsFalse(playlistItem.CanExcludeFromPlaylist);
            Assert.IsTrue(playlistItem.CanIncludeInPlaylist);
        }

        [TestMethod]
        public void Should_only_allow_to_move_up_playlist_if_not_at_top_and_down_if_not_at_bottom()
        {
            var playlistItem = Create(true, 0, 10);
            Assert.IsFalse(playlistItem.CanMoveUpPlaylist);
            Assert.IsTrue(playlistItem.CanMoveDownPlaylist);

            playlistItem = Create(true, 9, 10);
            Assert.IsTrue(playlistItem.CanMoveUpPlaylist);
            Assert.IsFalse(playlistItem.CanMoveDownPlaylist);

            playlistItem = Create(true, 8, 10);
            Assert.IsTrue(playlistItem.CanMoveUpPlaylist);
            Assert.IsTrue(playlistItem.CanMoveDownPlaylist);

            playlistItem = Create(true, 1, 10);
            Assert.IsTrue(playlistItem.CanMoveUpPlaylist);
            Assert.IsTrue(playlistItem.CanMoveDownPlaylist);

            playlistItem = Create(true, 0, 1);
            Assert.IsFalse(playlistItem.CanMoveUpPlaylist);
            Assert.IsFalse(playlistItem.CanMoveDownPlaylist);
        }

        private PlaylistItemViewModel Create(bool isInPlaylist, int displayIndex, int totalPlaylistItems)
        {
            var item = new MediaItem { Id = "test-1", Name = "test 1" };
            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(new ObservableCollection<MediaItem>(new[] { item }));
            var eventAgg = new Mock<IEventAggregator>().Object;
            var persister = new Mock<IPodcastPlayPersister>().Object;
            return new PlaylistItemViewModel(new PodcastPlayViewModel(library.Object, eventAgg, persister), new MediaItemViewModel(eventAgg, item), isInPlaylist, displayIndex, totalPlaylistItems);
        }
    }
}
