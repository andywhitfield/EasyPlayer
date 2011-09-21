using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using EasyPlayer.Library.DefaultView;
using EasyPlayer.Messages;
using EasyPlayer.Persistence;
using EasyPlayer.Shell;

namespace EasyPlayer
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private IContainer container;

        public virtual T GetInstance<T>()
        {
            Debug.WriteLine("Getting instance of " + typeof(T).FullName);
            if (container == null) Configure();
            return (T)GetInstance(typeof(T), null);
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterAssemblyTypes(typeof(EventAggregator).Assembly, Assembly.GetExecutingAssembly())
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf()
                .Except<MediaItemView>();

            builder
                .RegisterType<MediaItemView>()
                .AsSelf();
            
            container = builder.Build();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            Debug.WriteLine("Getting instance of " + serviceType.FullName + " (key=" + key + ")");
            if (serviceType == null && !string.IsNullOrWhiteSpace(key))
            {
                serviceType = Type.GetType(key);
                key = null;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                if (container.IsRegistered(serviceType))
                    return container.Resolve(serviceType);
            }
            else
            {
                if (container.IsRegisteredWithName(key, serviceType))
                    container.ResolveNamed(key, serviceType);
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? serviceType.Name));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            Debug.WriteLine("Getting all instances of " + serviceType.FullName);
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            container.InjectProperties(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            this.RestoreWindowState(container.Resolve<IPersistence>());

            if (Application.Current.IsRunningOutOfBrowser && !Debugger.IsAttached)
            {
                Application.Current.CheckAndDownloadUpdateCompleted += (s1, e1) =>
                {
                    Debug.WriteLine("Application update checking completed. Is there an update? " + e1.UpdateAvailable);
                    if (!e1.UpdateAvailable) return;
                    var eventAgg = container.Resolve<IEventAggregator>();
                    eventAgg.Publish(new ApplicationUpdateAvailableMessage());
                };
                Application.Current.CheckAndDownloadUpdateAsync();
            }
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            base.OnExit(sender, e);
            container.Resolve<ShellViewModel>().NowPlaying.OnClose();
            this.SaveWindowState(container.Resolve<IPersistence>());
        }

        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                MessageBox.Show("Error in application: " + e.ExceptionObject);
        
            base.OnUnhandledException(sender, e);
        }
    }
}
