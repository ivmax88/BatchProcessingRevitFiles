using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BatchProcessingRevitFilesCore;

namespace BatchProcessingRevitFiles
{
    public class RevitFileListControlDesignModel : ViewModelBase
    {
        public static RevitFileListControlDesignModel Instance  => new RevitFileListControlDesignModel();

        public List<RevitFileListItemDesignModel> Items { get; set; }
        public RevitFileListControlDesignModel()
        {
            Items = new List<RevitFileListItemDesignModel>()
           {
               new RevitFileListItemDesignModel(){ Status= Status.ScriptFinished } ,
               new RevitFileListItemDesignModel(){ Status= Status.RevitFileClosed } ,
           };
        }
    }
}
