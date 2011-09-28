using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Caliburn.Micro;

namespace EasyPlayer.Widgets
{
    public class LoggingViewModel : Screen, IAppWidget
    {
        private DispatcherTimer autoRefreshTimer;

        public LoggingViewModel()
        {
            autoRefreshTimer = new DispatcherTimer();
            autoRefreshTimer.Interval = TimeSpan.FromMilliseconds(500);
            autoRefreshTimer.Tick += (o, s) => RefreshLog();
        }

        public string Name
        {
            get { return "Output"; }
        }

        public string AutoRefreshText
        {
            get
            {
                return autoRefreshTimer.IsEnabled ? "Turn Auto Refresh Off" : "Turn Auto Refresh On";
            }
        }

        public void RefreshLog()
        {
            NotifyOfPropertyChange(() => LogMessages);
        }

        public void AutoRefreshLog()
        {
            if (autoRefreshTimer.IsEnabled) autoRefreshTimer.Stop();
            else autoRefreshTimer.Start();

            NotifyOfPropertyChange(() => AutoRefreshText);
        }

        public IEnumerable<string> LogMessages { get { return Logger.RecentLogMessages.Reverse(); } }
    }
}
