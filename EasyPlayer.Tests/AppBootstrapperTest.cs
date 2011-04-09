using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyPlayer.Shell;

namespace EasyPlayer.Tests
{
    [TestClass]
    public class AppBootstrapperTest
    {
        [TestMethod]
        public void Initial_model_should_be_the_shell()
        {
            var bootstrap = new AppBootstrapper();
            Assert.AreEqual(typeof(ShellViewModel), bootstrap.GetType().BaseType.GetGenericArguments()[0]);
        }
    }
}
