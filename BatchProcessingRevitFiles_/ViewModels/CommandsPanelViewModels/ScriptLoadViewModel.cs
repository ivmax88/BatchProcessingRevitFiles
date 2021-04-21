using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.CSharp;

namespace BatchProcessingRevitFiles
{
    /// <summary>
    /// View Model для кнопок
    /// </summary>
    public class ScriptLoadViewModel : ViewModelBase
    {
        private FileInfo[] references;

        #region Singelton

        private static ScriptLoadViewModel instance;
        public static ScriptLoadViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScriptLoadViewModel();
                }
                return instance;
            }
        }

        #endregion

        #region Public properties

        public bool ScriptLoadVisibility { get; set; }
        public bool IsCompile { get; set; }
        public string PathToCompileScript { get; set; }
        public string PathToLibraries { get; set; }
        public string PathToScriptFile { get; set; }
        public bool IsDll { get; set; } = true;
        public string PathToLibrariesForDll { get; set; }
        public string PathToDll { get; set; }
        public Assembly AssemblyToExecute { get; private set; }

        #endregion

        #region Commands
        public RelayCommand GetReadyScriptFolder { get; set; }
        public RelayCommand GetScriptFile { get; set; }
        public RelayCommand Compile { get; set; }
        public RelayCommand LoadReferencesFolder { get; set; }
        public RelayCommand LoadReferencesFolderForDll { get; set; }
        public RelayCommand LoadDll { get; set; }
        public RelayCommand Close => new RelayCommand(() =>
        {
            ScriptLoadVisibility = false;
            CommandsPanelViewModel.Instance.CommandsVisiblity = true;
        });

        #endregion


        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScriptLoadViewModel()
        {
            PathToCompileScript = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\script";
            PathToScriptFile = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\script.cs";
            PathToDll = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\script.dll";
            PathToLibraries = $@"{Directory.GetCurrentDirectory()}\Data\References\{CommandsPanelViewModel.Instance.RevitVersion}";
            PathToLibrariesForDll = $@"{Directory.GetCurrentDirectory()}\Data\References\{CommandsPanelViewModel.Instance.RevitVersion}";
            Compile = new RelayCommand(CompileCommand);
            LoadReferencesFolder = new RelayCommand(LoadReferencesFolderCommand);
            LoadReferencesFolderForDll = new RelayCommand(LoadReferencesFolderForDllCommand);
            LoadDll = new RelayCommand(LoadDllCommand);
            GetReadyScriptFolder = new RelayCommand(GetReadyScriptFolderCommand);
            GetScriptFile = new RelayCommand(GetScriptFileCommand);
        }

       


        #endregion


        #region Private methods
        private void GetScriptFileCommand()
        {
            var open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "C# (*.cs)|*.cs";
            open.FilterIndex = 1;

            if (open.ShowDialog() == true)
            {
                var file = open.FileName;
                PathToScriptFile = file;
            }
        }

        private void GetReadyScriptFolderCommand()
        {
            var open = new FolderBrowserDialog();
            open.RootFolder = Environment.SpecialFolder.Desktop;
            if (open.ShowDialog() == DialogResult.OK)
            {
                PathToCompileScript = open.SelectedPath;
            }
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
                PathToLibraries = open.SelectedPath;
            }
        }


        /// <summary>
        /// Открыват папку со связями для готового dll
        /// </summary>

        private void LoadReferencesFolderForDllCommand()
        {
            var open = new FolderBrowserDialog();
            open.RootFolder = Environment.SpecialFolder.Desktop;
            if (open.ShowDialog() == DialogResult.OK)
            {
                var dir = new DirectoryInfo(open.SelectedPath);
                references = dir.GetFiles("*.dll", SearchOption.AllDirectories);
                PathToLibrariesForDll = open.SelectedPath;
            }
        }


        /// <summary>
        /// Компилирует скрипт из текстового файла
        /// </summary>
        private void CompileCommand()
        {
            Directory.CreateDirectory(PathToCompileScript);

            var codeProvider = new CSharpCodeProvider();
            var parameters = new CompilerParameters
            {
                OutputAssembly = GetFileName(),
                WarningLevel = 3,

                // Set whether to treat all warnings as errors.
                TreatWarningsAsErrors = false,
                CompilerOptions = "/optimize"
            };

            if (references == null)
            {
                var dir = new DirectoryInfo(PathToLibraries);
                references = dir.GetFiles("*.dll", SearchOption.AllDirectories);
            }

            references.Select(x => x.FullName).ToList().ForEach(x =>
            {
                parameters.ReferencedAssemblies.Add(x);
            });
            var result = codeProvider.CompileAssemblyFromFile(parameters, PathToScriptFile);

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
                //MessageBox.Show(mess);
                new MessageBoxDialogViewModel(mess);
            }
            else
            {
                new MessageBoxDialogViewModel("Компиляция скрипта прошла успешно");
                //MessageBox.Show("Компиляция скрипта прошла успешно");
                foreach (var item in RevitFileListViewModel.Instance.Items)
                {
                    item.AssemblyToExecute = result.CompiledAssembly;
                }
                AssemblyToExecute = result.CompiledAssembly;
            }

        }

        private string GetFileName()
        {
            var name = "script";
            var suffix = ".dll";
            var files = new DirectoryInfo(PathToCompileScript).GetFiles($"script*.dll");

            if (files.Length == 0) return $"{PathToCompileScript}\\{name}1{suffix}";

            var numbers = files.ToList().Select(x =>
            {
                var n = x.Name.Replace("script", string.Empty);
                return n.Replace(".dll", string.Empty);
            }).Select(x => int.Parse(x)).OrderBy(x => x).ToList();

            var last = numbers.Last();
            var v = $"{PathToCompileScript}\\{name}{++last}{suffix}";
            return v;
        }

        /// <summary>
        /// Загружает готовый dll
        /// </summary>
        private void LoadDllCommand()
        {
            var open = new Microsoft.Win32.OpenFileDialog();
            open.Multiselect = false;
            open.Filter = "dll (*.dll)|*.dll";
            open.FilterIndex = 1;

            if (open.ShowDialog() == true)
            {
                var file = open.FileName;
                PathToDll = file;
                var assembly = Assembly.ReflectionOnlyLoadFrom(PathToDll);
                foreach (var item in RevitFileListViewModel.Instance.Items)
                {
                    item.AssemblyToExecute = assembly;
                }
                AssemblyToExecute = assembly;

                new MessageBoxDialogViewModel("Файл принят");
                //MessageBox.Show("Файл принят");
            }

        }

        #endregion


    }
}
