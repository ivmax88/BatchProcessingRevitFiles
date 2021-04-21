using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BatchProcessingRevitFilesCore;

namespace BatchProcessingRevitFiles
{
    public class RevitFileListItemViewModel : RevitFileListItemDesignModel
    {
        private Process process;

        /// <summary>
        /// Команда для удаления строки
        /// </summary>
        public RelayCommand Remove => new RelayCommand(() =>
        {
            RevitFileListViewModel.Instance.RemoveFile(this);
        });
        
        public RevitFileListItemViewModel()
        {
        }

        internal void RunProcess()
        {
            var file = new DirectoryInfo(Directory.GetCurrentDirectory()).GetFiles($"BatchProcessingRevitFiles{CommandsPanelViewModel.Instance.RevitVersion}.rvt", SearchOption.AllDirectories).ToList().FirstOrDefault();
            process = Process.Start(Startup.Instance.Configurations[$"RevitPath:{CommandsPanelViewModel.Instance.RevitVersion}"], file.FullName);
            ProcessId = process.Id;
            Status = Status.RevitStarting;

            ClearErorrs();
        }

        internal void KillRevit()
        {
            process?.Kill();
            Status = Status.RevitFileClosed;
        }
    }
}
