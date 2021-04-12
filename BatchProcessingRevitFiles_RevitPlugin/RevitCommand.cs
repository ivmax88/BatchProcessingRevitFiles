using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BatchProcessingRevitFilesCore;
using Microsoft.AspNet.SignalR.Client;

namespace BatchProcessingRevitFiles_RevitPlugin
{
    public class RevitCommand : IExternalEventHandler
    {
        private Document doc;
        private Selection sel;
        private UIApplication app;
        private readonly Autodesk.Revit.UI.ExternalEvent externalEvent;
        private readonly IHubProxy myHub;
        private Action action;
        private bool skipFailures;
        private string docTitle;

        public RevitCommand(IHubProxy myHub)
        {
            externalEvent = ExternalEvent.Create(this);
            this.myHub = myHub;
        }

        public void Execute(UIApplication app)
        {

            doc = app.ActiveUIDocument.Document;
            sel = app.ActiveUIDocument.Selection;
            this.app = app;
            try
            {

                if (skipFailures)
                    app.Application.FailuresProcessing += Application_FailuresProcessing;

                action();

                if (skipFailures)
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;


                if(!docTitle.StartsWith("BatchProcessingRevitFiles2019"))
                    myHub.Invoke("SendStatus", Process.GetCurrentProcess().Id, Status.ScriptFinished);

            }
            catch (Exception ex)
            {
                if (skipFailures)
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;

                myHub.Invoke("SendError", Process.GetCurrentProcess().Id, ex.Message);

            }
        }

        private void Application_FailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            // Inside event handler, get all warnings
            var failList = e.GetFailuresAccessor().GetFailureMessages();
            if (failList.Any())
            {
                // skip all failures
                e.GetFailuresAccessor().DeleteAllWarnings();
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }

        public void Run(Action action,  bool skipFailures, string docTitle)
        {
            this.action = action;
            this.skipFailures = skipFailures;
            this.docTitle = docTitle;
            externalEvent.Raise();
        }


        public string GetName()
        {
            return "RevitCommandEventHandler";
        }
    }
}
