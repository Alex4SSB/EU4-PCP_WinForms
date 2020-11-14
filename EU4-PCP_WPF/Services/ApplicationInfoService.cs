using System;
using System.Diagnostics;
using System.Reflection;

using EU4_PCP_WPF.Contracts.Services;

namespace EU4_PCP_WPF.Services
{
    public class ApplicationInfoService : IApplicationInfoService
    {
        public ApplicationInfoService()
        {
        }

        public Version GetVersion()
        {
            // Set the app version in EU4-PCP_WPF > Properties > Package > PackageVersion
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var version = FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            return new Version(version);
        }
    }
}
