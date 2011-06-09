using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Messages;
using EasyPlayer.Widgets;

namespace EasyPlayer.Shell
{
    public class ShellViewModel : Conductor<IAppWidget>, IHandle<PlayRequestMessage>
    {
        private bool nowPlayingVisible = false;
        public BindableCollection<IAppWidget> Widgets { get; private set; }
        public IAppWidget ActiveWidget { get; set; }
        public bool NowPlayingVisible
        {
            get { return nowPlayingVisible; }
            set
            {
                if (nowPlayingVisible == value) return;
                nowPlayingVisible = value;
                NotifyOfPropertyChange<bool>(() => NowPlayingVisible);
                NotifyOfPropertyChange<bool>(() => ActiveItemVisible);
            }
        }

        public bool ActiveItemVisible { get { return !NowPlayingVisible; } }

        public ShellViewModel(IEnumerable<IAppWidget> widgets, IEventAggregator eventAgg)
        {
            eventAgg.Subscribe(this);
            Widgets = new BindableCollection<IAppWidget>(widgets.OrderBy(x => x.Name));
            ActivateWidget(Widgets.FirstOrDefault(a => a.Name == "Library"));
        }

        public void ActivateWidget(IAppWidget widget)
        {
            if (widget == null) return;
            ActivateItem(widget);
            NowPlayingVisible = false;
        }

        public void NowPlaying()
        {
            NowPlayingVisible = true;
        }

        public void Handle(PlayRequestMessage message)
        {
            NowPlaying();
        }
    }
}
