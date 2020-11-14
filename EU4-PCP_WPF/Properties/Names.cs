using System.Collections.Generic;

namespace EU4_PCP_WPF
{
    public static class Names
    {
        public static Dictionary<string, string> GlobalNames = new Dictionary<string, string>(){
            { "GamePathFilter", "EU4 Executable|eu4.exe" },
            { "GamePathPlaceholder", "[not set]" },
            { "ModPathFilter", "mod file|*.mod" },
            { "ModPathPlaceholder", "[not set]" },
            { "ProvinceNamesDefault", "0" },
            { "AutoLoadDefault", "1" },
            { "ShowAllProvincesDefault", "0" },
            { "CheckDupliDefault", "0" }
        };
    }
}
