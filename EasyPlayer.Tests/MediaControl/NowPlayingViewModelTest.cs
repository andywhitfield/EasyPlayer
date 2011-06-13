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
            var media = new Mock<IMediaItem>();
            var dummyStream = new MemoryStream();
            media.Setup(x => x.Name).Returns("Fake Media Item");
            media.Setup(x => x.DataStream).Returns(dummyStream);
            var vm = new NowPlayingViewModel(eventAgg.Object);

            var capturedEvents = new List<string>();
            vm.PropertyChanged += (s, e) => capturedEvents.Add(e.PropertyName);
            vm.Handle(new PlayRequestMessage(media.Object));

            var playStateChanges = capturedEvents.Where(x => x == "MediaPlayerState");
            Assert.AreEqual(2, playStateChanges.Count());
            Assert.AreEqual(PlayerState.Playing, vm.MediaPlayerState);
            Assert.AreSame(dummyStream, vm.MediaStream);
            Assert.AreEqual("Fake Media Item", vm.CurrentlyPlaying);
        }

        [TestMethod]
        public void When_playing_then_playpause_should_be_pause_otherwise_should_be_play()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var vm = new NowPlayingViewModel(eventAgg.Object);

            vm.MediaPlayerState = PlayerState.Stopped;
            Assert.AreEqual("Play", vm.PlayPauseText);

            vm.MediaPlayerState = PlayerState.Paused;
            Assert.AreEqual("Play", vm.PlayPauseText);

            vm.MediaPlayerState = PlayerState.Playing;
            Assert.AreEqual("Pause", vm.PlayPauseText);
        }
    }
}
