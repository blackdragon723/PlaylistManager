using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using PlaylistManager.ApplicationServices;
using PlaylistManager.ApplicationServices.Services;
using PlaylistManager.ApplicationServices.Services.Interfaces;
using PlaylistManager.WPF.ViewModels;
using PlaylistManager.WPF.ViewModels.Interfaces;

namespace PlaylistManager.WPF
{
    public class Bootstrapper : BootstrapperBase
    {
        private IWindsorContainer _windsorContainer;
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _windsorContainer = new WindsorContainer();

            _windsorContainer.Register(Component.For<IWindowManager>().ImplementedBy<WindowManager>());
            _windsorContainer.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>());

            _windsorContainer.Register(
                Component.For<IMainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Is(LifestyleType.Singleton));

            _windsorContainer.Register(
                Component.For<ILibraryService>().ImplementedBy<LibraryService>().LifeStyle.Is(LifestyleType.Singleton));
            _windsorContainer.Register(
                Component.For<IPlaylistService>().ImplementedBy<PlaylistService>().LifeStyle.Is(LifestyleType.Singleton));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ? _windsorContainer.Resolve(service) : _windsorContainer.Resolve<object>(key, new { });
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _windsorContainer.ResolveAll(service).Cast<object>();
        }

        protected override void BuildUp(object instance)
        {
            instance.GetType().GetProperties()
                .Where(property => property.CanWrite && property.PropertyType.IsPublic)
                .Where(property => _windsorContainer.Kernel.HasComponent(property.PropertyType)).ToList()
                .ForEach(property => property.SetValue(instance, _windsorContainer.Resolve(property.PropertyType), null));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IMainViewModel>();
        }

    }
}
