using EU4_PCP_Frame.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DarkUI.Config;
using DarkUI.Forms;
using System.Windows.Forms;
using static EU4_PCP_Frame.GlobVar;
using static EU4_PCP_Frame.PCP_Implementations;

namespace EU4_PCP_Frame
{
	public partial class MainWin : DarkForm
	{
		public MainWin()
		{
			InitializeComponent();
		}

		private void MainWin_Load(object sender, EventArgs e)
		{
			Show();

			Critical(CriticalType.Begin);
			SettingsInit();
			bool Success = LaunchSequence();
			if (!Success) ClearScreen();
			Critical(CriticalType.Finish, Success);
		}

		/// <summary>
		/// Handles the beginning and finishing of (relatively) long-execution-time sections.
		/// </summary>
		/// <param name="Mode">Begin / Finish.</param>
		/// <param name="Success"><see langword="true"/> to update FinishTiming.</param>
		private void Critical(CriticalType Mode, bool Success) => Critical(Mode, Scope.Game, Success);

		/// <summary>
		/// Handles the beginning and finishing of (relatively) long-execution-time sections.
		/// </summary>
		/// <param name="Mode">Begin / Finish.</param>
		/// <param name="Scope">Game / Mod.</param>
		/// <param name="Success"><see langword="true"/> to update FinishTiming.</param>
		private void Critical(CriticalType Mode, Scope Scope = Scope.Game, bool Success = false)
		{
			Text = $"{appName} {appVer}";
			switch (Mode)
			{
				case CriticalType.Begin:
					beginTiming = DateTime.Now;
					Cursor = System.Windows.Forms.Cursors.WaitCursor;
					Text += " - Loading";
					if (Scope == Scope.Mod) Text += " mod";
					lockdown = true;
					break;
				case CriticalType.Finish:
					Cursor = System.Windows.Forms.Cursors.Default;
					if (Success) finishTiming = DateTime.Now;
					lockdown = false;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Implement all settings in the app.
		/// </summary>
		private void SettingsInit()
		{
			AutoLoadSM.Tag = "Radio";
			ProvNamesSM.Tag = "CheckBox";
			DuplicatesSM.Tag = "CheckBox";

			if (Settings.Default.AutoLoad == 0)
				DisableLoadMCB.State(true);
			if (Settings.Default.AutoLoad == 1)
				RemLoadMCB.State(true);
			if (Settings.Default.AutoLoad == 2)
				FullyLoadMCB.State(true);

			DefinNamesMCB.State(true);
			if (Settings.Default.ProvNames > 0)
			{
				LocNamesMCB.State(true);
				if (Settings.Default.ProvNames > 1)
				{
					DynNamesMCB.State(true);
					GameBookmarkCB.Enabled = true;
					ModBookmarkCB.Enabled = true;
				}
			}

			CheckDupliMCB.State(Settings.Default.ColorDupli);
			ShowAllProvsMCB.State(Settings.Default.ShowRNW);
			ShowAllProvsMCB.Enabled = true;
			if (ShowAllProvsMCB.State())
			{
				IgnoreRnwMCB.Enabled = CheckDupliMCB.State();
				IgnoreRnwMCB.State(Settings.Default.IgnoreRNW);
			}
			else IgnoreRnwMCB.State(true);
		}

		/// <summary>
		/// Sets relevant controls in the window to their default state.
		/// </summary>
		private void ClearScreen()
		{
			ProvTable.Rows.Clear();
			ModSelCB.SelectedIndex = -1;
			GameBookmarkCB.DataSource = null;
			GameBookmarkCB.Enabled = false;
			GameStartDateTB.Text = "";
			GameProvCountTB.Text = "";
			GameProvShownTB.Text = "";
			GameMaxProvTB.Text = "";
			GameMaxProvTB.BackColor = Colors.GreyBackground;
		}

		/// <summary>
		/// A sequence that runs on app start and when selecting/changing game/mod path.
		/// </summary>
		/// <returns>MainSequence result, or <see langword="false"/> ValGame result.</returns>
		private bool LaunchSequence()
		{
			if (ValGame()) return false;
			if (PathHandler(Scope.Mod, Mode.Read))
			{
				ModPrep();
				ModSelCB.Enabled = true;
				ModSelCB.Items.Clear();
				ModSelCB.Items.Add("[Vanilla - no mod]");
				ModSelCB.Items.AddRange(mods.Select(m => m.Name).ToArray());
			}
			else ModSelCB.Enabled = false;

			ModBrowseB.Enabled = true;
			if (FullyLoadMCB.State() &&
				ModSelCB.Items.Contains(Settings.Default.LastSelMod))
			{
				ModSelCB.SelectedItem = Settings.Default.LastSelMod;
				selectedMod = mods[ModSelCB.SelectedIndex - 1];
				steamModPath = selectedMod.Path;
			}
			else ModSelCB.SelectedIndex = 0;

			return MainSequence();
		}

		/// <summary>
		/// Checks game validity.
		/// </summary>
		/// <returns><see langword="true"/> upon failure or if auto-loading is disabled.</returns>
		private bool ValGame()
		{
			if (DisableLoadMCB.State() ||
				Settings.Default.GamePath.Length == 0 ||
				!PathHandler(Scope.Game, Mode.Read)) return true;
			gamePath = GamePathTB.Text;
			DocsPrep();

			return false;
		}

		/// <summary>
		/// Handles all path validation for game and mod.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		/// <param name="Mode">Read from settings or Write to settings.</param>
		/// <returns><see langword="true"/> if the validation was successful.</returns>
		private bool PathHandler(Scope Scope, Mode Mode)
		{
			string Setting = "";
			TextBox Box = null;
			switch (Scope)
			{
				case Scope.Game:
					Setting = Settings.Default.GamePath;
					Box = GamePathTB;
					break;
				case Scope.Mod:
					Setting = Settings.Default.ModPath;
					Box = ModPathTB;
					break;
				default:
					break;
			}

			if (Setting.Contains('|'))
				Setting = Setting.Split('|')[0];

			switch (Mode)
			{
				case Mode.Read:
					switch (Scope)
					{
						case Scope.Game when !File.Exists(Setting + gameFile):
							ErrorMsg(ErrorType.GameExe);
							SetWr(Scope, "");
							return false;
						case Scope.Mod:
							if (Setting.Length < 1)
							{
								if (Directory.Exists($@"{selectedDocsPath}\mod"))
								{
									paradoxModPath = $@"{selectedDocsPath}\mod";
									Setting = paradoxModPath;
								}
								else return false;
							}
							else paradoxModPath = Setting;
							break;
						default:
							break;
					}
					Box.Text = Setting;
					break;
				case Mode.Write:
					SetWr(Scope, Box.Text);
					break;
				default:
					break;
			}
			return true;
		}

		/// <summary>
		/// Writes to the game / mod path settings.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		/// <param name="s">The string to store in the settings.</param>
		private void SetWr(Scope Scope, string s)
		{
			switch (Scope)
			{
				case Scope.Game:
					Settings.Default.GamePath = s;
					break;
				case Scope.Mod:
					Settings.Default.ModPath = s;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Includes all essential sequences that run every time, except when changing bookmarks.
		/// </summary>
		/// <returns><see langword="false"/> if any of the sub-sequences fails.</returns>
		private bool MainSequence()
		{
			enLoc = LocNamesMCB.State();
			enDyn = DynNamesMCB.State();
			showRnw = ShowAllProvsMCB.State();
			updateCountries = false;
			ClearArrays();

			if (BookStatus(true) && !DefinSetup(DecidePath()))
			{
				ErrorMsg(ErrorType.DefinRead);
				return false;
			}

			if (!enDyn)
			{
				FetchDefines();
				DefinesPrep();
				if (!ValDate()) return false;
			}

			if (!LocalisationSequence()) return false;
			if (!enDyn) DynamicSetup();
			PopulateTable();
			GameVer();

			if (selectedMod)
			{
				ModStartDateTB.Text = startDate.ToString(dateFormat);
				ModInfoGB.Text = $"Mod - {selectedMod.Ver}";
				CountProv(Scope.Mod);
				PopulateBooks(Scope.Mod);
				EnableBooks(Scope.Mod);
			}
			else
			{
				GameStartDateTB.Text = startDate.ToString(dateFormat);
				ModInfoGB.Text = "Mod";
				CountProv(Scope.Game);
				PopulateBooks(Scope.Game);
				EnableBooks(Scope.Game);
			}

			// Unless a bookmark is selected, perform relevant max_provinces check
			if (BookStatus(true) &&
				(!selectedMod && MaxProvinces(Scope.Game)) ||
				(selectedMod && MaxProvinces(Scope.Mod)))
				return false;

			ClearCP(); // Clear the color picker, and call the randomizer

			DupliPrep();
			if (DupliTable.Rows.Count > 0)
				ColorDupliGB.Visible = true;
			else ColorDupliGB.Visible = false;

			return true;
		}

		/// <summary>
		/// A mini function for the localisation sequence and the calling of the dynamic sequence.
		/// </summary>
		/// <returns>DynamicSequence result.</returns>
		private bool LocalisationSequence()
		{
			if (!enLoc) return true;

			if (!FetchFiles(FileType.Localisation))
			{
				ErrorMsg(ErrorType.LocFolder);
				return false;
			}
			LocPrep(LocScope.Province);
			if (!locSuccess)
			{
				ErrorMsg(ErrorType.LocRead);
				return false;
			}

			return DynamicSequence();
		}

		/// <summary>
		/// The full sequence of calculating the dynamic province names.
		/// </summary>
		/// <returns><see langword="false"/> on failure.</returns>
		private bool DynamicSequence()
		{
			if (!enDyn) return true;
			bool EnBooks = BookStatus(false);
			bool Success = true;

			Parallel.Invoke(
				() => CulturePrep(),
				() => FetchFiles(FileType.Country));

			if (cultures.Count < 1)
			{
				ErrorMsg(ErrorType.NoCultures);
				return false;
			}
			else if (cultures.Where(cul => cul.Group).Count() < 1)
			{
				ErrorMsg(ErrorType.NoCulGroups);
				return false;
			}

			Parallel.Invoke(
				() => CountryCulSetup(),
				() => FetchDefines());

			if (countries.Count < 1)
			{
				ErrorMsg(ErrorType.NoCountries);
				return false;
			}

			Parallel.Invoke(
				() => DefinesPrep(),
				() => { if (EnBooks) FetchFiles(FileType.Bookmark); });

			Parallel.Invoke(
				() => { if (EnBooks) BookPrep(); },
				() => Success = FetchFiles(FileType.Province));

			if (!Success)
			{
				ErrorMsg(ErrorType.HistoryProvFolder);
				return false;
			}
			if (!ValDate()) return false;

			Parallel.Invoke(
				() => OwnerSetup(),
				() => FetchFiles(FileType.ProvName));

			ProvNameSetup();
			DynamicSetup();

			return true;
		}

		/// <summary>
		/// Clears the Color Picker GB. <br />
		/// Calls: <br />
		/// * Successful add province <br />
		/// * Unsuccessful add province <br />
		/// * Substantial provinces reload (not bookmark change)
		/// </summary>
		private void ClearCP()
		{
			NextProvNameTB.Text = "";
			if (!ColorPickerGB.Enabled)
			{
				RedTB.Text = "0";
				GreenTB.Text = "0";
				BlueTB.Text = "0";
				GenColL.BackColor = Color.Black;
				NextProvNumberTB.Text = "";
			}
			else if (selectedMod) RndPrep();
		}

		/// <summary>
		/// Generates an exclusive random color, that doesn't exist in the provinces table.
		/// </summary>
		private void RndPrep()
		{
			var rnd = new Random();
			int R, G, B;
			var TempColor = new Color();

			do
			{
				R = rnd.Next(0, 255);
				G = rnd.Next(0, 255);
				B = rnd.Next(0, 255);
				TempColor = Color.FromArgb(R, G, B);
			} while (provinces.Count(p => p && p.Color == TempColor) > 0);

			RedTB.Text = R.ToString();
			GreenTB.Text = G.ToString();
			BlueTB.Text = B.ToString();
			GenColL.BackColor = TempColor;
			NextProvNumberTB.Text = provinces.Length.ToString();
		}


		/// <summary>
		/// Checks if the start date is greater than 01/01/0001, otherwise prompts.
		/// </summary>
		/// <returns><see langword="true"/> if the date is valid.</returns>
		private bool ValDate()
		{
			if (startDate > DateTime.MinValue) return true;
			ErrorMsg(ErrorType.ValDate);
			return false;
		}

		/// <summary>
		/// Default file max provinces.
		/// </summary>
		/// <param name="Scope"></param>
		/// <returns><see langword="true"/> upon failure.</returns>
		private bool MaxProvinces(Scope Scope)
		{
			var FilePath = gamePath;
			if (Scope == Scope.Mod) FilePath = steamModPath;

			string d_file;
			try
			{
				d_file = File.ReadAllText(FilePath + defMapPath, UTF8);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefMapRead);
				return true;
			}
			var Match = maxProvRE.Match(d_file);

			if (!Match.Success)
			{
				switch (Scope)
				{
					case Scope.Game:
						GameMaxProvTB.Text = "";
						break;
					case Scope.Mod:
						ModMaxProvTB.Text = "";
						ModSelCB.SelectedIndex = -1;
						break;
					default:
						break;
				}
				ErrorMsg(ErrorType.DefMapMaxProv);
				return true;
			}

			switch (Scope)
			{
				case Scope.Game:
					GameMaxProvTB.Text = Match.Value;
					break;
				case Scope.Mod:
					ModMaxProvTB.Text = Match.Value;
					break;
				default:
					break;
			}

			// Update TB colors
			ProvCountColor();

			return false;
		}

		/// <summary>
		/// Handles colorization of province count textboxes.
		/// </summary>
		private void ProvCountColor()
		{
			if (GameMaxProvTB.Text.Length > 0 && !selectedMod)
			{
				GameMaxProvTB.BackColor = GameMaxProvTB.Text.Gt(GameProvCountTB.Text)
					? Color.DarkOliveGreen : Color.Maroon;
			}

			if (ModMaxProvTB.Text.Length > 0)
			{
				ModMaxProvTB.BackColor = ModMaxProvTB.Text.Gt(ModProvCountTB.Text)
					? Color.DarkOliveGreen : Color.Maroon;

				ModProvCountTB.BackColor = ModProvCountTB.Text.Gt(GameProvCountTB.Text)
					? Colors.GreyBackground : Color.Maroon;
			}
		}

		/// <summary>
		/// Prepare the table of duplicate provinces.
		/// </summary>
		private void DupliPrep()
		{
			if (!CheckDupliMCB.State() || !selectedMod)
			{
				DupliTable.Rows.Clear();
				return;
			}
			var Colors = new int[provinces.Length];
			var Indexes = new int[provinces.Length];

			for (int Prov = 0; Prov < provinces.Length; Prov++)
			{
				if (!provinces[Prov]) continue;
				Colors[Prov] = provinces[Prov].Color;
				Indexes[Prov] = provinces[Prov];
			}

			Array.Sort(Colors, Indexes);
			var empty = Colors.Count(c => c == 0);
			if (Colors[0] == 0) // Shouldn't happen, but just in case
			{
				Array.Reverse(Colors);
				Array.Reverse(Indexes);
				Array.Resize(ref Colors, Colors.Length - empty);
				Array.Resize(ref Indexes, Indexes.Length - empty);
				Array.Reverse(Colors);
				Array.Reverse(Indexes);
			}
			else // Default case
			{
				Array.Resize(ref Colors, Colors.Length - empty);
				Array.Resize(ref Indexes, Indexes.Length - empty);
			}

			if (Colors.Distinct().Count() == Indexes.Length) return; // No duplicates
			for (int Prov = 0; Prov < Colors.Length; Prov++)
			{
				if (Colors[Prov] != Colors[Prov - 1]) continue;
				duplicates.Add(new Dupli(provinces[Indexes[Prov]], provinces[Indexes[Prov - 1]]));
			}

			DupliTable.RowCount = duplicates.Count;
			for (int Prov = 0; Prov < duplicates.Count; Prov++)
			{
				DupliTable.Rows[Prov].SetValues(duplicates[Prov].ToRow());
			}
		}

		/// <summary>
		/// Decide whether to update the bookmarks - if a bookmark is selected.
		/// <br /> <br />
		/// The function is used to prevent updating the bookmarks CBs when a bookmark is selected,
		/// thus preventing the start date from changing, which in turn prevents useless
		/// activation of some sequences again.
		/// <br />
		/// Optionally combined with enabled bookmarks check, to disable some sequences
		/// when the bookmarks are disabled.
		/// </summary>
		/// <param name="Enabled"><see langword="true"/> to ignore bookmarks being enabled or disabled 
		/// (tied to dynamic enabled).</param>
		/// <returns><see langword="true"/> if Bookmarks should be updated.</returns>
		private bool BookStatus(bool Enabled)
		{
			return (Enabled || DynNamesMCB.State()) && !(
				GameBookmarkCB.SelectedIndex > 0 || (
				ModBookmarkCB.SelectedIndex > 0 && selectedMod));
		}

		private void PopulateTable()
		{
			Province[] SelProv = provinces.Where(Prov => Prov && Prov.Show).ToArray();
			var oldCount = ProvTable.RowCount;
			ProvTable.RowCount = SelProv.Length;

			if (oldCount < ProvTable.RowCount)
			{
				for (int Row = oldCount; Row < SelProv.Length; Row++)
				{
					if (Row % 2 == 0)
					{
						for (int Column = 1; Column < 6; Column++)
						{
							ProvTable[Column, Row].Style.BackColor = Colors.HeaderBackground;
						}
					}
				}
			}

			for (int Prov = 0; Prov < SelProv.Length; Prov++)
			{
				ProvTable[0, Prov].Style.BackColor = SelProv[Prov].Color;
				ProvTable.Rows[Prov].SetValues(SelProv[Prov].ToRow());
				provinces[SelProv[Prov].Index].TableIndex = Prov;
			}
			ProvTableSB.Maximum = ProvTable.RowCount - ProvTable.DisplayedRowCount(false) + 1;
			ProvTable.ClearSelection();
			if (ProvTableSB.Maximum < 1)
				ProvTableSB.Visible = false;
			else
				ProvTableSB.Visible = true;
		}

		/// <summary>
		/// Update the bookmark CBs with all relevant bookmarks.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		private void PopulateBooks(Scope Scope)
		{
			if (!BookStatus(false)) return;

			if (!bookmarks.Where(b => b.Code != null).Any()) return;

			var Books = new List<string>(bookmarks.Select(b => b.Name));

			switch (Scope)
			{
				case Scope.Game:
					GameBookmarkCB.DataSource = Books;
					break;
				case Scope.Mod:
					ModBookmarkCB.DataSource = Books;
					ModBookmarkCB.SelectedIndex = 0;
					break;
				default:
					break;
			}
		}

		/// <summary>
		///  Decides whether to enable / disable bookmark CBs.
		/// </summary>
		/// <param name="Scope">Game / Mod</param>
		/// <returns><see langword="true"/> if bookmark CBs should be enabled.</returns>
		private bool PrepEnableBooks(Scope Scope)
		{
			if (!DynNamesMCB.State()) return false;
			return Scope switch
			{
				Scope.Game => GameBookmarkCB.Items.Count > 0,
				Scope.Mod => ModBookmarkCB.Items.Count > 0,
				_ => throw new NotImplementedException()
			};
		}

		/// <summary>
		/// Enables / disables bookmark CBs.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		private void EnableBooks(Scope Scope)
		{
			var En = PrepEnableBooks(Scope);
			switch (Scope)
			{
				case Scope.Game:
					GameBookmarkCB.Enabled = En;
					if (!En) GameBookmarkCB.DataSource = null;
					break;
				case Scope.Mod:
					ModBookmarkCB.Enabled = En;
					if (!En) ModBookmarkCB.DataSource = null;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// A smart count of overall provinces and shown provinces. <br />
		/// </summary>
		/// <param name="Scope"></param>
		private void CountProv(Scope Scope)
		{
			switch (Scope)
			{
				case Scope.Game:
					GameProvCountTB.Text = provinces.Count(p => p && p.ToString().Length > 0).ToString();
					GameProvShownTB.Text = ProvTable.Rows.Count.ToString();
					break;
				case Scope.Mod:
					ModProvCountTB.Text = provinces.Count(p => p && p.ToString().Length > 0).ToString();
					ModProvShownTB.Text = ProvTable.Rows.Count.ToString();
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Combines the game and mod bookmark CB index changed handlers.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		private void EnactBook(Scope Scope)
		{
			if (lockdown) return;
			beginTiming = DateTime.Now;
			Cursor =System.Windows.Forms.Cursors.WaitCursor;
			Text = $"{appName} {appVer} - Loading bookmark";

			switch (Scope)
			{
				case Scope.Game:
					startDate = bookmarks.First(book => book.Name == GameBookmarkCB.SelectedItem.ToString()).StartDate;
					GameStartDateTB.Text = startDate.ToString(dateFormat);
					break;
				case Scope.Mod:
					startDate = bookmarks.First(book => book.Name == ModBookmarkCB.SelectedItem.ToString()).StartDate;
					ModStartDateTB.Text = startDate.ToString(dateFormat);
					break;
				default:
					break;
			}

			showRnw = ShowAllProvsMCB.State();
			updateCountries = true;
			CountryCulSetup();
			OwnerSetup();
			ProvNameSetup();
			DynamicSetup();
			PopulateTable();

			Cursor =System.Windows.Forms.Cursors.Default;
			Text = $"{appName} {appVer}";
			finishTiming = DateTime.Now;
		}

		/// <summary>
		/// Handles the folder browser.
		/// </summary>
		/// <param name="Scope">Game / Mod.</param>
		private void FolderBrowse(Scope Scope)
		{
			if (BrowserFBD.ShowDialog() != DialogResult.OK) return;
			Critical(CriticalType.Begin);

			switch (Scope)
			{
				case Scope.Game:
					GamePathTB.Text = BrowserFBD.SelectedPath;
					break;
				case Scope.Mod:
					ModPathTB.Text = BrowserFBD.SelectedPath;
					break;
				default:
					break;
			}
			PathHandler(Scope, Mode.Write);

			Critical(CriticalType.Finish, LaunchSequence());
		}

		/// <summary>
		/// Handles mod changing.
		/// </summary>
		private void ChangeMod()
		{
			if (ModSelCB.SelectedIndex < 1)
				Critical(CriticalType.Begin);
			else
				Critical(CriticalType.Begin, Scope.Mod);

			ModStartDateTB.Text = "";
			if (ModSelCB.SelectedIndex < 1)
			{
				selectedMod = null;
				steamModPath = "";
				ColorPickerGB.Enabled =
				ModInfoGB.Visible = false;
				ModBookmarkCB.SelectedIndex = -1;
			}
			else
			{
				if (!ModInfoGB.Visible)
				{
					if (GameBookmarkCB.Items.Count > 0)
					{
						GameBookmarkCB.SelectedIndex = 0;
						if (!selectedMod)
						{
							GameBookmarkCB.Enabled = false;
							startDate = bookmarks[GameBookmarkCB.SelectedIndex].StartDate;
							GameStartDateTB.Text = startDate.ToString(dateFormat);
						}
					}
					ColorPickerGB.Enabled =
					ModInfoGB.Visible = true;
				}
				selectedMod = mods[ModSelCB.SelectedIndex - 1];
				steamModPath = selectedMod.Path;
			}

			var Success = MainSequence();
			if (!Success)
			{
				if (selectedMod)
				{
					ModSelCB.SelectedIndex = 0;
					ChangeMod();
				}
				else ClearScreen();
			}
			else
				Settings.Default.LastSelMod = ModSelCB.SelectedItem.ToString();

			Critical(CriticalType.Finish, Success);
		}

		/// <summary>
		/// Adds the new <see cref="Province"/> to the definition.csv file, and updates the default.map file.
		/// </summary>
		private void NewProv()
		{
			string[] defFile;
			try
			{
				// Read all lines from the mod definition file
				defFile = File.ReadAllText(steamModPath + definPath, UTF7).Split(separators, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefinRead);
				return;
			}

			// A new line to be added when writing back to the file, in case there is no new line at the end of the file
			var newLine = newLineRE.Match(defFile[defFile.Length - 1]).Success ? "" : "\r\n";

			// Create an object of the new province
			var newProv = new Province {
				Index = NextProvNumberTB.Text.ToInt(),
				DefName = NextProvNameTB.Text,
				Color = new P_Color(RedTB.Text, GreenTB.Text, BlueTB.Text)
			};

			// Add the new province to the provinces array
			Array.Resize(ref provinces, provinces.Length + 1);
			provinces[newProv] = newProv;

			try
			{
				// Add the new province to the definition file
				File.AppendAllText(steamModPath + definPath, newLine + newProv.ToCsv() + "\r\n");
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefinWrite);
				return;
			}
			
			// Update prov counters
			ModProvCountTB.Text = Inc(ModProvCountTB.Text, 1);
			ModProvShownTB.Text = Inc(ModProvShownTB.Text, 1);
			ModMaxProvTB.Text = Inc(ModMaxProvTB.Text, 1);

			// In case the user gave the new province a strange name
			newProv.IsRNW();
			// Update ProvTable with the new province
			PopulateTable();
			// Update TB colors
			ProvCountColor();

			string DefMap;
			try
			{
				DefMap = File.ReadAllText(steamModPath + defMapPath);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefMapRead);
				return;
			}
			DefMap = defMapRE.Replace(DefMap, $"max_provinces = {ModMaxProvTB.Text}");
			try
			{
				File.WriteAllText(steamModPath + defMapPath, DefMap, UTF8);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefMapWrite);
			}
		}

		/// <summary>
		/// Reads game version from the game logs.
		/// </summary>
		private void GameVer()
		{
			try
			{
				GameInfoGB.Text = $"Game - {gameVerRE.Match(File.ReadAllText(selectedDocsPath + gameLogPath, UTF8)).Value}";
			}
			catch (Exception) { GameInfoGB.Text = "Game"; }
		}

		#region Control Handlers

		private void GameBrowseB_Click(object sender, EventArgs e)
		{
			if (lockdown) return;
			if (GamePathTB.Text != "") { BrowserFBD.SelectedPath = GamePathTB.Text; }
			FolderBrowse(Scope.Game);
		}

		private void ModBrowseB_Click(object sender, EventArgs e)
		{
			if (lockdown) return;
			if (ModPathTB.Text != "") { BrowserFBD.SelectedPath = ModPathTB.Text; }
			FolderBrowse(Scope.Mod);
		}

		private void NextProvNameTB_TextChanged(object sender, EventArgs e)
		{
			AddProvB.Enabled = NextProvNameTB.Text.Count(c => c == ' ') < NextProvNameTB.Text.Length;
		}

		private void AddProvB_Click(object sender, EventArgs e)
		{
			if (lockdown) return;
			NewProv();
			ClearCP();
		}

		private void DisableLoadMCB_Click(object sender, EventArgs e)
		{
			if (DisableLoadMCB.State()) return;

			DisableLoadMCB.State(true);

			RemLoadMCB.State(false);
			FullyLoadMCB.State(false);
			Settings.Default.AutoLoad = 0;
		}

		private void RemLoadMCB_Click(object sender, EventArgs e)
		{
			if (RemLoadMCB.State()) return;

			RemLoadMCB.State(true);
			DisableLoadMCB.State(false);
			FullyLoadMCB.State(false);
			Settings.Default.AutoLoad = 1;
		}

		private void FullyLoadMCB_Click(object sender, EventArgs e)
		{
			if (FullyLoadMCB.State()) return;

			FullyLoadMCB.State(true);
			DisableLoadMCB.State(false);
			RemLoadMCB.State(false);
			Settings.Default.AutoLoad = 2;
		}

		private void RandomizeB_Click(object sender, EventArgs e)
		{
			if (!lockdown) RndPrep();
		}

		private void CheckDupliMCB_CheckedChanged(object sender, EventArgs e)
		{
			if (ShowAllProvsMCB.State())
				IgnoreRnwMCB.Enabled = CheckDupliMCB.State();
			if (lockdown) return;

			Settings.Default.ColorDupli = CheckDupliMCB.State();
			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			Critical(CriticalType.Finish, MainSequence());
		}

		private void DefinNamesMCB_Click(object sender, EventArgs e)
		{
			DefinNamesMCB.State(true);
			LocNamesMCB.State(false);
			DynNamesMCB.State(false);
			Settings.Default.ProvNames = 0;
			
			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			Critical(CriticalType.Finish, MainSequence());
		}

		private void LocNamesMCB_Click(object sender, EventArgs e)
		{
			DefinNamesMCB.State(true);
			LocNamesMCB.State(true);
			DynNamesMCB.State(false);
			Settings.Default.ProvNames = 1;

			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			Critical(CriticalType.Finish, MainSequence());
		}

		private void DynNamesMCB_Click(object sender, EventArgs e)
		{
			DefinNamesMCB.State(true);
			LocNamesMCB.State(true);
			DynNamesMCB.State(true);
			Settings.Default.ProvNames = 2;

			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			Critical(CriticalType.Finish, MainSequence());
		}

		private void ShowAllProvsMCB_Click(object sender, EventArgs e)
		{
			ShowAllProvsMCB.State(!ShowAllProvsMCB.State());
			Settings.Default.ShowRNW = ShowAllProvsMCB.State();

			if (!ShowAllProvsMCB.State())
			{
				IgnoreRnwMCB.Enabled = false;
				IgnoreRnwMCB.State(true);
			}
			else
				IgnoreRnwMCB.Enabled = true;

			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			Critical(CriticalType.Finish, MainSequence());
		}

		private void DupliTable_DoubleClick(object sender, EventArgs e)
		{
			if (DupliTable.SelectedCells.Count != 1) return;

			string val = DupliTable.SelectedCells[0].Value.ToString();
			ProvTable.FirstDisplayedScrollingRowIndex =
				duplicates.First(p => p.Prov1.ToString() == val || p.Prov2.ToString() == val).Prov1.TableIndex;
		}

		private void GameBookmarkCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnactBook(Scope.Game);
		}

		private void ModBookmarkCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnactBook(Scope.Mod);
		}

		private void GameBookmarkCB_DropDown(object sender, EventArgs e)
		{
			foreach (var Item in GameBookmarkCB.Items)
			{
				TempL.Text = Item.ToString();
				if (TempL.Width + 5 > GameBookmarkCB.DropDownWidth)
					GameBookmarkCB.DropDownWidth = TempL.Width + 5;
			}
			if (GameBookmarkCB.Items.Count > GameBookmarkCB.MaxDropDownItems)
				GameBookmarkCB.DropDownWidth += widthSB;
		}

		private void ModBookmarkCB_DropDown(object sender, EventArgs e)
		{
			foreach (var Item in ModBookmarkCB.Items)
			{
				TempL.Text = Item.ToString();
				if (TempL.Width > ModBookmarkCB.DropDownWidth)
					ModBookmarkCB.DropDownWidth = TempL.Width;
			}
			if (ModBookmarkCB.Items.Count > ModBookmarkCB.MaxDropDownItems)
				ModBookmarkCB.DropDownWidth += widthSB;
		}

		private void GamePathTB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(GamePathTB, GamePathTB.Text);
		}

		private void ModPathTB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(ModPathTB, ModPathTB.Text);
		}

