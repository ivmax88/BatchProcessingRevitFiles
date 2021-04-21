using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using BatchProcessingRevitFilesCore;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace BatchProcessingRevitFiles
{
    [HubName("MyHub")]
    public class MyHub : Hub
    {
        public MyHub()
        {

        }

        //public void OpenModel()
        //{
        //    var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
        //    context.Clients.All.OpenModel(@"C:\Users\ivanov.OLIMPROEKT\Desktop\ForTest\Связь.rvt");
        //}

        public void SendError(int id, string error)
        {
            var item = RevitFileListViewModel.Instance.Items.ToList().FirstOrDefault(x => x.ProcessId == id);
            item.AddError( new RevitError() { Error = error , ErrorTime = DateTime.Now});
        }

        public void SendStatus(int id, Status status)
        {
            var item = RevitFileListViewModel.Instance.Items.ToList().FirstOrDefault(x => x.ProcessId == id);

            if (item == null) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                item.Status = status;

            });
            switch (status)
            {
                case Status.None:
                    break;
                case Status.RevitStarting:
                    break;
                case Status.RevitStarted:
                    Clients.Caller.OpenModel(item.FilePath);
                    break;
                case Status.RevitFileOpening:
                    break;
                case Status.RevitFileOpened:
                    var pathToReferences = ScriptLoadViewModel.Instance.IsDll ? ScriptLoadViewModel.Instance.PathToLibrariesForDll : ScriptLoadViewModel.Instance.PathToLibraries;
                    Clients.Caller.LoadScript(item.AssemblyToExecute.Location, pathToReferences);
                    break;
                case Status.ScriptStarted:
                    break;
                case Status.ScriptFinished:
                    item.KillRevit();
                    break;
                case Status.RevitFileClosed:
                    break;
                default:
                    break;
            }
        }
        public override  Task OnConnected()
        {
            return base.OnConnected();
        }
       
    }
}
