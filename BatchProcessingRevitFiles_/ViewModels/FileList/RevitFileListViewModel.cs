using System.Collections.ObjectModel;

namespace BatchProcessingRevitFiles
{
    public class RevitFileListViewModel : ViewModelBase
    {
        private static RevitFileListViewModel instance;
        public static RevitFileListViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RevitFileListViewModel();
                }
                return instance;
            }
        }

        /// <summary>
        /// Список файлов для обрабтки
        /// </summary>
        public ObservableCollection<RevitFileListItemViewModel> Items { get; set; }

        public RevitFileListViewModel()
        {
            Items = new ObservableCollection<RevitFileListItemViewModel>();
        }

        /// <summary>
        /// Метод для удаления строки
        /// </summary>
        /// <param name="file">Строка для удаления</param>
        internal void RemoveFile(RevitFileListItemViewModel file) => Items.Remove(file);
    }
}
