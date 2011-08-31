using EasyPlayer.Library;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPlayer.Tests.Library
{
    [TestClass]
    public class MediaItemTest
    {
        [TestMethod]
        public void By_default_an_item_is_not_available_and_not_deleted()
        {
            Assert.IsFalse(new MediaItem().IsAvailable);
            Assert.IsFalse(new MediaItem().IsDeleted);
        }

        [TestMethod]
        public void Download_progress_should_be_constrained_from_zero_to_one_hundred()
        {
            var item = new MediaItem();
            Assert.AreEqual(0, item.DownloadProgress);

            item.DownloadProgress = -1;
            Assert.AreEqual(0, item.DownloadProgress);

            item.DownloadProgress = 1;
            Assert.AreEqual(1, item.DownloadProgress);

            item.DownloadProgress = 99;
            Assert.AreEqual(99, item.DownloadProgress);

            item.DownloadProgress = 100;
            Assert.AreEqual(100, item.DownloadProgress);

            item.DownloadProgress = 101;
            Assert.AreEqual(100, item.DownloadProgress);

            item.DownloadProgress = int.MaxValue;
            Assert.AreEqual(100, item.DownloadProgress);

            item.DownloadProgress = int.MinValue;
            Assert.AreEqual(0, item.DownloadProgress);
        }
    }
}
