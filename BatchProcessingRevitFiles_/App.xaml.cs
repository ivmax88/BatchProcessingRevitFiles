using System.Windows;
using BatchProcessingRevitFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Hosting;

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

            string url = "http://127.0.0.1:49101";

            WebApp.Start<Startup>(url);
            var c = BatchProcessingRevitFiles.Startup.Instance.Configurations;
            var view = BatchProcessingRevitFiles.Startup.Instance.Provider.GetRequiredService<MainWindow>();
            view.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
