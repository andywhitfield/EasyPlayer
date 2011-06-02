using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Autofac;
using Caliburn.Micro;
using EasyPlayer.Shell;
using System.Diagnostics;

namespace EasyPlayer
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private IContainer container;

        public virtual T GetInstance<T>()
        {
            if (container == null) Configure();
            return (T)GetInstance(typeof(T), null);
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly(), typeof(EventAggregator).Assembly)
                .SingleInstance()
                .AsImplementedInterfaces()
                .AsSelf();
            container = builder.Build();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
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
            return container.Resolve(typeof(IEnumerable<>).MakeGenericType(serviceType)) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            container.InjectProperties(instance);
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

        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                MessageBox.Show("Error in application: " + e.ExceptionObject);
        
            base.OnUnhandledException(sender, e);
        }
    }
}
