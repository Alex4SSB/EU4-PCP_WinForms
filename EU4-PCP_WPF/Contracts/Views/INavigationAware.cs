namespace EU4_PCP_WPF.Contracts.Views
{
    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);

        void OnNavigatedFrom();
    }
}
