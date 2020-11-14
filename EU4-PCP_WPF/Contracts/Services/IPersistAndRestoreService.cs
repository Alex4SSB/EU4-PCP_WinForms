namespace EU4_PCP_WPF.Contracts.Services
{
    public interface IPersistAndRestoreService
    {
        void RestoreData();

        void PersistData();
    }
}
