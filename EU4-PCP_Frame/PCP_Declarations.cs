using System;
using System.Drawing;
using System.Windows.Forms;

namespace EU4_PCP
{
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

		public string ToCsv()
		{
			return $"{R};{G};{B}";
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
			return $"{Index};{Color.ToCsv()};{DefName};x";
		}

		public bool IsRNW(bool updateShow = true)
		{
			var isRnw = PCP_RegEx.rnwRE.Match(DefName).Success;
			if (updateShow && isRnw)
				Show = false;
			return isRnw;
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

	public class ProvNameClass
	{
		public ProvName[] ProvNames;
	}

	public class Country : ProvNameClass
	{
		public string Code;
		public Culture Culture;

		public static implicit operator bool(Country obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Code;
		}
	}

	public class Culture : ProvNameClass
	{
		public string Name;
		public Culture Group;

		public static implicit operator bool(Culture obj)
		{
			return obj is object;
		}

		public override string ToString()
		{
			return Name;
		}

		public Culture() { }

		public Culture(string name)
		{
			Name = name;
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

	public class DupliProv
	{
		public Province Prov;
		public Label DupliLabel;

		public static implicit operator bool(DupliProv obj)
		{
			return obj is object;
		}

		public DupliProv(Province prov)
		{
			Prov = prov;
		}

		public override string ToString()
		{
			return Prov.ToString();
		}
	}

	public class Dupli
	{
		public DupliProv Dupli1;
		public DupliProv Dupli2;

		public Dupli(Province prov1, Province prov2)
		{
			Dupli1 = new DupliProv(prov1);
			Dupli2 = new DupliProv(prov2);
		}

		public override string ToString()
		{
			return $"{Dupli1} | {Dupli2}";
		}
	}

}
