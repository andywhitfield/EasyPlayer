using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;
using EasyPlayer.Widgets.PodcastPlay;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets.PodcastPlay
{
    [TestClass]
    public class PodcastPlayViewModelTest
    {
        [TestMethod]
        public void When_items_are_moved_in_playlists_then_persister_should_be_called()
        {
            var item1 = new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true };
            var item2 = new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true };
            var item3 = new MediaItem { Id = "test-3", Name = "test 3", IsAvailable = true };
            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(new ObservableCollection<MediaItem>(new[] { item1, item2, item3 }));
            var eventAgg = new Mock<IEventAggregator>().Object;
            var persister = new Mock<IPodcastPlayPersister>();
            persister.Setup(p => p.IsEmpty).Returns(true);

            var vm = new PodcastPlayViewModel(library.Object, eventAgg, persister.Object);
            Assert.AreEqual(3, vm.PlaylistItems.Count());
            Assert.AreEqual(0, vm.ExcludedItems.Count());

            vm.Move(vm.PlaylistItems.ElementAt(0), false);
            persister.Verify(p => p.Save(It.Is<IEnumerable<string>>(pl => pl.SequenceEqual(new[] { "test-2", "test-1", "test-3" })), It.Is<IEnumerable<string>>(pl => pl.SequenceEqual(new string[0]))));

            vm.SwitchPlaylist(vm.PlaylistItems.ElementAt(2));
            persister.Verify(p => p.Save(It.Is<IEnumerable<string>>(pl => pl.SequenceEqual(new[] { "test-2", "test-1" })), It.Is<IEnumerable<string>>(pl => pl.SequenceEqual(new[] { "test-3" }))));
        }

        [TestMethod]
        public void When_an_item_is_added_to_the_library_should_add_to_the_bottom_of_the_playlist()
        {
            var libraryItems = new ObservableCollection<MediaItem>(new[] { new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true } });

            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(libraryItems);
            var eventAgg = new Mock<IEventAggregator>().Object;
            var persister = new Mock<IPodcastPlayPersister>();
            persister.Setup(p => p.IsEmpty).Returns(true);

            var vm = new PodcastPlayViewModel(library.Object, eventAgg, persister.Object);
            Assert.AreEqual(1, vm.PlaylistItems.Count());
            Assert.AreEqual(0, vm.ExcludedItems.Count());

            libraryItems.Add(new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true });
            Assert.AreEqual(2, vm.PlaylistItems.Count());
            Assert.AreEqual("test-2", vm.PlaylistItems.Last().MediaItemView.Item.Id);
        }

        [TestMethod]
        public void When_an_item_is_removed_from_the_library_should_remove_from_the_playlist()
        {
            var libraryItems = new ObservableCollection<MediaItem>(new[] {
                new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true },
                new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true }
            });

            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(libraryItems);
            var eventAgg = new Mock<IEventAggregator>().Object;
            var persister = new Mock<IPodcastPlayPersister>();
            persister.Setup(p => p.IsEmpty).Returns(true);

            var vm = new PodcastPlayViewModel(library.Object, eventAgg, persister.Object);
            Assert.AreEqual(2, vm.PlaylistItems.Count());

            libraryItems.RemoveAt(0);
            Assert.AreEqual(1, vm.PlaylistItems.Count());
            Assert.AreEqual("test-2", vm.PlaylistItems.First().MediaItemView.Item.Id);

            libraryItems.RemoveAt(0);
            Assert.AreEqual(0, vm.PlaylistItems.Count());
        }

        [TestMethod]
        public void Given_playing_from_the_playlist_when_media_finishes_should_play_the_next_item()
        {
            var libraryItems = new ObservableCollection<MediaItem>(new[] {
                new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true },
                new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true }
            });

            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(libraryItems);
            var eventAgg = new Mock<IEventAggregator>();
            var persister = new Mock<IPodcastPlayPersister>();
            persister.Setup(p => p.IsEmpty).Returns(true);

            var vm = new PodcastPlayViewModel(library.Object, eventAgg.Object, persister.Object);

            // first item is requested to play
            vm.PlaylistItems.First().MediaItemView.PlayMediaItem();
            
            // now finished
            vm.Handle(new NowPlayingMediaEndedMessage(libraryItems[0]));

            Assert.AreEqual(1, vm.PlaylistItems.Count());
            eventAgg.Verify(e => e.Publish(It.Is<PlayRequestMessage>(p => p.Media.Id == "test-2")));

            // then second item finished
            vm.Handle(new NowPlayingMediaEndedMessage(libraryItems[1]));
            Assert.AreEqual(0, vm.PlaylistItems.Count());
        }

        [TestMethod]
        public void Given_an_item_played_one_minute_ago_should_not_delete()
        {
            var libraryItems = new ObservableCollection<MediaItem>(new[] {
                new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true },
                new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true }
            });
            libraryItems[0].ExtendedProperties.Add("PodcastPlay.Played", DateTime.UtcNow.ToString("u"));
            libraryItems[1].ExtendedProperties.Add("PodcastPlay.Played", DateTime.UtcNow.AddMinutes(-1).ToString("u"));

            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(libraryItems);

            var vm = new PodcastPlayViewModel(library.Object, new Mock<IEventAggregator>().Object, new Mock<IPodcastPlayPersister>().Object);
            vm.RemovePlayedItemsAfter = TimeSpan.FromSeconds(90);
            vm.RemovePlayedItems(null);

            Assert.IsFalse(libraryItems[0].IsDeleted);
            Assert.IsFalse(libraryItems[1].IsDeleted);
        }

        [TestMethod]
        public void Given_an_item_played_two_minutes_ago_should_delete_when_timeout_is_90_seconds()
        {
            var libraryItems = new ObservableCollection<MediaItem>(new[] {
                new MediaItem { Id = "test-1", Name = "test 1", IsAvailable = true },
                new MediaItem { Id = "test-2", Name = "test 2", IsAvailable = true }
            });
            libraryItems[0].ExtendedProperties.Add("PodcastPlay.Played", "some-invalid-date");
            libraryItems[1].ExtendedProperties.Add("PodcastPlay.Played", DateTime.UtcNow.AddMinutes(-2).ToString("u"));

            var library = new Mock<ILibrary>();
            library.Setup(l => l.MediaItems).Returns(libraryItems);
            var eventAgg = new Mock<IEventAggregator>();

            var vm = new PodcastPlayViewModel(library.Object, eventAgg.Object, new Mock<IPodcastPlayPersister>().Object);
            vm.RemovePlayedItemsAfter = TimeSpan.FromSeconds(90);
            vm.RemovePlayedItems(null);

            Assert.IsFalse(libraryItems[0].IsDeleted);
            Assert.IsTrue(libraryItems[1].IsDeleted);
            eventAgg.Verify(e => e.Publish(It.Is<MediaItemDeletedMessage>(r => object.ReferenceEquals(r.Media, libraryItems[1]))));
        }
    }
}
