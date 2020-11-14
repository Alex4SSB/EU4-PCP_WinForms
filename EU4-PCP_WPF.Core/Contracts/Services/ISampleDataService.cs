using System.Collections.Generic;
using System.Threading.Tasks;

using EU4_PCP_WPF.Core.Models;

namespace EU4_PCP_WPF.Core.Contracts.Services
{
    public interface ISampleDataService
    {
        Task<IEnumerable<SampleOrder>> GetGridDataAsync();
    }
}
