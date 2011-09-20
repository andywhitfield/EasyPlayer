using Caliburn.Micro;
using EasyPlayer.Messages;
using EasyPlayer.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Shell
{
    [TestClass]
    public class OutOfQuotaViewModelTest
    {
        [TestMethod]
        public void Minimum_increase_should_be_size_of_file_trying_to_save_minus_the_current_free_space()
        {
            var outOfQuota = new OutOfQuotaViewModel(new Mock<IEventAggregator>().Object, new OutOfQuotaMessage(100, 10, 15));
            Assert.AreEqual(5, outOfQuota.IncreaseToMin);
            Assert.AreEqual("5", outOfQuota.RequiredIncrease);
        }

        [TestMethod]
        public void Given_an_increase_of_quota_should_send_raise_quota_event()
        {
            var eventAgg = new Mock<IEventAggregator>();
            var outOfQuota = new OutOfQuotaViewModel(eventAgg.Object, new OutOfQuotaMessage(100, 10, 15));

            outOfQuota.Parent = new Mock<IConductor>().Object;
            outOfQuota.IncreaseTo = 150;
            outOfQuota.OK();

            eventAgg.Verify(e => e.Publish(It.Is<IncreaseQuotaMessage>(m => m.IncreaseBy == 50), null));
        }
    }
}
