using System.Collections.Generic;
using System.Windows;

namespace BatchProcessingRevitFiles
{
    public class ErrorsListViewModel : ViewModelBase
    {
        private readonly Window window;
        public List<RevitError> Errors { get; set; }

        public RelayCommand Close => new RelayCommand(() => window.Close());
        public ErrorsListViewModel()
        {

        }
        public ErrorsListViewModel(Window window, List<RevitError> errors)
        {
            this.window = window;
            Errors = errors;
        }
    }
}
