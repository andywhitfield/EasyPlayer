using System;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Caliburn.Micro;
using EasyPlayer.Persistence;

namespace EasyPlayer.Shell
{
    static class WindowPersistence
    {
        private static readonly ILog log = LogManager.GetLog(typeof(WindowPersistence));
        private const string WindowSettingsFilename = "window-settings.xml";

        public static void RestoreWindowState(this Bootstrapper bootstrapper, IPersistence persistence)
        {
            if (!bootstrapper.Application.IsRunningOutOfBrowser) return;
            var mainWindow = bootstrapper.Application.MainWindow;

            var windowInfo = persistence.ReadTextFile(WindowSettingsFilename);
            if (string.IsNullOrWhiteSpace(windowInfo)) return;

            try
            {
                var posAndSize = XDocument.Parse(windowInfo).Descendants("MainWindow").FirstOrDefault();
                if (posAndSize == null) return;

                if (bool.Parse(posAndSize.Attribute("IsMaximized").Value))
                    mainWindow.WindowState = WindowState.Maximized;
                else
                {
                    mainWindow.Width = int.Parse(posAndSize.Attribute("Width").Value);
                    mainWindow.Height = int.Parse(posAndSize.Attribute("Height").Value);
                    mainWindow.Top = int.Parse(posAndSize.Attribute("Top").Value);
                    mainWindow.Left = int.Parse(posAndSize.Attribute("Left").Value);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error setting window position from saved settings! {0}", ex);
                persistence.DeleteFile(WindowSettingsFilename);
            }
        }

        public static void SaveWindowState(this Bootstrapper bootstrapper, IPersistence persistence)
        {
            if (!bootstrapper.Application.IsRunningOutOfBrowser) return;
            var mainWindow = bootstrapper.Application.MainWindow;
            var xml = new XDocument(
              new XElement("WindowInfo",
                new XElement("MainWindow",
                  new XAttribute("IsMaximized", mainWindow.WindowState == WindowState.Maximized),
                  new XAttribute("Width", mainWindow.Width),
                  new XAttribute("Height", mainWindow.Height),
                  new XAttribute("Top", mainWindow.Top),
                  new XAttribute("Left", mainWindow.Left)
                )
              )
            );

            persistence.WriteTextFile(WindowSettingsFilename, xml.ToString(SaveOptions.DisableFormatting));
        }
    }
}
