using System;
using System.Collections.Generic;

namespace EU4_PCP
{
	public static class PCP_Data
	{
		// BOOLEANS
		public static bool showRnw;
		public static bool enLoc;
		public static bool enDyn;
		public static bool updateCountries;
		public static bool lockdown = false;

		// GLOBAL OBJECTS
		public static ModObj selectedMod;
		public static DateTime beginTiming;
		public static DateTime finishTiming;
		public static DateTime startDate = DateTime.MinValue;

		// STRING LISTS
		public static List<string> definesFiles = new List<string>();
		public static List<string> cultureFiles = new List<string>();

		// CLASS ARRAYS AND LISTS
		public static Province[] provinces;
		public static List<Country> countries = new List<Country>();
		public static List<Culture> cultures = new List<Culture>();
		public static List<Bookmark> bookmarks = new List<Bookmark>();
		public static List<MembersCount> members = new List<MembersCount>();
		public static List<ModObj> mods = new List<ModObj>();
		public static List<FileObj> locFiles = new List<FileObj>();
		public static List<FileObj> countryFiles = new List<FileObj>();
		public static List<FileObj> bookFiles = new List<FileObj>();
		public static List<FileObj> provFiles = new List<FileObj>();
		public static List<FileObj> provNameFiles = new List<FileObj>();
		public static List<Dupli> duplicates = new List<Dupli>();
	}
}
