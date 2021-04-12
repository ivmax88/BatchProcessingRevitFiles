using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BatchProcessingRevitFilesCore;
using Microsoft.AspNet.SignalR.Client;

namespace BatchProcessingRevitFiles_RevitPlugin
{
    public class Application : IExternalApplication
    {
        private IHubProxy myHub;
        private Assembly assembly;
        private Document doc;
        private RevitCommand RevitCommand;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                var connection = new HubConnection("http://127.0.0.1:8088/");
                myHub = connection.CreateHubProxy("MyHub");

                var connect = connection.Start();
                connect.Wait();

                application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
                application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened; ;
            }
            catch (Exception)
            {
                //application.ControlledApplication.ApplicationInitialized += ControlledApplication_ApplicationInitialized;
                //application.ControlledApplication.DocumentOpening += ControlledApplication_DocumentOpening;
                //application.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened; ;
            }

            return Result.Succeeded;
        }
        private void ControlledApplication_DocumentOpening(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
            if (!e.PathName.Contains("BatchProcessingRevitFiles2019"))
            {
                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.RevitFileOpening);
            }
        }

        private void ControlledApplication_DocumentOpened(object sender, Autodesk.Revit.DB.Events.DocumentOpenedEventArgs e)
        {
            this.doc = e.Document;
            if (e.Document.Title.StartsWith("BatchProcessingRevitFiles2019"))
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

                    }, false, doc.Title);

                });

            }
            else
            {
                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.RevitFileOpened);

                myHub.On("LoadScript", (Action<string>)(path =>
                {
                    StartScript(doc, path);

                }));
            }
        }

        private void StartScript(Document doc, string path)
        {
            try
            {
                assembly = Assembly.LoadFrom(path);
                var type = assembly.GetTypes().Where(x => x.GetMethod("Execute") != null).FirstOrDefault();
                var m = type.GetMethod("Execute");

                myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.ScriptStarted);
                Thread.Sleep(3000);
                RevitCommand.Run(() => m.Invoke(null, new object[] { doc, myHub }), true, doc.Title);
            }
            catch (Exception ex)
            {
                myHub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }

       



    }
}
