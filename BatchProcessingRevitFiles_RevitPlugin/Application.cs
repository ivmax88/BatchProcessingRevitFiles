using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using BatchProcessingRevitFilesCore;
using Microsoft.AspNet.SignalR.Client;

namespace BatchProcessingRevitFiles_RevitPlugin
{
    public class Application : IExternalApplication
    {
        private HubConnection connection;
        private IHubProxy myHub;
        private Assembly assembly;
        private Document doc;
        private RevitCommand RevitCommand;

        public Result OnShutdown(UIControlledApplication application)
        {
            Hook.RemoveHook();
            application.ControlledApplication.FailuresProcessing -= ControlledApplication_FailuresProcessing;
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                connection = new HubConnection("http://127.0.0.1:49101/");
                myHub = connection.CreateHubProxy("MyHub");

                var connect = connection.Start();
                connect.Wait();

                application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
                application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
                application.DialogBoxShowing += Application_DialogBoxShowing;
                application.ControlledApplication.FailuresProcessing += ControlledApplication_FailuresProcessing;
                Hook.SetupHook();
            }
            catch (Exception)
            {
                //application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;
                //application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
                //application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened; ;
            }

            return Result.Succeeded;
        }

        private void ControlledApplication_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            var failList = e.GetFailuresAccessor().GetFailureMessages();
            if (failList.Any())
            {
                e.GetFailuresAccessor().DeleteAllWarnings();
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }

        private void Application_DialogBoxShowing(object sender, DialogBoxShowingEventArgs e)
        {
            try
            {
                // https://www.revitapidocs.com/2020/cb46ea4c-2b80-0ec2-063f-dda6f662948a.htm
                e.OverrideResult(1);
            }
            catch (Exception ex)
            {
                myHub.Invoke("SendError", Process.GetCurrentProcess().Id, ex.Message);
            }

        }

        private void ControlledApplication_DocumentOpening(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
            if (!e.PathName.Contains("BatchProcessingRevitFiles20"))
            {
                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.RevitFileOpening);
            }
        }

        private void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            this.doc = e.Document;
            if (e.Document.Title.StartsWith("BatchProcessingRevitFiles"))
            {
                RevitCommand = new RevitCommand(myHub);

                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.RevitStarted);

                myHub.On("OpenModel", path =>
                {
                    RevitCommand.Run(() =>
                    {
                        try
                        {
                            var app = new UIDocument(doc).Application.Application;
                            this.doc = app.OpenDocumentFile(path);

                        }
                        catch (Exception ex2)
                        {
                            myHub.Invoke("SendError", Process.GetCurrentProcess().Id, ex2.Message);
                        }

                    }, doc.Title);

                });

            }
            else
            {
                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.RevitFileOpened);

                myHub.On<string, string>("LoadScript", (path, libsPath) => StartScript(doc, path, libsPath));

            }
        }

        private void StartScript(Document doc, string path, string libsPath)
        {
            try
            {
                new DirectoryInfo(libsPath).GetFiles("*.dll", SearchOption.AllDirectories).ToList().ForEach(x => Assembly.LoadFrom(x.FullName));

                assembly = Assembly.LoadFrom(path);
                var type = assembly.GetTypes().Where(x => x.GetMethod("Execute") != null).FirstOrDefault();
                var m = type.GetMethod("Execute");

                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.ScriptStarted);
                Thread.Sleep(3000);
                RevitCommand.Run(() => m.Invoke(null, new object[] { doc, myHub }), doc.Title);
            }
            catch (Exception ex)
            {
                myHub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }





    }
}
