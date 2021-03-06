using System;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.AspNet.SignalR.Client;

namespace RevitScript
{
    /// <summary>
    /// Создает стену в начале координат
    /// </summary>
    public static class Script1
    {
        //Плагин ревита ищет статический метод Execute и выполняет его
        //Такой метод во всей сборке должен быть один
        //Метод принимает документ, открываемого файла и экземпляр хаба для передачи ошибок по Id текущего процесса
        public static void Execute(Document doc, IHubProxy hub)
        {
            try
            {
                using (var tr = new Transaction(doc, "create test wall"))
                {
                    tr.Start();

                    var levelid = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                        .OfCategory(BuiltInCategory.OST_Levels).ToElementIds().FirstOrDefault();
                    Wall.Create(doc, Line.CreateBound(new XYZ(), new XYZ(1, 0, 0)), levelid, false);
                    tr.Commit();
                }

                doc.Save();
            }
            catch (Exception ex)
            {
                hub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }

        }
    }
}
