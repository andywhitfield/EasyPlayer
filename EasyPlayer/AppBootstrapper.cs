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
using Caliburn.Micro;
using EasyPlayer.Shell;

namespace EasyPlayer
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        protected override void Configure()
        {
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            this.RestoreWindowState();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
            this.SaveWindowState();
        }
    }
}
