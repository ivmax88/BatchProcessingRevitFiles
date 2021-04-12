using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BatchProcessingRevitFilesCore;
using Microsoft.CSharp;

namespace BatchProcessingRevitFiles
{
    /// <summary>
    /// View Model для кнопок
    /// </summary>
    public class CommandsPanelViewModel : ViewModelBase
    {
        private FileInfo[] references;

        public static CommandsPanelViewModel Instance => new CommandsPanelViewModel();

        #region Commands

        public RelayCommand Run { get; set; }
        public RelayCommand LoadFile { get; set; }
        public RelayCommand LoadFolder { get; set; }
        public RelayCommand LoadCodeFile { get; set; }
        public RelayCommand LoadReferencesFolder { get; set; }


        #endregion


        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public CommandsPanelViewModel()
        {
            Run = new RelayCommand(RunCommand);
            LoadFile = new RelayCommand(LoadFileCommand);
            LoadFolder = new RelayCommand(LoadFolderCommand);
            LoadCodeFile = new RelayCommand(LoadCodeFileCommand);
            LoadReferencesFolder = new RelayCommand(LoadReferencesFolderCommand);
        }

        /// <summary>
        /// Открыват папку со связями для компиляции скрипта
        /// </summary>
        private void LoadReferencesFolderCommand()
        {
            var open = new FolderBrowserDialog();
            open.RootFolder = Environment.SpecialFolder.Desktop;
            if (open.ShowDialog() == DialogResult.OK)
            {
                var dir = new DirectoryInfo(open.SelectedPath);
                references = dir.GetFiles("*.dll", SearchOption.AllDirectories);

            }
        }

        #endregion
        private void LoadCodeFileCommand()
        {
            var open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "C# (*.cs)|*.cs";
            open.FilterIndex = 1;

            if (open.ShowDialog() == true)
            {
                DeleteScriptFile();
                var file = open.FileNames;
                CSharpCodeProvider codeProvider = new CSharpCodeProvider();
                var parameters = new CompilerParameters
                {
                    OutputAssembly = GetFileName()
                };

                references.Select(x => x.FullName).ToList().ForEach(x =>
                {
                    parameters.ReferencedAssemblies.Add(x);
                });
                //parameters.ReferencedAssemblies.Add("System.Core.dll");
                //parameters.ReferencedAssemblies.Add("RevitAPI.dll");
                //parameters.ReferencedAssemblies.Add("RevitAPIUI.dll");
                //parameters.ReferencedAssemblies.Add("Microsoft.AspNet.SignalR.Client.dll"); 
                var result = codeProvider.CompileAssemblyFromFile(parameters, file);

                if (result.Errors.Count > 0)
                {
                    var mess = string.Empty;
                    foreach (CompilerError CompErr in result.Errors)
                    {
                        mess += "Line number " + CompErr.Line +
                                    ", Error Number: " + CompErr.ErrorNumber +
                                    ", '" + CompErr.ErrorText + ";" +
                                    Environment.NewLine + Environment.NewLine;
                    }
                    MessageBox.Show(mess);
                }
                else
                {
                    MessageBox.Show("Компиляция скрипта прошла успешно");
                    foreach (var item in RevitFileListViewModel.Instance.Items)
                    {
                        item.AssemblyToExecute = result.CompiledAssembly;
                    }
                }
            }
        }

        private string GetFileName()
        {
            var name = "script";
            var suffix = ".dll";
            var files = new FileInfo(Assembly.GetExecutingAssembly().FullName).Directory.GetFiles($"script*.dll");

            if (files.Length == 0) return $"{name}1{suffix}";

            var numbers = files.ToList().Select(x =>
            {
                var n = x.Name.Replace("script", string.Empty);
                return n.Replace(".dll", string.Empty);
            }).Select(x => int.Parse(x)).OrderBy(x => x).ToList();

            var last = numbers.Last();
            return $"{name}{++last}{suffix}";
        }


        /// <summary>
        /// Удаляет файл скрипта если он есть
        /// </summary>
        private void DeleteScriptFile()
        {
            var files = new FileInfo(Assembly.GetExecutingAssembly().FullName).Directory.GetFiles($"script*.dll");
            if (files.Length == 0) return;

            files.ToList().ForEach(x =>
            {
                try
                {
                    x.Delete();
                }
                catch { }
            });
        }


        /// <summary>
        /// Команда для добавления всех файлов из папки в список обработки
        /// </summary>
        private void LoadFolderCommand()
        {
            var open = new FolderBrowserDialog();
            open.RootFolder = Environment.SpecialFolder.Desktop;
            if (open.ShowDialog() == DialogResult.OK)
            {
                var dir = new DirectoryInfo(open.SelectedPath);
                var filesRvt = dir.GetFiles("*.rvt", SearchOption.AllDirectories);
                var filesRfa = dir.GetFiles("*.rfa", SearchOption.AllDirectories);
                filesRvt.ToList().ForEach(x => AddFileToFilesList(x.FullName));
                filesRfa.ToList().ForEach(x => AddFileToFilesList(x.FullName));
            }
        }

        /// <summary>
        /// Команда для добавления файлов в список обработки
        /// </summary>
        private void LoadFileCommand()
        {
            var open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = true;
            open.Filter = "rvt files (*.rvt)|*.rvt|rfa files (*.rfa)|*.rfa";
            open.FilterIndex = 1;

            if (open.ShowDialog() == true)
            {
                var files = open.FileNames;

                files.ToList().ForEach(x =>
                {
                    AddFileToFilesList(x);

                });

            }
        }

        /// <summary>
        /// Добавляет файл в список обработки
        /// </summary>
        /// <param name="filepath">Путь к файлу</param>
        private static void AddFileToFilesList(string filepath)
        {
            var list = RevitFileListViewModel.Instance.Items;

            if (!list.Any(f => f.FilePath == filepath))
            {
                RevitFileListViewModel.Instance.Items.Add(new RevitFileListItemViewModel()
                {
                    FilePath = filepath,
                    Status = Status.None
                });
            }
        }

        private void RunCommand()
        {


            foreach (var item in RevitFileListViewModel.Instance.Items)
            {
                if (item.AssemblyToExecute == null) continue;
                item.RunProcess();
            }
        }



    }
}
