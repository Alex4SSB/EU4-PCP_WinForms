using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EU4_PCP_WPF.Views
{
    public partial class ColorPickerPage : Page, INotifyPropertyChanged
    {
        public ColorPickerPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
