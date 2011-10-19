using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Library.DefaultView;
using EasyPlayer.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Library.DefaultView
{
    [TestClass]
    public class MediaItemViewModelTest
    {
        [TestMethod]
        public void Can_only_play_or_delete_media_when_media_is_available_and_not_deleted()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var mediaItem = new MediaItem();
            var model = new MediaItemViewModel(eventAgg.Object, mediaItem);

            Assert.IsFalse(model.CanPlayMediaItem);
            Assert.IsFalse(model.CanDeleteMediaItem);

            mediaItem.IsAvailable = true;
            Assert.IsTrue(model.CanPlayMediaItem);
            Assert.IsTrue(model.CanDeleteMediaItem);

            model.DeleteMediaItem();
            Assert.IsFalse(model.CanPlayMediaItem);
            Assert.IsFalse(model.CanDeleteMediaItem);
        }

        [TestMethod]
        public void Playing_media_should_send_a_play_message_to_the_event_agg()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var mediaItem = new MediaItem { Name = "test-item-to-play", IsAvailable = true };
            var model = new MediaItemViewModel(eventAgg.Object, mediaItem);
            model.PlayMediaItem();

            eventAgg.Verify(e => e.Publish(Match.Create<object>(m => MatchPlayRequestMessage(m, mediaItem))));
        }

        private bool MatchPlayRequestMessage(object m, MediaItem mediaItem)
        {
            var message = m as PlayRequestMessage;
            if (message == null) return false;

            return object.ReferenceEquals(message.Media, mediaItem);
        }
    }
}
