using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Messages;
using EasyPlayer.Widgets;

namespace EasyPlayer.Shell
{
    public class ShellViewModel : Conductor<IAppWidget>, IHandle<PlayRequestMessage>
    {
        public BindableCollection<IAppWidget> Widgets { get; private set; }
        public IAppWidget ActiveWidget { get; set; }

        public ShellViewModel(IEnumerable<IAppWidget> widgets, IEventAggregator eventAgg)
        {
            eventAgg.Subscribe(this);
            Widgets = new BindableCollection<IAppWidget>(widgets);
            ActivateWidget(Widgets.FirstOrDefault(a => a.Name == "Library"));
        }

        public void ActivateWidget(IAppWidget widget)
        {
            if (widget == null) return;
            ActivateItem(widget);
        }

        public void Handle(PlayRequestMessage message)
        {
            System.Windows.MessageBox.Show("Play: " + message.Media.Name);
        }
    }
}
