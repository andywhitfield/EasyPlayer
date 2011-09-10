using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.MediaControl;
using EasyPlayer.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.MediaControl
{
    [TestClass]
    public class NowPlayingViewModelTest
    {
        [TestMethod]
        public void Given_a_play_request_message_should_stop_currently_playing_item_and_start_new_media()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var media = new Mock<MediaItem>();
            var dummyStream = new MemoryStream();
            media.Setup(x => x.Name).Returns("Fake Media Item");
            media.Setup(x => x.DataStream).Returns(() => dummyStream);
            var vm = new NowPlayingViewModel(eventAgg.Object);

            var capturedEvents = new List<string>();
            vm.PropertyChanged += (s, e) => capturedEvents.Add(e.PropertyName);
            vm.Handle(new PlayRequestMessage(media.Object));

            var playStateChanges = capturedEvents.Where(x => x == "MediaPlayerState");
            Assert.AreEqual(1, playStateChanges.Count());
            Assert.AreEqual(PlayerState.Playing, vm.MediaPlayerState);
            Assert.AreSame(dummyStream, vm.MediaStream);
            Assert.AreEqual("Fake Media Item", vm.CurrentlyPlaying);
        }

        [TestMethod]
        public void When_playing_then_playpause_should_be_pause_otherwise_should_be_play()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var media = new Mock<MediaItem>();
            var vm = new NowPlayingViewModel(eventAgg.Object);
            vm.Handle(new PlayRequestMessage(media.Object));

            vm.MediaPlayerState = PlayerState.Stopped;
            Assert.AreEqual("Play", vm.PlayPauseText);

            vm.MediaPlayerState = PlayerState.Paused;
            Assert.AreEqual("Play", vm.PlayPauseText);

            vm.MediaPlayerState = PlayerState.Playing;
            Assert.AreEqual("Pause", vm.PlayPauseText);
        }

        [TestMethod]
        public void When_no_item_playing_then_should_not_change_play_state()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var vm = new NowPlayingViewModel(eventAgg.Object);

            Assert.IsFalse(vm.CanPlayPause);
            Assert.IsFalse(vm.CanStop);

            Assert.AreEqual(PlayerState.Stopped, vm.MediaPlayerState);
            vm.MediaPlayerState = PlayerState.Paused;
            Assert.AreEqual(PlayerState.Stopped, vm.MediaPlayerState);
            vm.MediaPlayerState = PlayerState.Playing;
            Assert.AreEqual(PlayerState.Stopped, vm.MediaPlayerState);

            // now, requesting an item to be played should allow the state change
            var media = new Mock<MediaItem>();
            vm.Handle(new PlayRequestMessage(media.Object));

            Assert.IsTrue(vm.CanPlayPause);
            Assert.IsTrue(vm.CanStop);
            vm.MediaPlayerState = PlayerState.Playing;
            Assert.AreEqual(PlayerState.Playing, vm.MediaPlayerState);
            Assert.AreEqual("Pause", vm.PlayPauseText);
        }

        [TestMethod]
        public void When_media_is_opened_should_forward_to_previous_media_position()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var vm = new NowPlayingViewModel(eventAgg.Object);

            var media = new MediaItem();
            media.MediaPosition = 100;
            vm.Handle(new PlayRequestMessage(media));
            Assert.AreEqual(0, vm.SliderPosition);

            vm.MediaOpened();
            Assert.AreEqual(100, vm.SliderPosition);
        }

        [TestMethod]
        public void When_requesting_to_play_currently_playing_media_should_just_resume_playing()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var vm = new NowPlayingViewModel(eventAgg.Object);

            var media = new MediaItem();
            media.MediaPosition = 100;
            vm.Handle(new PlayRequestMessage(media));
            Assert.AreEqual(0, vm.SliderPosition);
            Assert.AreEqual(PlayerState.Playing, vm.MediaPlayerState);

            vm.SliderPosition = 10;

            vm.Handle(new PlayRequestMessage(media));
            Assert.AreEqual(10, vm.SliderPosition);
            Assert.AreEqual(PlayerState.Playing, vm.MediaPlayerState);
        }
    }
}
