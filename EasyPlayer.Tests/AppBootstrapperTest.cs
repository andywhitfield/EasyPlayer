using EasyPlayer.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void Container_can_instantiate_shell_view_model()
        {
            var bootstrap = new AppBootstrapper();
            var model = bootstrap.GetInstance<ShellViewModel>();
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.ActiveItem);
            Assert.AreEqual("Library", model.ActiveItem.Name);
        }
    }
}
