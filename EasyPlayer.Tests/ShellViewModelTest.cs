using System.Collections.Generic;
using Caliburn.Micro;
using EasyPlayer.Shell;
using EasyPlayer.Widgets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests
{
    [TestClass]
    public class ShellViewModelTest
    {
        [TestMethod]
        public void When_nowplaying_is_visible_then_active_item_should_not_be_visible()
        {
            var widgets = new Mock<IAppWidget>();
            var eventAgg = new Mock<IEventAggregator>();
            var vm = new ShellViewModel(new[] { widgets.Object }, eventAgg.Object, new Mock<IWindowManager>().Object, null, null);

            Assert.IsFalse(vm.NowPlayingVisible);
            Assert.IsTrue(vm.ActiveItemVisible);

            var capturedEvents = new List<string>();
            vm.PropertyChanged += (s, e) => capturedEvents.Add(e.PropertyName);
            vm.NowPlayingVisible = true;

            Assert.IsTrue(vm.NowPlayingVisible);
            Assert.IsFalse(vm.ActiveItemVisible);
            Assert.AreEqual(2, capturedEvents.Count);
            Assert.IsTrue(capturedEvents.Contains("NowPlayingVisible"));
            Assert.IsTrue(capturedEvents.Contains("ActiveItemVisible"));
        }
    }
}
