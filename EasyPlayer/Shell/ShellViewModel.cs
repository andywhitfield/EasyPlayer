using Caliburn.Micro;
using EasyPlayer.Widgets;
using System.Collections.Generic;
using System.Linq;

namespace EasyPlayer.Shell
{
    public class ShellViewModel : Conductor<IAppWidget>
    {
        public BindableCollection<IAppWidget> Widgets { get; private set; }
        public IAppWidget ActiveWidget { get; set; }

        public ShellViewModel(IEnumerable<IAppWidget> widgets)
        {
            Widgets = new BindableCollection<IAppWidget>(widgets);
            ActivateWidget(Widgets.FirstOrDefault(a => a.Name == "Library"));
        }

        public void ActivateWidget(IAppWidget widget)
        {
            if (widget == null) return;
            ActivateItem(widget);
        }
    }
}
