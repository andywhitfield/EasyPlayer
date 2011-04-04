using Caliburn.Micro;

namespace EasyPlayer
{
    public class ShellViewModel : Conductor<IAppWidget>
    {
        public BindableCollection<IAppWidget> Widgets { get; private set; }
        public IAppWidget ActiveWidget { get; set; }

        public ShellViewModel()
        {
            // populate widgets - could probably be replaced by an IOC container auto-populating this collection
            PopulateWidgets();
            ActivateWidget(Widgets[0]);
        }

        public void ActivateWidget(IAppWidget widget)
        {
            ActivateItem(widget);
        }

        private void PopulateWidgets()
        {
            Widgets = new BindableCollection<IAppWidget> {
                new LibraryWidgetViewModel(),
                new Mp3WidgetViewModel()
            };
        }
    }
}
