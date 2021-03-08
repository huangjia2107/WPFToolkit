using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Test
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /*
        /// <summary>
        /// 注入
        /// </summary>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //Local Settings
            var localConfig = FileHelper.LoadFromJsonFile<GlobalConfig>(ResourcesMap.LocationDic[Location.GlobalConfigFile]);

            //version
            var version = ResourceAssembly.GetName().Version;
            var appData = new AppData
            {
                Version = $"{version.Major}.{version.Minor}.{version.Build}",
                Config = localConfig ?? new GlobalConfig()
            };

            Logger.Instance.Common.Info($"[ Version ] v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}");

            //Just for main app
            containerRegistry.RegisterInstance(appData);

            //For all modules
            containerRegistry.RegisterInstance(appData.Config);

            //ServiceCollection
            RegisterTypesByServiceCollection(containerRegistry);

            //Dialog
            containerRegistry.RegisterDialogWindow<DialogWindow>();
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>();

            //Command
            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
        }

        /// <summary>
        /// ServiceCollection
        /// </summary>
        private void RegisterTypesByServiceCollection(IContainerRegistry containerRegistry)
        {
            var services = new ServiceCollection();
            var action = (Action<HttpApiOptions>)(options => options.JsonDeserializeOptions.Converters.Add(new JsonNumberConverter()));

            //CAS Api
            services.AddHttpApi<IServerApi>(action);
            services.AddHttpApi<IEventApi>(action);
            services.AddHttpApi<IDAApi>(action);
            services.AddHttpApi<IAlgorithmTemplateApi>(action);
            services.AddHttpApi<IFileApi>(action);

            //MP Api
            services.AddHttpApi<IMpApi>();

            var sp = services.BuildServiceProvider();

            var container = containerRegistry.GetContainer();
            container.Register<IServiceScopeFactory, DryIocServiceScopeFactory>(Reuse.Singleton);
            container.Populate(services);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            var appData = Container.Resolve<AppData>();

            //Procdump
            //procdump -ma -h -e -o -64 -w CASApp.exe ..\Log
            if (appData.Config.CommonData.Procdump
                && Directory.Exists(ResourcesMap.LocationDic[Location.ProcdumpPath])
                && File.Exists(ResourcesMap.LocationDic[Location.ProcdumpBatFile]))
            {
                using (var p = new Process())
                {
                    p.StartInfo = new ProcessStartInfo
                    {
                        FileName = ResourcesMap.LocationDic[Location.ProcdumpBatFile],
                        CreateNoWindow = true,
                        WorkingDirectory = ResourcesMap.LocationDic[Location.ProcdumpPath]
                    };

                    p.Start();
                }
            }

            var regionManager = Container.Resolve<IRegionManager>();
            if (!regionManager.Regions.ContainsRegionWithName(Service.RegionNames.WorkName))
                return;

            var region = regionManager.Regions[Service.RegionNames.WorkName];
            var defaultActivateView = region.Views.FirstOrDefault(v =>
            {
                return (Attribute.GetCustomAttribute(v.GetType(), typeof(ViewSortHintAttribute)) as ViewSortHintAttribute) != null;
            });

            if (defaultActivateView != null)
                region.Activate(defaultActivateView);
        }

        */
    }
}
