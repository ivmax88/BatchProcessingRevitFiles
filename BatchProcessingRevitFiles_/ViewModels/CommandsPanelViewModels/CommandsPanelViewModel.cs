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

        #region Singelton

        private static CommandsPanelViewModel instance;
        public static CommandsPanelViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CommandsPanelViewModel();
                }
                return instance;
            }
        }

        #endregion

        #region Commands

        public RelayCommand Run { get; set; }
        public RelayCommand LoadFile { get; set; }
        public RelayCommand LoadFolder { get; set; }
        public RelayCommand ShowCodeFileView { get; set; }


        #endregion

        #region Public properties

        public bool CommandsVisiblity { get; set; } = true;
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
            ShowCodeFileView = new RelayCommand(ShowCodeFileViewCommand);
        }

      

        #endregion

        /// <summary>
        /// Показывает окно для загрузки скрипта
        /// </summary>
        private void ShowCodeFileViewCommand()
        {
            ScriptLoadViewModel.Instance.ScriptLoadVisibility = true;
            CommandsVisiblity = false;
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
                var item = new RevitFileListItemViewModel()
                {
                    FilePath = filepath,
                    Status = Status.None
                };
                if (ScriptLoadViewModel.Instance.AssemblyToExecute != null)
                    item.AssemblyToExecute = ScriptLoadViewModel.Instance.AssemblyToExecute;
                RevitFileListViewModel.Instance.Items.Add(item);
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
