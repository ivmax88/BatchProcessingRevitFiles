using System;
using System.Diagnostics;
using System.Reflection;
using Autodesk.Revit.DB;
using Microsoft.AspNet.SignalR.Client;
using TestLib;

namespace RevitScript
{
    /// <summary>
    /// Добавляет новый строковый параметр по эксемпляру
    /// </summary>
    public static class Script3
    {
        //Плагин ревита ищет статический метод Execute и выполняет его
        //Такой метод во всей сборке должен быть один
        //Метод принимает документ, открываемого файла и экземпляр хаба для передачи ошибок по Id текущего процесса
        public static void Execute(Document doc, IHubProxy hub)
        {
            try
            {
                if (!doc.IsFamilyDocument) return;

                using (var tr = new Transaction(doc, "add parameter"))
                {
                    tr.Start();
                    var man = doc.FamilyManager;
                    hub.Invoke("SendError", Process.GetCurrentProcess().Id, "before load dll");

                    Assembly.LoadFrom(@"C:\Users\ivanov.OLIMPROEKT\source\repos\BatchProcessingRevitFiles\TestLib\bin\Debug\TestLib.dll");
                    man.AddParameter(Testib.ParameterName, BuiltInParameterGroup.PG_TEXT, ParameterType.Text, true);
                    tr.Commit();
                }
                doc.Save();
                doc.Close();
            }
            catch (Exception ex)
            {
                hub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }

        }
    }

}
