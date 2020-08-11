using EU4_PCP.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static EU4_PCP_Frame.GlobVar;

namespace EU4_PCP_Frame
{
	public static class PCP_Implementations
	{
		#region Overrides and Helper Functions

		/// <summary>
		/// Appends an array of strings with a new <see cref="string"/>, growing the array if the last cell isn't empty. <br />
		/// [Emulates the List.Add() method for arrays.]
		/// </summary>
		/// <param name="arr">The array to be modified.</param>
		/// <param name="item">The item to be added.</param>
		public static void Add(ref string[] arr, string item)
		{
			if (arr[arr.Length - 1].Length > 0)
				Array.Resize(ref arr, arr.Length + 1);
			arr[arr.Length - 1] = item;
		}

		/// <summary>
		/// Checks if an integer is in a range.
		/// </summary>
		/// <param name="eval">The number to evaluate.</param>
		/// <param name="lLimit">Lower limit of the range.</param>
		/// <param name="uLimit">Upper limit of the range.</param>
		/// <returns><see langword="true"/> if the number is in range.</returns>
		public static bool Range(this int eval, int lLimit, int uLimit)
		{
			return eval >= lLimit && eval <= uLimit;
		}

		/// <summary>
		/// Converts the <see cref="string"/> representation of a number to its 32-bit signed integer equivalent. 
		/// <br />
		/// [An alias to int.Parse()]
		/// </summary>
		/// <param name="s">A string containing a number to convert.</param>
		/// <returns>A 32-bit signed integer equivalent to the number contained in s.</returns>
		public static int ToInt(this string s)
		{
			return int.Parse(s);
		}

