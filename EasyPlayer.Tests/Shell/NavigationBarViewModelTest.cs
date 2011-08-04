using EasyPlayer.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EasyPlayer.Tests.Shell
{
    [TestClass]
    public class NavigationBarViewModelTest
    {
        [TestMethod]
        public void When_search_text_contains_empty_string_or_whitespace_should_disable_go_button()
        {
            var anyNavigator = new Mock<ICanNavigate>();
            anyNavigator.Setup(n => n.CanNavigateTo(It.IsAny<string>())).Returns(true);

            var model = new NavigationBarViewModel(new [] { anyNavigator.Object });
            Assert.IsFalse(model.CanGo);

            model.SearchValue = "  ";
            Assert.IsFalse(model.CanGo);

            model.SearchValue = "file.mp3";
            Assert.IsTrue(model.CanGo);
        }

        [TestMethod]
        public void When_go_is_clicked_then_appropriate_navigator_should_be_sent_the_search_string()
        {
            Verify(navigators => navigators.ElementAt(0), "someaudio.mp3");
            Verify(navigators => navigators.ElementAt(1), "somevideo.mp4");
        }

        [TestMethod]
        public void Given_a_search_string_that_doesnt_match_a_navigator_then_go_should_be_disabled()
        {
            var mp3Navigator = new Mock<ICanNavigate>();
            mp3Navigator.Setup(n => n.CanNavigateTo(It.IsRegex(".*mp3$"))).Returns(true);

            var model = new NavigationBarViewModel(new[] { mp3Navigator.Object });
            Assert.IsFalse(model.CanGo);

            model.SearchValue = "file.mp3";
            Assert.IsTrue(model.CanGo);

            model.SearchValue = "vid.mp4";
            Assert.IsFalse(model.CanGo);
        }

        private void Verify(Func<IEnumerable<Mock<ICanNavigate>>, Mock<ICanNavigate>> navigatorPicker, string searchString)
        {
            var mp3Navigator = new Mock<ICanNavigate>();
            mp3Navigator.Setup(n => n.CanNavigateTo(It.IsRegex(".*mp3$"))).Returns(true);

            var mp4Navigator = new Mock<ICanNavigate>();
            mp4Navigator.Setup(n => n.CanNavigateTo(It.IsRegex(".*mp4$"))).Returns(true);

            var navigators = new[] { mp3Navigator, mp4Navigator };

            var model = new NavigationBarViewModel(navigators.Select(x => x.Object));
            model.SearchValue = searchString;
            Assert.IsTrue(model.CanGo);

            model.Go();

            navigatorPicker(navigators).Verify(n => n.NavigateTo(searchString));
        }
    }
}
