using System.Drawing;
using System.Text;

namespace EU4_PCP
{
	public static class PCP_Const
	{
		// MISC
		public static readonly string APP_NAME = "EU4 Province Color Picker";
		public static readonly string APP_VER = "1.4.4";
		public static readonly string[] NOT_ENG = { "_l_french", "_l_german", "_l_spanish" };
		public static readonly string[] NOT_CUL = {
			"graphical_culture", "second_graphical_culture", "male_names", "female_names", "dynasty_names", "primary"};
		public static readonly string DATE_FORMAT = "dd/MM/yyyy";
		public static readonly string[] EUDF = {
			"yyyy.M.dd", "yyyy.MM.dd", "yyyy.M.d", "yyyy.MM.d"
		}; // EU Date Formats. The years 2 - 999 are interpreted falsely, and thus processed in the date parser

		// SYSTEM VARS
		public static readonly Size MARKER_SIZE = new Size(8, 5);
		public static readonly int MARKER_Y_OFFSET = 15;
		public static readonly int HEIGHT_OFFSET_SB = 34;
		public static readonly int WIDTH_SB = System.Windows.Forms.SystemInformation.VerticalScrollBarWidth;
		public static readonly Encoding UTF7 = Encoding.UTF7;
		public static readonly Encoding UTF8 = new UTF8Encoding(false);
		public static readonly string[] SEPARATORS = new string[] { "\n", "\r" };
	}
}
