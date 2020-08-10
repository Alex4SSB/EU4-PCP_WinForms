using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EU4_PCP_Frame
{
	public static class GlobVar
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

		// REPLACE
		public static readonly string culturesRep = "common/cultures";
		public static readonly string bookmarksRep = "common/bookmarks";
		public static readonly string provNamesRep = "common/province_names";
		public static readonly string countriesRep = "history/countries";
		public static readonly string provincesRep = "history/provinces";
		public static readonly string localisationRep = "localisation";

		// MISC
		public static readonly string appName = "EU4 Province Color Picker";
		public static readonly string appVer = "1.4.2";
		public static readonly string[] notEnglish = { "_l_french", "_l_german", "_l_spanish" };
		public static readonly string[] notCulture = {
			"graphical_culture", "second_graphical_culture", "male_names", "female_names", "dynasty_names", "primary"};
		public static readonly string dateFormat = "dd/MM/yyyy";
		public static readonly string[] EUDF = {
			"yyyy.M.dd", "yyyy.MM.dd", "yyyy.M.d", "yyyy.MM.d"
		}; // EU Date Formats. The years 2 - 999 are interpreted falsely, and thus processed in the date parser

		// SYSTEM VARS
		public static readonly int widthSB = SystemInformation.VerticalScrollBarWidth;
		public static readonly Encoding UTF7 = Encoding.UTF7;
		public static readonly Encoding UTF8 = new UTF8Encoding(false);
		public static readonly string[] separators = new string[] { "\n", "\r" };

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

		// Global read-only RegEx patterns
		public static readonly Regex definesDateRE = new Regex(@"(?<=START_DATE *= *"")[\d.]+(?="")");
		public static readonly Regex definesFileRE = new Regex(@"\w+\.lua$");
		public static readonly Regex locNameRE = new Regex(@"(?<="").+(?="")");
		public static readonly Regex bookLocCodeRE = new Regex(@"\w+(?=:)");
		public static readonly Regex bookmarkCodeRE = new Regex(@"(?<=^\t*name *= *"")\w+(?="")", RegexOptions.Multiline);
		public static readonly Regex bookmarkDateRE = new Regex(@"(?<=^\t*date *= *)[\d.]+", RegexOptions.Multiline);
		public static readonly Regex bookmarkDefRE = new Regex(@"\t*default *= *yes", RegexOptions.Multiline);
		public static readonly Regex gameVerRE = new Regex(@"(?<=^Game Version: \w+ ).*(?=\.\w+)", RegexOptions.Multiline);
		public static readonly Regex provFileRE = new Regex(@"^[0-9]+(?=.*?$)");
		public static readonly Regex provOwnerRE = new Regex(@"(?<=^owner *= *)[A-Z]+", RegexOptions.Multiline);
		public static readonly Regex dateOwnerRE = new Regex(@"(?<=owner *= *)[A-Z][A-Z0-9]{2}");
		public static readonly Regex dateCulRE = new Regex(@"(?<=primary_culture *= *)\w+");
		public static readonly Regex provEventRE = new Regex(@"^\s*[\d.]* *= *{[^{]*owner[^{]*}", RegexOptions.Multiline);
		public static readonly Regex culEventRE = new Regex(@"^\s*[\d.]* *= *{[^{]*primary_culture[^{]*}", RegexOptions.Multiline);
		public static readonly Regex priCulRE = new Regex(@"(?<=^\s*primary_culture *= *)\w+", RegexOptions.Multiline);
		public static readonly Regex locProvRE = new Regex(@"(?<=^[ \t]*PROV)([0-9])+(:.*)", RegexOptions.Multiline);
		public static readonly Regex locFileRE = new Regex(@"\w+(english)\.yml$");
		public static readonly Regex maxProvRE = new Regex(@"(?<=^max_provinces *= *)\d+", RegexOptions.Multiline);
		public static readonly Regex defMapRE = new Regex(@"max_provinces.*");
		public static readonly Regex modFileRE = new Regex(@"\w+\.mod$");
		public static readonly Regex modNameRE = new Regex(@"(?<=^name *= *"").+?(?="")", RegexOptions.Multiline);
		public static readonly Regex modReplaceRE = new Regex(@"(?<=^replace_path *= *"")[\w /]+(?="")", RegexOptions.Multiline);
		public static readonly Regex modVerRE = new Regex(@"(?<=^supported_version *= *"")\d+(\.\d+)*", RegexOptions.Multiline);
		public static readonly Regex modPathRE = new Regex(@"(?<=^path *= *"")[\w /:]+(?="")", RegexOptions.Multiline);
		public static readonly Regex rnwRE = new Regex(@"(Unused(Land){0,1}\d+|RNW)");
		public static readonly Regex remoteModRE = new Regex("remote_file_id", RegexOptions.Multiline);
		public static readonly Regex newLineRE = new Regex(".*?[\r\n]");
	}

	public class P_Color
	{
		public byte R;
		public byte G;
		public byte B;
		public Color Color;

		public P_Color(byte[] provColor)
		{
			R = provColor[0];
			G = provColor[1];
			B = provColor[2];
			Color = Color.FromArgb(R, G, B);
		}

		public P_Color(params string[] stringColor)
		{
			byte[] provColor = new byte[3];
			stringColor.ToByte(out provColor);

			R = provColor[0];
			G = provColor[1];
			B = provColor[2];
			Color = Color.FromArgb(R, G, B);
		}

		public static implicit operator Color(P_Color c) => c.Color;

		public static implicit operator int(P_Color c) => c.Color.ToArgb();
	}

	public class Province
	{
		public int Index;
		public string DefName = "";
		public string LocName = "";
		public string DynName = "";
		public P_Color Color;
		public Country Owner;
		public bool Show = true;
		public int TableIndex;

		public static implicit operator bool(Province obj)
		{
			return obj is object;
		}

		public static implicit operator int(Province p)
		{
			return p.Index;
		}

		public override string ToString()
		{
			if (DynName.Length > 0) { return DynName; }
			if (LocName.Length > 0) { return LocName; }
			return DefName;
		}

		public string[] ToRow()
		{
			return new string[] {
				"",
				Index.ToString(),
				ToString(),
				Color.R.ToString(),
				Color.G.ToString(),
				Color.B.ToString() };
		}

		public string ToCsv()
		{
			return $"{Index};{Color.R};{Color.G};{Color.B};{DefName};x";
		}

		public void IsRNW()
		{
			if (GlobVar.rnwRE.Match(DefName).Success)
				Show = false;
		}
	}

	public class ProvName
	{
		public int Index;
		public string Name;

		public static implicit operator bool(ProvName obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class Country
	{
		public string Code;
		public Culture Culture;
		public ProvName[] ProvNames;

		public static implicit operator bool(Country obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Code;
		}
	}

	public class Culture
	{
		public string Name;
		public Culture Group;
		public ProvName[] ProvNames;

		public static implicit operator bool(Culture obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public class Bookmark : IComparable<Bookmark>
	{
		public string Code;
		public DateTime StartDate;
		public string Name;
		public bool DefBook;

		public static implicit operator bool(Bookmark obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			if (Name != null)
				return Name;

			return Code;
		}

		public int CompareTo(Bookmark other)
		{
			return StartDate.CompareTo(other.StartDate);
		}

		public override bool Equals(object obj)
		{
			return obj is Bookmark bookmark && StartDate == bookmark.StartDate;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(Bookmark left, Bookmark right)
		{
			return left.CompareTo(right) == 0;
		}

		public static bool operator !=(Bookmark left, Bookmark right)
		{
			return left.CompareTo(right) != 0;
		}
	}

	public class MembersCount
	{
		public string Path;
		public int Count;
		public LocScope Type;
		public Scope Scope;

		public static implicit operator bool(MembersCount obj)
		{
			return (obj is object && obj.Path != null);
		}

		public override string ToString()
		{
			return $"{Path}|{Count}|{(int)Type}";
		}

		public MembersCount(string[] member)
		{
			Path = member[0];
			Count = int.Parse(member[1]);
			Type = (LocScope)int.Parse(member[2]);
		}

		public MembersCount() { }
	}

	public class ModObj : IComparable<ModObj>
	{
		public string Name;
		public string Path;
		public string Ver; // Supported game version
		public Replace Replace;

		public static implicit operator bool(ModObj obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Name;
		}

		public int CompareTo(ModObj other)
		{
			return Name.CompareTo(other.Name);
		}

		public ModObj() { }

	}

	public class Replace
	{
		public bool Countries = false;
		public bool Provinces = false;
		public bool Cultures = false;
		public bool Bookmarks = false;
		public bool ProvNames = false;
		public bool Localisation = false;

		public static implicit operator bool(Replace obj)
		{
			return obj is object;
		}

		public Replace() { }
	}

	public class FileObj
	{
		public string Path;
		public string File;
		public FileType Scope;

		public static implicit operator bool(FileObj obj)
		{
			return obj is object;
		}

		public FileObj(string fPath, FileType scope)
		{
			Path = fPath;
			File = System.IO.Path.GetFileName(fPath);
			Scope = scope;
		}

		public static bool operator ==(FileObj left, FileObj right)
		{
			return left.File == right.File;
		}

		public static bool operator !=(FileObj left, FileObj right)
		{
			return left.File != right.File;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return Path;
		}
	}

	public class Dupli
	{
		public Province Prov1;
		public Province Prov2;

		public static implicit operator bool(Dupli obj)
		{
			return obj is object;
		}

		public Dupli(Province prov1, Province prov2)
		{
			this.Prov1 = prov1;
			this.Prov2 = prov2;
		}

		public string[] ToRow()
		{
			return new string[] { 
				Prov1.ToString(), 
				Prov2.ToString() };
		}
	}

}