		/// <summary>
		/// Converts a <see cref="string"/> array to <see cref="byte"/> array.
		/// </summary>
		/// <param name="s">The <see cref="string"/> array to convert.</param>
		/// <param name="res">The output <see cref="byte"/> array that will receive the result of the conversion.</param>
		/// <param name="startIndex">Starting index in the array</param>
		/// <returns><see langword="true"/> if the conversion was successful.</returns>
		public static bool ToByte(this string[] s, out byte[] res, int startIndex = 0)
		{
			res = new byte[s.Length];
			for (int i = startIndex; i < 3 + startIndex; i++)
			{
				if (!byte.TryParse(s[i], out res[i - startIndex]))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Converts a <see cref="string"/> array to <see cref="byte"/> array.
		/// </summary>
		/// <param name="s">The <see cref="string"/> array to convert.</param>
		/// <returns><see cref="byte"/> array that contains the result of the conversion.</returns>
		public static byte[] ToByte(this string[] s)
		{
			var res = new byte[s.Length];
			for (int i = 0; i < s.Length; i++)
			{
				res[i] = byte.Parse(s[i]);
			}
			return res;
		}

		/// <summary>
		/// Compares two numeric string using > (greater than) operator
		/// </summary>
		/// <param name="s">The left hand string.</param>
		/// <param name="other">The right hand string.</param>
		/// <returns><see langword="true"/> if the right hand string is greater than the left hand string.</returns>
		public static bool Gt(this string s, string other)
		{
			return s.ToInt() > other.ToInt();
		}

		/// <summary>
		/// Increments the numeric value of a <see cref="string"/> by a given value.
		/// </summary>
		/// <param name="s">The <see cref="string"/> to increment.</param>
		/// <param name="val">The value by which to increment.</param>
		public static void Inc(ref string s, int val)
		{
			int temp = s.ToInt();
			temp += val;
			s = temp.ToString();
		}

		/// <summary>
		/// Increments the numeric value of a <see cref="string"/> by a given value.
		/// </summary>
		/// <param name="s">The <see cref="string"/> to increment.</param>
		/// <param name="val">The value by which to increment.</param>
		/// <returns>The incremented <see cref="string"/>.</returns>
		public static string Inc(string s, int val)
		{
			int temp = s.ToInt();
			temp += val;
			return temp.ToString();
		}

		/// <summary>
		/// Gets the <see cref="Color"/> of a <see cref="Province"/> represented in a ProvTable row.
		/// </summary>
		/// <param name="row">The row containing the <see cref="Province"/> to parse.</param>
		/// <returns>a <see cref="Color"/> object representing the <see cref="Province"/> colors.</returns>
		public static Color FromRow(DataGridViewRow row)
		{
			// Uses the province index (ID column in the table), as the array index.
			return provinces[row.Cells[0].Value.ToString().ToInt()].Color;
		}

		/// <summary>
		/// Parses the <see cref="Province"/> name from the CSV string.
		/// </summary>
		/// <param name="list">Separated CSV line.</param>
		/// <returns>Parsed <see cref="Province"/> name.</returns>
		public static string DefinProvName(string[] list)
		{
			return list.Length switch
			{
				6 when list[5].Length > 1 => list[5].Trim(),
				6 when list[4].Length > 1 => list[4].Trim(),
				5 when list[4].Length > 1 => list[4].Trim(),
				_ => ""
			};
		}

		/// <summary>
		/// Converts LocScope to FileType.
		/// </summary>
		/// <param name="scope"><see cref="LocScope"/> enum.</param>
		/// <returns><see cref="FileType"/> enum.</returns>
		public static FileType FromLoc(LocScope scope) => scope switch
        {
            LocScope.Province => FileType.Province,
            LocScope.Bookmark => FileType.Bookmark,
            _ => throw new NotImplementedException()
        };

		/// <summary>
		/// Determines member scope by whether it contains the game path.
		/// </summary>
		/// <param name="member">The member of which to determine the scope.</param>
		public static void MemberScope(this MembersCount member)
        {
			member.Scope = 
				member.Path.Contains(Directory.GetParent(gamePath + locPath).FullName) ? 
				Scope.Game : Scope.Mod;
		}

        #endregion

        /// <summary>
        /// Creates the <see cref="Province"/> objects in the <see cref="provinces"/> array. <br />
        /// Initializes with index, color, and name from definition file.
        /// </summary>
        /// <param name="path">Root folder to work on.</param>
        /// <returns><see langword="false"/> if an exception occurs while trying to read from the definition file.</returns>
        public static bool DefinSetup(string path)
		{

			string[] dFile;
			try
			{
				dFile = File.ReadAllText(path + definPath, UTF7).Split(
					separators, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (Exception)
			{ return false; }

			Array.Resize(ref provinces, dFile.Length);
			Array.Clear(provinces, 0, dFile.Length);

			Parallel.ForEach(dFile, p =>
			{
				string[] list = p.Split(';');
				int i = -1;
				byte[] provColor = new byte[3];
				if (!(int.TryParse(list[0], out i) &&
					list.ToByte(out provColor, 1)))
					return;

				Province prov = new Province
				{
					Index = i,
					Color = new P_Color(provColor),
					DefName = DefinProvName(list),
					LocName = "",
					DynName = ""
				};

				provinces[i] = prov;
			});

			return true;
		}

		/// <summary>
		/// Dynamically prepares the localisation files.
		/// </summary>
		/// <param name="scope">Province or Bookmark.</param>
		public static bool LocPrep(LocScope scope)
		{
			bool readSuccess;

			string setting = scope switch
			{
				LocScope.Province => Settings.Default.ProvLocFiles,
				LocScope.Bookmark => Settings.Default.BookLocFiles,
				_ => "",
			};

			if (setting.Length > 0)
			{
				LocMembers(Mode.Read, scope);
				List<FileObj> filesList = new List<FileObj>();
				foreach (var member in members.Where(m => m.Type == scope))
				{
					if (!selectedMod && member.Scope == Scope.Mod)
						continue;
					filesList.Add(new FileObj(member.Path, FromLoc(scope)));
				}

				bool abort = false;
				for (int i = 0; i < filesList.Count; i++)
				{
					var lFile = filesList[i];
					var memberFiles = locFiles.Where(f => f == lFile);
					if (memberFiles?.Any() == false || (selectedMod &&
						!lFile.Path.Contains(Directory.GetParent(steamModPath + locPath).FullName)))
					{
						abort = true;
						break;
					}

					filesList[i] = memberFiles.First(); // For mod replacement files
					locFiles.Remove(memberFiles.First());
				}

				// If members were recovered successfully 
				if ((!abort && filesList.Count > 0) && NameSetup(filesList, scope, out readSuccess))
					return readSuccess;
			}

			// If there are no members in the settings, or the members have changed
			NameSetup(locFiles, scope, out readSuccess);
			if (readSuccess)
				LocMembers(Mode.Write, scope);

			return readSuccess;
		}

		/// <summary>
		/// Finds localisation names for Provinces and Bookmarks.
		/// </summary>
		/// <param name="lFiles">List of localisation files to work on.</param>
		/// <param name="scope">Province or Bookmark.</param>
		/// <param name="readSuccess"><see langword="false"/> if there was an error reading one of the files.</param>
		/// <returns><see langword="false"/> if the member count of the game was NOT according to the settings.</returns>
		private static bool NameSetup(List<FileObj> lFiles, LocScope scope, out bool readSuccess)
		{
			bool success = true;
			bool recall = members.Count(m => m.Type == scope) > 0;
			bool locSuccess = true;

			Regex locRE = scope switch // Select province or bookmark RegEx
			{
				LocScope.Province => locProvRE,

				// bookRE is a dynamic RegEx and thus isn't defined in PCP_Declarations.
				LocScope.Bookmark => new Regex($@"^ *({BookPattern()}):\d* *"".+""", RegexOptions.Multiline),
				_ => throw new NotImplementedException()
			};

			Parallel.ForEach(lFiles, locFile =>
			{
				string l_file;
				try
				{
					l_file = File.ReadAllText(locFile.Path, UTF7);
				}
				catch (Exception)
				{
					locSuccess = false;
					return;
				}

				var collection = locRE.Matches(l_file);
				if (recall)
				{
					var member = new MembersCount();
					var tempMembers = members.Where(m => Path.GetFileName(m.Path) == locFile.File);
					
					if (tempMembers.Any())
						member = tempMembers.First();

					if (!selectedMod && member && member.Count != collection.Count)
					{
						success = false;
						member.Count = collection.Count;
					}
				}
				else if (collection.Count > 0)
				{
					members.Add(new MembersCount
					{
						Count = collection.Count,
						Path = locFile.Path,
						Type = scope
					});
					members.Last().MemberScope();
				}

				foreach (Match prov in collection)
				{
					NameSelect(prov.Value, locFile.Path, scope);
				}
			});

			if (scope == LocScope.Bookmark &&
				!success &&
				bookmarks.Count(book => book.Name == null) == 0)
			{ success = true; }

			readSuccess = locSuccess;
			return success;
		}

		/// <summary>
		/// Calls a function to write the name of either a <see cref="Province"/> or a <see cref="Bookmark"/>.
		/// </summary>
		/// <param name="match">The RegEx match result.</param>
		/// <param name="path">Localisation file path.</param>
		/// <param name="scope">Province or Bookmark.</param>
		/// <returns><see langword="true"/> if the name was written.</returns>
		private static bool NameSelect(string match, string path, LocScope scope) => scope switch
		{
			LocScope.Province => NameProv(match, path),
			LocScope.Bookmark => NameBook(match, path),
			_ => false
		};

		/// <summary>
		/// Writes the localisation name to the matching <see cref="Province"/>.
		/// </summary>
		/// <param name="match">The RegEx match result. (Localisation file line)</param>
		/// <param name="path">Localisation file path.</param>
		/// <returns><see langword="true"/> if the name was written.</returns>
		private static bool NameProv(string match, string path)
		{
			string name = match.Split('"')[1].Trim();
			var provId = match.Split(':')[0].ToInt();

			if (name.Length < 1 || provId >= provinces.Length) return false;
			if (provinces[provId].LocName != "" && path.Contains(gamePath)) return false;

			provinces[provId].LocName = name;
			return true;
		}

		/// <summary>
		/// Writes the name to the matching <see cref="Bookmark"/>.
		/// </summary>
		/// <param name="match">The RegEx match result. (Bookmark name)</param>
		/// <param name="path">Localisation file path.</param>
		/// <returns><see langword="true"/> if the name was written.</returns>
		private static bool NameBook(string match, string path)
		{
			var tempBook = bookmarks.First(book => book.Code == bookLocCodeRE.Match(match).Value);
			if (selectedMod && tempBook.Name != null &&
			path.Contains(Directory.GetParent(gamePath + locPath).FullName))
				return false;
			tempBook.Name = locNameRE.Match(match).Value;

			return true;
		}

		/// <summary>
		/// A link between LocFiles <see cref="Settings"/> and <see cref="members"/> list.
		/// </summary>
		/// <param name="mode">Read from the <see cref="Settings"/>, or Write to the <see cref="Settings"/></param>
		/// <param name="scope">Province or Bookmark</param>
		public static void LocMembers(Mode mode, LocScope scope)
		{
			switch (mode)
			{
				case Mode.Read:
					string[] lines = { };
					
					lines = scope switch
					{
						LocScope.Province => Settings.Default.ProvLocFiles.Split(separators, StringSplitOptions.RemoveEmptyEntries),
						LocScope.Bookmark => Settings.Default.BookLocFiles.Split(separators, StringSplitOptions.RemoveEmptyEntries),
						_ => throw new NotImplementedException()
					};

					foreach (var member in lines.Where(l => l.Length > 5))
					{
						members.Add(new MembersCount(member.Split('|')));
						members.Last().MemberScope();
					}
					break;
				case Mode.Write:
					string join = "";
					foreach (var member in members.Where(m => m.Type == scope))
					{
						join += $"{member}\r\n";
					}
					join = join.Substring(0, join.Length - 2);
					switch (scope)
					{
						case LocScope.Province:
							Settings.Default.ProvLocFiles = join;
							break;
						case LocScope.Bookmark:
							Settings.Default.BookLocFiles = join;
							break;
						default:
							break;
					}
					break;
			}
		}

		/// <summary>
		/// Sets the owner object of each <see cref="Province"/> object in the 
		/// <see cref="provinces"/> array according to the start date.
		/// <param name="updateOwner">true to ignore not empty owner.</param>
		/// </summary>
		public static void OwnerSetup(bool updateOwner = false)
		{
			Parallel.ForEach(provFiles, p_file =>
			{
				var match = provFileRE.Match(p_file.File);
				if (!match.Success) return;
				int i = match.Value.ToInt();
				if (i >= provinces.Length) return;
				if (!updateOwner && provinces[i].Owner) return;

				string provFile = File.ReadAllText(p_file.Path);
				var currentOwner = LastEvent(provFile, EventType.Province);

				// Order is inverted to make handling of no result from LastEvent easier.
				// On the other hand, a successful LastEvent result removes the need for the ProvOwnerRE search.
				if (currentOwner == "")
				{
					match = provOwnerRE.Match(provFile);
					if (!match.Success) return;
					currentOwner = match.Value;
				}

				var owner = countries.Where(c => c.Code == currentOwner);
				if (owner.Any())
					provinces[i].Owner = owner.First();
			});
		}

		/// <summary>
		/// Checks validity of the line.
		/// </summary>
		/// <param name="line">The line to check.</param>
		/// <param name="eq"><see langword="true"/> to disable check for '='.</param>
		/// <returns><see langword="true"/> if the line should be skipped.</returns>
		public static bool NextLine(string line, bool eq = false)
		{
			line.Trim();
			if (line == "" || line[0] == '#') return true;
			return !eq && line.IndexOf('=') == -1;
		}

		/// <summary>
		/// Finds the result of the last relevant event in the file 
		/// (last owner for <see cref="Province"/>, last culture for <see cref="Country"/>).
		/// </summary>
		/// <param name="eFile">The file to be searched.</param>
		/// <param name="scope">Province or Country.</param>
		/// <returns>Last owner or last culture.</returns>
		private static string LastEvent(string eFile, EventType scope)
		{
			var lastDate = DateTime.MinValue;
			DateTime currentDate;
			MatchCollection eventMatch;
			Match match;
			var currentResult = "";

			eventMatch = scope switch
			{
				EventType.Province => provEventRE.Matches(eFile),
				EventType.Country => culEventRE.Matches(eFile),
				_ => throw new NotImplementedException(),
			};

			foreach (Match evnt in eventMatch)
			{
				currentDate = DateParser(evnt.Value.Split('=')[0].Trim());
				if (currentDate < lastDate) continue;
				if (currentDate > startDate) break;

				match = scope switch
				{
					EventType.Province => dateOwnerRE.Match(evnt.Value),
					EventType.Country => dateCulRE.Match(evnt.Value),
					_ => throw new NotImplementedException(),
				};

				if (!match.Success) continue;
				currentResult = match.Value;
				lastDate = currentDate;
				currentDate = DateTime.MinValue;
			}

			return currentResult;
		}

		/// <summary>
		/// Converts <see cref="string"/> to <see cref="DateTime"/>. Also handles years 2 - 999.
		/// </summary>
		/// <param name="str">The string to be parsed.</param>
		/// <returns>The parsed date as a <see cref="DateTime"/> object in case of successful conversion; <see cref="DateTime.MinValue"/> upon failure.</returns>
		public static DateTime DateParser(string str)
		{
			var pDate = DateTime.MinValue;
			DateTime.TryParseExact($"{str.Split('.')[0].ToInt() + 1000}{str.Substring(str.IndexOf('.'))}",
				EUDF, CultureInfo.InvariantCulture, DateTimeStyles.None, out pDate);

			if (pDate != DateTime.MinValue) pDate = pDate.AddYears(-1000);
			return pDate;
		}

		/// <summary>
		/// Creates the pattern for the multi-bookmark search <see cref="Regex"/>.
		/// </summary>
		/// <returns>The <see cref="Regex"/> pattern.</returns>
		private static string BookPattern()
		{
			string pattern = "";
			foreach (var book in bookmarks)
			{
				pattern += $"{book.Code}|";
			}
			return pattern.Substring(0, pattern.Length - 1);
		}

		#region Culture

		/// <summary>
		/// Creates the <see cref="Country"/> objects in the <see cref="countries"/> list. <br />
		/// Initializes with code and culture object.
		/// </summary>
		public static void CountryCulSetup()
		{
			object countryLock = new object();
			Parallel.ForEach(countryFiles, cFile =>
			{
				string code = cFile.File.Substring(0, 3);
				string countryFile = File.ReadAllText(cFile.Path);
				string priCul = "";
				var match = priCulRE.Match(countryFile);
				if (!match.Success) return;

				// Order is inverted to make handling of no result from LastEvent easier
				priCul = LastEvent(countryFile, EventType.Country);
				if (priCul == "") { priCul = match.Value; }

				if (updateCountries)
				{
					try
					{
						countries.First(c => c.Code == code).Culture =
						cultures.First(cul => cul.Name == priCul);
					}
					catch (Exception) { }
				}
				else
				{
					var cul = cultures.Where(cul => cul.Name == priCul);
					if (!cul.Any()) return;

					var country = new Country
					{
						Code = code,
						Culture = cul.First()
					};

					lock (countryLock)
					{ countries.Add(country); }
				}
			});

		}

		/// <summary>
		/// Creates the <see cref="Culture"/> objects in the <see cref="cultures"/> list. <br />
		/// Initializes with code and culture group object.
		/// </summary>
		/// <param name="culFile">The culture file to work on.</param>
		private static void CultureSetup(string culFile)
		{
			string[] cFile;
			try
			{
				cFile = File.ReadAllLines(culFile, UTF7);
			}
			catch (Exception) { return; }

			int brackets = 0;
			var cGroup = new Culture();
			foreach (string line in cFile)
			{
				if (NextLine(line, true)) continue;
				if (line.Contains('{'))
				{
					if (line.Contains('}')) continue;
					brackets++;
				}

				int eIndex = line.IndexOf('=');
				if (eIndex > -1 && brackets.Range(1, 2))
				{
					string temp = line.Substring(0, eIndex).Trim();
					if (!notCulture.Contains(temp))
					{

						Culture culture = new Culture
						{
							Name = temp
						};
						switch (brackets)
						{
							case 1:
								cGroup = culture;
								culture.Group = null;
								break;
							case 2:
								culture.Group = cGroup;
								break;
						}
						cultures.Add(culture);
					}
				}

				if (line.Contains('}') && brackets > 0)
					brackets--;
			}
		}

		/// <summary>
		/// Handles the culture setup, along with files preparations.
		/// </summary>
		public static void CulturePrep()
		{
			CulFilePrep();

			// Separate handling because parallel loop seems to work slower for one file.
			if (cultureFiles.Count == 1)
				CultureSetup(cultureFiles[0]);
			else if (cultureFiles.Count > 1)
			{
				Parallel.ForEach(cultureFiles, culFile =>
				{
					CultureSetup(culFile);
				});
			}
		}

		/// <summary>
		/// Gathers all <see cref="Culture"/> files into the <see cref="cultureFiles"/> list.
		/// </summary>
		private static void CulFilePrep()
		{
			if (selectedMod)
			{
				try
				{
					cultureFiles.AddRange(Directory.GetFiles(steamModPath + culturePath).ToList());
				}
				catch (Exception) { }
			}
			try
			{
				cultureFiles.AddRange(Directory.GetFiles(gamePath + culturePath).ToList());
			}
			catch (Exception) { }
		}

		#endregion

		/// <summary>
		/// Attaches (the contents of) each file from province names to the relevant <see cref="Country"/> / <see cref="Culture"/>.
		/// </summary>
		public static void ProvNameSetup()
		{
			Parallel.ForEach(provNameFiles, fileName =>
			{
				string[] nFile = File.ReadAllLines(fileName.Path, UTF7);
				string name = fileName.File.Split('.')[0];
				List<ProvName> names = new List<ProvName>();

				foreach (var line in nFile)
				{
					if (NextLine(line)) { continue; }
					var split = line.Split('=');
					names.Add(new ProvName { 
						Index = split[0].Trim().ToInt(), 
						Name = locNameRE.Match(split[1]).Value });
				}

				try
				{
					countries.First(cnt => cnt.Code == name).ProvNames = names.ToArray();
				}
				catch (Exception)
				{
					try
					{
						cultures.First(cul => cul.Name == name).ProvNames = names.ToArray();
					}
					catch (Exception) { }
					// For when there's a province name file that doesn't match any culture or country.
				}
			});
		}
		/// <summary>
		/// Finds the correct dynamic name source (owner / culture / group). <br />
		/// Also applies RNW / Unused policy.
		/// </summary>
		public static void DynamicSetup()
		{
			foreach (var prov in provinces)
			{
				if (!prov) { continue; }
				prov.DynName = "";
				if (!showRnw) { prov.IsRNW(); }
				if (!prov.Owner)
				{
					if (prov.Show
						&& prov.DefName.Length < 1
						&& prov.LocName.Length < 1)
					{ prov.Show = false; }
					continue;
				}
				if (DynamicName(prov, NameType.Country)) { continue; }
				if (!prov.Owner.Culture) { continue; }
				if (DynamicName(prov, NameType.Culture)) { continue; }
				if (prov.Owner.Culture.Group) { DynamicName(prov, NameType.Group); }
			}
		}

		/// <summary>
		/// Selects the owner / culture / culture group for the dynamic name.
		/// </summary>
		/// <param name="prov">The <see cref="Province"/> to select a dynamic name for.</param>
		/// <param name="mode"><see cref="Country"/> / <see cref="Culture"/> / Culture group.</param>
		/// <returns><see langword="true"/> if a name was successfully selected.</returns>
		private static bool DynamicName(Province prov, NameType mode)
		{
			ProvName[] source = mode switch
			{
				NameType.Country => prov.Owner.ProvNames,
				NameType.Culture => prov.Owner.Culture.ProvNames,
				NameType.Group => prov.Owner.Culture.Group.ProvNames,
				_ => throw new NotImplementedException()
			};

			if (source == null) return false;
			var query = source.Where(prv => prv.Index == prov.Index);
			if (query.Count() != 1) return false;
			prov.DynName = query.First().Name;
			return true;
		}

		/// <summary>
		/// Creates the <see cref="Bookmark"/> objects in the <see cref="bookmarks"/> array. <br /> 
		/// Initializes with code and date.
		/// </summary>
		public static void BookPrep()
		{
			foreach (var bookFile in bookFiles)
			{
				string bFile = File.ReadAllText(bookFile.Path);
				var codeMatch = bookmarkCodeRE.Match(bFile);
				var dateMatch = bookmarkDateRE.Match(bFile);
				if (!codeMatch.Success || !dateMatch.Success) { continue; }

				DateTime tempDate = DateParser(dateMatch.Value);
				if (tempDate == DateTime.MinValue) { continue; }
				bookmarks.Add(new Bookmark {
					Code = codeMatch.Value,
					StartDate = tempDate,
					DefBook = bookmarkDefRE.Match(bFile).Success
				});
			}
			if (bookmarks.Count == 0) return; 
			LocPrep(LocScope.Bookmark);
			SortBooks();
		}

		/// <summary>
		/// Sorts the <see cref="bookmarks"/> by date and removes bookmarks of the same date as the default one.
		/// </summary>
		private static void SortBooks()
		{
			bookmarks.Sort();
			startDate = bookmarks[0].StartDate;
			if (bookmarks.Count(book => book.DefBook) != 1) return;

			var tempBooks = bookmarks.ToArray();
			bookmarks.Clear();
			bookmarks.Add(new Bookmark());
			int counter = 1;

			for (int i = 1; i < tempBooks.Length; i++)
			{
				if (tempBooks[i] == tempBooks[i - 1])
					counter++;
				else if (counter > 1)
				{
					bookmarks[0] = tempBooks.First(b => b.DefBook);
					counter = 1;
					bookmarks.Add(tempBooks[i]);
				}
				else bookmarks.Add(tempBooks[i]);
			}
			if (counter > 1)
				bookmarks[0] = tempBooks.First(b => b.DefBook);
		}

		/// <summary>
		/// Prepares mod defines files.
		/// </summary>
		public static void FetchDefines()
		{
			if (!selectedMod) return;

			try
			{
				definesFiles = Directory.GetFiles(steamModPath + definesPath).ToArray().Where(
					f => definesFileRE.Match(f).Success).ToList();
			}
			catch (Exception) { }
			definesFiles.Add(steamModPath + definesLuaPath);
		}

		/// <summary>
		/// Handles multiple defines files parsing.
		/// </summary>
		public static void DefinesPrep()
		{
			if (selectedMod)
			{
				Parallel.ForEach(definesFiles, dFile =>
				{
					DefinesSetup(dFile);
				});
				if (startDate != DateTime.MinValue) return;
			}
			DefinesSetup(gamePath + definesLuaPath);
		}

		/// <summary>
		/// Parses the start date from a defines file.
		/// </summary>
		/// <param name="path">The defines file to parse.</param>
		private static void DefinesSetup(string path)
		{
			Match match;
			try
			{
				match = definesDateRE.Match(File.ReadAllText(path));
				startDate = DateParser(match.Value);
			}
			catch (Exception) { return; }
		}

		/// <summary>
		/// Reads the files from the mod folder, and creates the <see cref="ModObj"/> 
		/// objects in the <see cref="mods"/> list.
		/// </summary>
		public static void ModPrep()
		{
			var modLock = new object();
			string[] files;
			try
			{
				files = Directory.GetFiles(paradoxModPath).ToArray().Where(
					f => modFileRE.Match(f).Success).ToArray();
			}
			catch (Exception) { return; }

			Parallel.ForEach(files, modFile =>
			{
				string mFile = File.ReadAllText(modFile);
				var nameMatch = modNameRE.Match(mFile);
				var pathMatch = modPathRE.Match(mFile);
				var verMatch = modVerRE.Match(mFile);
				var remoteMatch = remoteModRE.Match(mFile).Success;

				if (!(nameMatch.Success && pathMatch.Success && verMatch.Success)) return;

				var modPath = pathMatch.Value;
				if (!remoteMatch)
				{
					if (Directory.Exists(Directory.GetParent(paradoxModPath).FullName + @"\" + modPath.TrimStart('/', '\\')))
						modPath = Directory.GetParent(paradoxModPath).FullName + @"\" + modPath.TrimStart('/', '\\');
					else return;
				}

				// With many mods in the folder, a context switch that will cause an OutOfRange exception is more likely
				lock (modLock)
				{
					mods.Add(new ModObj
					{
						Name = nameMatch.Value,
						Path = modPath,
						Ver = verMatch.Value,
						Replace = ReplacePrep(mFile)
					});
				}
			});

			mods.Sort();
		}

		/// <summary>
		/// Parses replace folders out of the mod file.
		/// </summary>
		/// <param name="rFile">The mod file to examine.</param>
		/// <returns>Folders to replace as a <see cref="Replace"/> object.</returns>
		private static Replace ReplacePrep(string rFile)
		{
			IEnumerable<string> vals;

			try
			{
				vals = ((IEnumerable<Match>)modReplaceRE.Matches(rFile)).Select(m => m.Value);
			}
			catch (Exception)
			{
				return new Replace();
			}

			return new Replace {
				Cultures = vals.Contains(culturesRep),
				Bookmarks = vals.Contains(bookmarksRep),
				ProvNames = vals.Contains(provNamesRep),
				Countries = vals.Contains(countriesRep),
				Provinces = vals.Contains(provincesRep),
				Localisation = vals.Contains(localisationRep)
			};
		}

		/// <summary>
		/// Checks presence of OneDrive and selects the correct Documents folder
		/// </summary>
		public static void DocsPrep()
		{
			if (Directory.Exists(oneDrivePath) && Directory.EnumerateFileSystemEntries(oneDrivePath).Any())
				selectedDocsPath = oneDrivePath;
			else
				selectedDocsPath = docsPath;
		}

		/// <summary>
		/// Selects mod path if a mod is selected, or game path if not.
		/// </summary>
		/// <returns>SteamModPath or GamePath.</returns>
		public static string DecidePath()
		{
			return selectedMod ? steamModPath : gamePath;
		}

		/// <summary>
		/// All array clearing.
		/// </summary>
		public static void ClearArrays()
		{
			members.Clear();
			cultureFiles.Clear();
			locFiles.Clear();
			countryFiles.Clear();
			countries.Clear();
			cultures.Clear();
			bookFiles.Clear();
			bookmarks.Clear();
			provFiles.Clear();
			duplicates.Clear();
		}

		#region File Fetching

		/// <summary>
		/// General function for fetching files from folders. <br />
		/// Handles game, regular mod, and replace mod folders.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <returns><see langword="false"/> if an exception occurred and the processing was unsuccessful.</returns>
		public static bool FetchFiles(FileType scope)
		{
			var filesList = SelectList(scope);
			IEnumerable<string> baseFiles, addFiles;

			try
			{
				baseFiles = ApplyRegEx(scope, Directory.GetFiles(PathRep(scope)));
			}
			catch (Exception) { return false; }

			if (!selectedMod || SelectReplace(scope))
			{
				filesList.AddRange(baseFiles.Select(f => new FileObj(f, scope)));
				return true;
			}

			try
			{
				addFiles = ApplyRegEx(scope, Directory.GetFiles(
					steamModPath + SelectFolder(scope), "*", SearchOption.AllDirectories));
			}
			catch (Exception) { return false; }

			filesList.AddRange(addFiles.Select(f => new FileObj(f, scope)));

			foreach (var b_file in baseFiles)
			{
				var TempFile = new FileObj(b_file, scope);
				if (ReplacedFile(scope, filesList.Where(c => c == TempFile))) { continue; }
				filesList.Add(TempFile);
			}

			return true;
		}

		/// <summary>
		/// Selects the path for the base files - game, or mod if relevant folder is to be replaced.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <returns>Path for base files.</returns>
		private static string PathRep(FileType scope)
		{
			string path;
			if (selectedMod && SelectReplace(scope))
				path = steamModPath;
			else path = gamePath;

			return path + SelectFolder(scope);
		}

		/// <summary>
		/// Folder replacement selector. <br />
		/// Can be called only when <see cref="selectedMod"/> is not <see langword="null"/>.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <returns><see langword="true"/> if the current scope requires replacement.</returns>
		private static bool SelectReplace(FileType scope)
		{
			var rep = selectedMod.Replace;
			switch (scope)
			{
				case FileType.Localisation when rep.Localisation:
				case FileType.Country when rep.Countries:
				case FileType.Bookmark when rep.Bookmarks:
				case FileType.Province when rep.Provinces:
				case FileType.ProvName when rep.ProvNames:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Converts scope to relevant path.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <returns>The path corresponding to the scope.</returns>
		private static string SelectFolder(FileType scope) => scope switch
		{
			FileType.Localisation => locPath,
			FileType.Country => histCountryPath,
			FileType.Bookmark => bookmarksPath,
			FileType.Province => histProvPath,
			FileType.ProvName => provNamesPath,
			_ => throw new NotImplementedException(),
		};

		/// <summary>
		/// Converts scope to relevant list.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <returns>The files list corresponding to the scope.</returns>
		private static List<FileObj> SelectList(FileType scope) => scope switch
		{
			FileType.Localisation => locFiles,
			FileType.Country => countryFiles,
			FileType.Bookmark => bookFiles,
			FileType.Province => provFiles,
			FileType.ProvName => provNameFiles,
			_ => throw new NotImplementedException(),
		};

		/// <summary>
		/// Applies RegEx on localisation files. <br />
		/// Other files are returned unchanged.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <param name="query">The file to process.</param>
		/// <returns>The input query.</returns>
		private static IEnumerable<string> ApplyRegEx(FileType scope, IEnumerable<string> query)
		{
			if (scope == FileType.Localisation)
				return query.Where(f => locFileRE.Match(f).Success);
			return query;
		}

		/// <summary>
		/// Checks if the query contains province name files or localisation files from /replace.
		/// </summary>
		/// <param name="scope">The type of the files to process.</param>
		/// <param name="query">The file to process.</param>
		/// <returns><see langword="true"/> for positive file count of: 
		/// country, bookmark, province, and replace localisation files. 
		/// <see langword="false"/> otherwise, and always for ProvName.</returns>
		private static bool ReplacedFile(FileType scope, IEnumerable<FileObj> query)
		{
			return scope switch
			{
				FileType.Localisation => RepLocCheck(query),
				FileType.Country when query.Count() > 0 => true,
				FileType.Bookmark when query.Count() > 0 => true,
				FileType.Province when query.Count() > 0 => true,
				FileType.ProvName => false,
				_ => false,
			};
		}

		/// <summary>
		/// Checks if the query contains localisation files from /replace.
		/// </summary>
		/// <param name="query"></param>
		/// <returns><see langword="true"/> if there is at least one replace file in the query.</returns>
		private static bool RepLocCheck(IEnumerable<FileObj> query)
		{
			return query.Count(f => f.Path.Contains(repLocPath)) > 0;
		}

		#endregion

		/// <summary>
		/// Handles all error messages.
		/// </summary>
		/// <param name="type">The error type</param>
		public static void ErrorMsg(ErrorType type) => _ = (type switch
		{
			ErrorType.DefinRead => MessageBox.Show("The definition.csv file is missing or corrupt",
				"Unable to parse definition file", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.DefinWrite => MessageBox.Show("The definition.csv file is inaccessible for writing",
				"Unable to access definition file", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.DefMapRead => MessageBox.Show("The default.map file is missing or corrupt",
				"Unable to parse default file", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.DefMapWrite => MessageBox.Show("The default.map file is inaccessible for writing",
				"Unable to access default file", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.DefMapMaxProv => MessageBox.Show("The 'default.map' file has no max_provinces definition!",
				"Missing max_provinces", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.LocFolder => MessageBox.Show("The localisation folder is missing or inaccessible",
				"Unable to access localisation files", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.LocRead => MessageBox.Show("One or more of the localisation files are missing or corrupt",
				"Unable to parse localisation file(s)", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.HistoryProvFolder => MessageBox.Show($"The {histProvPath} folder is missing or inaccessible",
				"Unable to access province history files", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.ValDate => MessageBox.Show("At least one bookmark, or a defines entry are required to determine the start date",
				"Unable to determine start date", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.GameExe => MessageBox.Show("Cannot find the game executable!",
				"Missing EXE", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.NoCultures => MessageBox.Show("Unable to find any cultures in the culture file(s), or the file(s) / folder are inaccessible",
				"Missing cultures", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.NoCulGroups => MessageBox.Show("Unable to find any culture groups in the culture file(s)",
				"No culture groups", MessageBoxButtons.OK, MessageBoxIcon.Error),
			ErrorType.NoCountries => MessageBox.Show("Unable to find the countries folder, or the files are inaccessible",
				"Missing countries", MessageBoxButtons.OK, MessageBoxIcon.Error),
			_ => throw new NotImplementedException(),
		});
	}
}
