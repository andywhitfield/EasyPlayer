using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Messages;
using EasyPlayer.Widgets;
using System.Windows.Input;
using EasyPlayer.MediaControl;

namespace EasyPlayer.Shell
{
    public class ShellViewModel : Conductor<IAppWidget>, IHandle<PlayRequestMessage>
    {
        private bool nowPlayingVisible = false;
        private NowPlayingViewModel nowPlaying;
        private NavigationBarViewModel navBar;

        public ShellViewModel(IEnumerable<IAppWidget> widgets, IEventAggregator eventAgg, NowPlayingViewModel nowPlaying, NavigationBarViewModel navBar)
        {
            eventAgg.Subscribe(this);
            this.nowPlaying = nowPlaying;
            this.navBar = navBar;
            Widgets = new BindableCollection<IAppWidget>(widgets.OrderBy(x => x.Name));
            ActivateWidget(Widgets.FirstOrDefault(a => a.Name == "Library"));
        }

        public BindableCollection<IAppWidget> Widgets { get; private set; }
        public IAppWidget ActiveWidget { get; set; }

        public NavigationBarViewModel NavigationBar { get { return navBar; } }

        public NowPlayingViewModel NowPlaying { get { return nowPlaying; } }
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

        public void ActivateWidget(IAppWidget widget)
        {
            if (widget == null) return;
            ActivateItem(widget);
            NowPlayingVisible = false;
        }

        public void NowPlayingWidget()
        {
            NowPlayingVisible = true;
        }

        public void Handle(PlayRequestMessage message)
        {
            NowPlayingWidget();
        }

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                NowPlaying.PlayPause();
            else if (e.Key == Key.E && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                NavigationBar.StartSearch();
        }
    }
}