		private void ModSelCB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(ModSelCB, ModSelCB.Text);
		}

		private void GameBookmarkCB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(GameBookmarkCB, GameBookmarkCB.Text);
		}

		private void ModBookmarkCB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(ModBookmarkCB, ModBookmarkCB.Text);
		}

		private void GameStartDateTB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(GameStartDateTB, dateFormat);
		}

		private void ModStartDateTB_MouseHover(object sender, EventArgs e)
		{
			TextBoxTT.SetToolTip(ModStartDateTB, dateFormat);
		}

		private void GlobSetM_DropDownOpening(object sender, EventArgs e)
		{
			AutoLoadSM.Enabled =
			ProvNamesSM.Enabled = !lockdown;
		}

		private void ModSetM_DropDownOpening(object sender, EventArgs e)
		{
			DuplicatesSM.Enabled = !lockdown;
		}

		private void ProvNamesSM_DropDownOpening(object sender, EventArgs e)
		{
			DefinNamesMCB.Enabled = BookStatus(true);
			LocNamesMCB.Enabled =
			DynNamesMCB.Enabled = DefinNamesMCB.Enabled;
		}

		private void StatsM_DropDownOpening(object sender, EventArgs e)
		{
			if (beginTiming == DateTime.MinValue || finishTiming < beginTiming)
				LoadingValueML.Text = "";
			else
				LoadingValueML.Text = $"{finishTiming.Subtract(beginTiming).TotalSeconds:0.000} seconds";
		}

		private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
		{
			Settings.Default.Save();
		}

		private void ProvTable_Scroll(object sender, ScrollEventArgs e)
		{
			ProvTableSB.ScrollTo(e.NewValue);
		}

		private void ProvTableSB_MouseMove(object sender, MouseEventArgs e)
		{
			ProvTable.FirstDisplayedScrollingRowIndex = ProvTableSB.Value;
		}

		private void ProvTableSB_Scroll(object sender, ScrollEventArgs e)
		{
			ProvTable.FirstDisplayedCell = ProvTable.Rows[e.NewValue].Cells[0];
		}

		private void ModSelCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lockdown) return;
			ChangeMod();
		}

		private void ModSelCB_DropDown(object sender, EventArgs e)
		{
			foreach (var Item in ModSelCB.Items)
			{
				TempL.Text = Item.ToString();
				if (TempL.Width + 5 > ModSelCB.DropDownWidth)
					ModSelCB.DropDownWidth = TempL.Width + 5;
			}
			if (ModSelCB.Items.Count > ModSelCB.MaxDropDownItems)
				ModSelCB.DropDownWidth += widthSB;
		}

		private void CheckDupliMCB_Click(object sender, EventArgs e)
		{
			CheckDupliMCB.State(!CheckDupliMCB.State());
		}

		private void IgnoreRnwMCB_Click(object sender, EventArgs e)
		{
			IgnoreRnwMCB.State(IgnoreRnwMCB.State());
		}

		private void GameMaxProvTB_MouseHover(object sender, EventArgs e)
		{
			string text;
			if (GameMaxProvTB.BackColor == Color.Maroon)
				text = "Amount of provinces exceeds the limit.";
			else
				text = "Amount of provinces is within the limit.";

			TextBoxTT.SetToolTip(GameMaxProvTB, text);
		}

		private void ModMaxProvTB_MouseHover(object sender, EventArgs e)
		{
			string text;
			if (ModMaxProvTB.BackColor == Color.Maroon)
				text = "Amount of provinces exceeds the limit.";
			else
				text = "Amount of provinces is within the limit.";

			TextBoxTT.SetToolTip(ModMaxProvTB, text);
		}

		private void ModProvCountTB_MouseHover(object sender, EventArgs e)
		{
			string text = "";
			if (ModProvCountTB.BackColor == Color.Maroon)
				text = "The game has more provinces, so name conflicts may occur.";

			TextBoxTT.SetToolTip(ModProvCountTB, text);
		}

		#endregion

	}

	public static class MainWinExtensions
	{
		/// <summary>
		/// Sets menu item check state. <br />
		/// (Updates image in process)
		/// </summary>
		/// <param name="item">The menu item to set.</param>
		/// <param name="checkState">The check state to set to the menu item.</param>
		public static void State(this ToolStripMenuItem item, bool checkState)
		{
			item.Tag = checkState;

			if (checkState)
			{
				item.Image = item.OwnerItem.Tag switch
				{
					"Radio" => Resources.CheckedRadio,

					"CheckBox" => Resources.CheckedIconBox,

					_ => throw new NotImplementedException(),
				};
			}
			else
			{
				item.Image = item.OwnerItem.Tag switch
				{
					"RadioButton" => Resources.UncheckedRadio,

					"CheckBox" => Resources.UncheckedBox,

					_ => throw new NotImplementedException(),
				};
			}
		}

		/// <summary>
		/// Gets menu item check state.
		/// </summary>
		/// <param name="item">The menu item of which to get the check state.</param>
		/// <returns></returns>
		public static bool State(this ToolStripMenuItem item)
		{
			if (item.Tag is null) return false;
			return (bool)item.Tag;
		}
	}
}
