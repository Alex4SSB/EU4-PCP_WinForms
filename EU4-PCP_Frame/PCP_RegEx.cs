using System.Text.RegularExpressions;

namespace EU4_PCP
{
    public static class PCP_RegEx
	{
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
		public static readonly Regex newLineRE = new Regex("[\r\n]+");
	}

}
