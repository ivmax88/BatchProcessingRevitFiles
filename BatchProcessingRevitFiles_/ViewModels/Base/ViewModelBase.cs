using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BatchProcessingRevitFiles
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
