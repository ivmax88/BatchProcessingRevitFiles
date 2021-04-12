using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.AspNet.SignalR.Client;

namespace RevitScript
{
    public static class Script
    {
        public static void Execute(Document doc, IHubProxy hub)
        {
            try
            {
                hub.Invoke("SendError", Process.GetCurrentProcess().Id, "start script - " + doc.Title);

                using (var tr = new Transaction(doc, "create test wall"))
                {
                    tr.Start();
                    hub.Invoke("SendError", Process.GetCurrentProcess().Id, "start stransaction - " + doc.Title);

                    var levelid = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels).ToElementIds().FirstOrDefault();
                    Wall.Create(doc, Line.CreateBound(new XYZ(), new XYZ(1, 0, 0)), levelid, false);
                    tr.Commit();
                    hub.Invoke("SendError", Process.GetCurrentProcess().Id, "commit stransaction - " + doc.Title);

                }

                hub.Invoke("SendError", Process.GetCurrentProcess().Id, "end script - " + doc.Title);

                doc.Save();

                hub.Invoke("SendError", Process.GetCurrentProcess().Id, "save doc" + doc.Title);

            }
            catch (Exception ex)
            {
                hub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }

        }
    }
}
