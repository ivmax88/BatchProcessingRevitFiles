using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
namespace RevitScript
{
    /// <summary>
    /// Вычисляет объет всех стен по материалы
    /// Записывате на рабочий стол серелизованный файл класса <see cref="MaterialVolume"/>
    /// </summary>
    public static class Script2
    {
        //Плагин ревита ищет статический метод Execute и выполняет его
        //Такой метод во всей сборке должен быть один
        //Метод принимает документ, открываемого файла и экземпляр хаба для передачи ошибок по Id текущего процесса
        public static void Execute(Document doc, IHubProxy hub)
        {
            try
            {
                var volumes = new List<MaterialVolume>();
                var walls = new FilteredElementCollector(doc).WhereElementIsNotElementType()
                                                .OfCategory(BuiltInCategory.OST_Walls)
                                                .Cast<Wall>()
                                                .ToList();
                foreach (var wall in walls)
                {
                    var materials = wall.GetMaterialIds(false).Select(xx => doc.GetElement(xx) as Material);
                    materials.ToList().ForEach(m =>
                    {
                        var volume = wall.GetMaterialVolume(m.Id);
                        var e = volumes.FirstOrDefault(q => q.Id == m.Id.IntegerValue);
                        if (e == null)
                            volumes.Add(new MaterialVolume()
                            {
                                Id = m.Id.IntegerValue,
                                Name = m.Name,
                                Volume = UnitUtils.ConvertFromInternalUnits(volume, DisplayUnitType.DUT_CUBIC_METERS)
                            });
                        else
                            e.Volume += UnitUtils.ConvertFromInternalUnits(volume, DisplayUnitType.DUT_CUBIC_METERS);
                    });
                }

                var json = JsonConvert.SerializeObject(volumes, Formatting.Indented);
                File.WriteAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "\\объемы.txt"), json);
            }
            catch (Exception ex)
            {
                hub.Invoke("SendError", Process.GetCurrentProcess().Id, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }

        }
    }
    public class MaterialVolume
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Volume { get; set; }
    }
}
