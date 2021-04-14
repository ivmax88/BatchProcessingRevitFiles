using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Cors;
using Owin;

namespace BatchProcessingRevitFiles
{
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
