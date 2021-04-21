using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchProcessingRevitFiles
{
    public class MessageBoxDialogViewModel : ViewModelBase
    {
        private MessageBoxDialog view;
        public RelayCommand Close { get; set; }
        public string   Message { get; set; }
        public MessageBoxDialogViewModel(string mess)
        {
            view = new MessageBoxDialog();
            view.DataContext = this;
            view.Owner= App.Current.MainWindow;
            view.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            Message = mess;
            Close = new RelayCommand(() => view.Close());
            view.ShowDialog();
        }
    }
}
