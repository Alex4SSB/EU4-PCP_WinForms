using System;
namespace EU4_PCP
{
    public static class PCP_Paths
    {
		// DOCS FOLDERS
		public static readonly string userFolder = $@"C:\Users\{Environment.UserName}";
		public static readonly string docsFolder = @"\Documents\Paradox Interactive\Europa Universalis IV";
		public static readonly string docsPath = $@"{userFolder}{docsFolder}";
		public static readonly string oneDrivePath = $@"{userFolder}\OneDrive{docsFolder}";
		public static string paradoxModPath = ""; // The folder that contains the .mod files
		public static string steamModPath = ""; // Current Mod - parsed from the .mod file
		public static string selectedDocsPath = ""; // OneDrive / Offline
		public static readonly string gameLogPath = @"\logs\game.log";

		// MAIN FOLDERS
		public static string gamePath = "";
		public static readonly string gameFile = @"\eu4.exe";
		public static readonly string definPath = @"\map\definition.csv";
		public static readonly string defMapPath = @"\map\default.map";
		public static readonly string locPath = @"\localisation";
		public static readonly string repLocPath = @"\localisation\replace";
		public static readonly string histProvPath = @"\history\provinces";
		public static readonly string histCountryPath = @"\history\countries";
		public static readonly string culturePath = @"\common\cultures";
		public static readonly string cultureFile = @"\00_cultures.txt";
		public static readonly string provNamesPath = @"\common\province_names";
		public static readonly string bookmarksPath = @"\common\bookmarks";
		public static readonly string definesPath = @"\common\defines";
		public static readonly string definesLuaPath = @"\common\defines.lua";

		// REPLACE FOLDERS
		public static readonly string culturesRep = "common/cultures";
		public static readonly string bookmarksRep = "common/bookmarks";
		public static readonly string provNamesRep = "common/province_names";
		public static readonly string countriesRep = "history/countries";
		public static readonly string provincesRep = "history/provinces";
		public static readonly string localisationRep = "localisation";
	}
}
