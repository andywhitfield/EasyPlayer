using EasyPlayer.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyPlayer.Library;
using EasyPlayer.Library.DefaultView;

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

        [TestMethod]
        public void Container_should_return_same_instance_for_library()
        {
            var bootstrap = new AppBootstrapper();
            var library1 = bootstrap.GetInstance<ILibrary>();
            Assert.IsNotNull(library1);

            var library2 = bootstrap.GetInstance<ILibrary>();
            Assert.IsNotNull(library2);

            Assert.AreSame(library1, library2);
        }

        [TestMethod]
        public void Container_should_a_new_instance_for_mediaitemview()
        {
            var bootstrap = new AppBootstrapper();
            var view1 = bootstrap.GetInstance<MediaItemView>();
            Assert.IsNotNull(view1);

            var view2 = bootstrap.GetInstance<MediaItemView>();
            Assert.IsNotNull(view2);

            Assert.AreNotSame(view1, view2);
        }
    }
}
