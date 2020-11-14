using System;

using EU4_PCP_WPF.Models;

namespace EU4_PCP_WPF.Contracts.Services
{
    public interface IThemeSelectorService
    {
        bool SetTheme(AppTheme? theme = null);

        AppTheme GetCurrentTheme();
    }
}
