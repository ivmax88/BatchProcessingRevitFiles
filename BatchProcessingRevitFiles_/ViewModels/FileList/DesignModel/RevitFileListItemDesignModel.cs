using System.Reflection;
using BatchProcessingRevitFilesCore;

namespace BatchProcessingRevitFiles
{
    public class RevitFileListItemDesignModel : ViewModelBase
    {
        public static RevitFileListItemDesignModel Instance => new RevitFileListItemDesignModel();
        public string FilePath { get; set; }
        public Status Status { get; set; }
        public int ProcessId { get; set; }
        public Assembly AssemblyToExecute { get; set; }
        public bool HasAssembly => AssemblyToExecute != null;

        public RevitFileListItemDesignModel()
        {
            FilePath = @"\\Diskstation\Производство\Ревит\REVIT_SETUP\12_Автоматизация\_ТЗ\Project1.rvt";
            Status = Status.RevitStarted;
        }
    }
}
