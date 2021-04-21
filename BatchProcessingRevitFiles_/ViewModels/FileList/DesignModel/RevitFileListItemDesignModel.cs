using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using BatchProcessingRevitFilesCore;

namespace BatchProcessingRevitFiles
{
    public class RevitFileListItemDesignModel : ViewModelBase
    {
        private List<RevitError> errors = new List<RevitError>();

        public static RevitFileListItemDesignModel Instance => new RevitFileListItemDesignModel();
        public string FilePath { get; set; }
        public Status Status { get; set; }
        public int ProcessId { get; set; }
        public Assembly AssemblyToExecute { get; set; }
        public bool HasAssembly => AssemblyToExecute != null;
        public string ImageSourcePath => HasAssembly ? "/Images/RevitFileListItem/dll.png" : "/Images/RevitFileListItem/nodll.png";
        public bool NewError { get;private set; }

        public RelayCommand ShowErrors => new RelayCommand(() =>
        {
            var view = new ErrorsView(errors);
            var position = GetMousePosition();
            view.Left = position.X -40;
            view.Top = position.Y +15;
            NewError = false;
            view.ShowDialog();
        });



        public RevitFileListItemDesignModel()
        {
            FilePath = @"\\Diskstation\Производство\Ревит\REVIT_SETUP\12_Автоматизация\_ТЗ\Project1.rvt";
            Status = Status.RevitStarted;
        }


        #region Методы для работы со списком ошибок

        protected void ClearErorrs() => errors.Clear();

        internal void AddError(RevitError error)
        {
            errors.Add(error);
            NewError = true;
        }


        #endregion


        #region Координаты мыши
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        };
        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        #endregion
    }
}
