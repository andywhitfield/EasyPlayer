using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using EasyPlayer.MediaControl;
using EasyPlayer.Messages;
using EasyPlayer.Widgets;

namespace EasyPlayer.Shell
{
    public class ShellViewModel : Conductor<IAppWidget>, IHandle<PlayRequestMessage>, IHandle<ActivateWidgetMessage>, IHandle<OutOfQuotaMessage>, IHandle<ApplicationUpdateAvailableMessage>
    {
        private IEventAggregator eventAgg;
        private IWindowManager windowMgr;
        private bool nowPlayingVisible = false;
        private NowPlayingViewModel nowPlaying;
        private NavigationBarViewModel navBar;

        public ShellViewModel(IEnumerable<IAppWidget> widgets, IEventAggregator eventAgg, IWindowManager windowMgr, NowPlayingViewModel nowPlaying, NavigationBarViewModel navBar)
        {
            this.eventAgg = eventAgg;
            this.nowPlaying = nowPlaying;
            this.navBar = navBar;
            this.windowMgr = windowMgr;

            eventAgg.Subscribe(this);
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

        public void Handle(PlayRequestMessage message) { NowPlayingWidget(); }
        public void Handle(ActivateWidgetMessage message) { ActivateWidget(message.Widget); }
        public void Handle(OutOfQuotaMessage message) { windowMgr.ShowDialog(new OutOfQuotaViewModel(eventAgg, message)); }
        public void Handle(ApplicationUpdateAvailableMessage message) { windowMgr.ShowNotification(new ApplicationUpdateViewModel(), 4500); }

        public void KeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                NowPlaying.PlayPause();
            else if (e.Key == Key.E && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                NavigationBar.StartSearch();
        }
    }
}
