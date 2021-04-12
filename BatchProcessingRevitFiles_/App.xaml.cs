using System;
using System.IO;
using System.Windows;
using BatchProcessingRevitFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace BatchProcessingRevitFiles
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string url = "http://127.0.0.1:8088";

            WebApp.Start(url);

            var view = BatchProcessingRevitFiles.Startup.Instance.Provider.GetRequiredService<MainWindow>();
            view.Show();



        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }

    public class Startup
    {
        private readonly IConfiguration configurations;
        private readonly IServiceProvider provider;

        public IServiceProvider Provider => provider;
        public IConfiguration Configurations => configurations;

        public static Startup Instance { get; private set; }

        public Startup()
        {
            configurations = new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                         .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configurations);

            services.AddSingleton<MainWindow>();

            services.AddSingleton<MyHub>();

            provider = services.BuildServiceProvider();

            Instance = this;
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}
