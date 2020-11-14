using EU4_PCP_WPF.Contracts.Views;
using EU4_PCP_WPF.Core.Contracts.Services;
using EU4_PCP_WPF.Core.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace EU4_PCP_WPF.Views
{
    public partial class ProvTablePage : Page, INotifyPropertyChanged, INavigationAware
    {
        private readonly ISampleDataService _sampleDataService;

        public ObservableCollection<SampleOrder> Source { get; } = new ObservableCollection<SampleOrder>();

        public ProvTablePage(ISampleDataService sampleDataService)
        {
            _sampleDataService = sampleDataService;
            InitializeComponent();
            DataContext = this;
        }

        public async void OnNavigatedTo(object parameter)
        {
            Source.Clear();

            // TODO WTS: Replace this with your actual data
            var data = await _sampleDataService.GetGridDataAsync();

            foreach (var item in data)
            {
                Source.Add(item);
            }
        }

        public void OnNavigatedFrom()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
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
