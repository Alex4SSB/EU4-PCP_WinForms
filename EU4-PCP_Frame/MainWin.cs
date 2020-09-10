using DarkUI.Config;
using DarkUI.Forms;
using EU4_PCP.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using static EU4_PCP.PCP_Const;
using static EU4_PCP.PCP_Data;
using static EU4_PCP.PCP_Implementations;
using static EU4_PCP.PCP_Paths;
using static EU4_PCP.PCP_RegEx;
using System.Text;

namespace EU4_PCP
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
			bool success = LaunchSequence();
			if (!success) ClearScreen();
			Critical(CriticalType.Finish, success);
		}

		/// <summary>
		/// Handles the beginning and finishing of (relatively) long-execution-time sections.
		/// </summary>
		/// <param name="mode">Begin / Finish.</param>
		/// <param name="success"><see langword="true"/> to update FinishTiming.</param>
		private void Critical(CriticalType mode, bool success) => Critical(mode, CriticalScope.Game, success);

		/// <summary>
		/// Handles the beginning and finishing of (relatively) long-execution-time sections.
		/// </summary>
		/// <param name="mode">Begin / Finish.</param>
		/// <param name="scope">Game / Mod.</param>
		/// <param name="success"><see langword="true"/> to update FinishTiming.</param>
		private void Critical(CriticalType mode, CriticalScope scope = CriticalScope.Game, bool success = false)
		{
			Text = $"{APP_NAME} {APP_VER}";
			switch (mode)
			{
				case CriticalType.Begin:
					beginTiming = DateTime.Now;
					Cursor = Cursors.WaitCursor;

					Text += " - Loading";
					Text += scope switch
					{
						CriticalScope.Game => "",
						CriticalScope.Mod => " mod",
						CriticalScope.Bookmark => " bookmark",
						_ => ""
					};

					lockdown = true;
					break;
				case CriticalType.Finish:
					Cursor = Cursors.Default;
					if (success) finishTiming = DateTime.Now;
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

			GamePathMB.Click += GamePathMB_Click;
			ModPathMB.Click += ModPathMB_Click;
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
			GameMaxProvTB.BackColor = Colors.LightBackground;
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

			ModPathMB.Enabled = true;
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
			gamePath = GamePathMTB.Text;
			DocsPrep();

			return false;
		}

		/// <summary>
		/// Handles all path validation for game and mod.
		/// </summary>
		/// <param name="scope">Game / Mod.</param>
		/// <param name="mode">Read from settings or Write to settings.</param>
		/// <returns><see langword="true"/> if the validation was successful.</returns>
		private bool PathHandler(Scope scope, Mode mode)
		{
			string setting = "";
			ToolStripTextBox box = null;
			switch (scope)
			{
				case Scope.Game:
					setting = Settings.Default.GamePath;
					box = GamePathMTB;
					break;
				case Scope.Mod:
					setting = Settings.Default.ModPath;
					box = ModPathMTB;
					break;
				default:
					break;
			}

			if (setting.Contains('|'))
				setting = setting.Split('|')[0];

			switch (mode)
			{
				case Mode.Read:
					switch (scope)
					{
						case Scope.Game when !File.Exists(setting + gameFile):
							ErrorMsg(ErrorType.GameExe);
							SetWr(scope, "");
							return false;
						case Scope.Mod:
							if (setting.Length < 1)
							{
								if (Directory.Exists($@"{selectedDocsPath}\mod"))
								{
									paradoxModPath = $@"{selectedDocsPath}\mod";
									setting = paradoxModPath;
								}
								else return false;
							}
							else paradoxModPath = setting;
							break;
						default:
							break;
					}
					box.Text = setting;
					break;
				case Mode.Write:
					SetWr(scope, box.Text);
					break;
				default:
					break;
			}
			return true;
		}

		/// <summary>
		/// Writes to the game / mod path settings.
		/// </summary>
		/// <param name="scope">Game / Mod.</param>
		/// <param name="s">The string to store in the settings.</param>
		private void SetWr(Scope scope, string s)
		{
			switch (scope)
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

			// Repaint duplicate rows before clearing the list
			PaintDupli(true);
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
				ModStartDateTB.Text = startDate.ToString(DATE_FORMAT);
				ModInfoGB.Text = $"Mod - {selectedMod.Ver}";
				CountProv(Scope.Mod);
				PopulateBooks(Scope.Mod);
				EnableBooks(Scope.Mod);
			}
			else
			{
				GameStartDateTB.Text = startDate.ToString(DATE_FORMAT);
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
			
			if (!LocPrep(LocScope.Province))
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
			bool enBooks = BookStatus(false);
			bool success = true;

			Parallel.Invoke(
				() => CulturePrep(),
				() => FetchFiles(FileType.Country));

			if (cultures.Count < 1)
			{
				ErrorMsg(ErrorType.NoCultures);
				return false;
			}
			else if (cultures.Where(cul => cul && cul.Group).Count() < 1)
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
				() => { if (enBooks) FetchFiles(FileType.Bookmark); });

			Parallel.Invoke(
				() => { if (enBooks) BookPrep(); },
				() => success = FetchFiles(FileType.Province));

			if (!success)
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
			AddProvB.Text = "Add Province";
			NextProvNameTB.BackColor = Colors.LightBackground;
			NextProvNameTB.ReadOnly = false;
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
			int r, g, b;
			var tempColor = new Color();

			do
			{
				r = rnd.Next(0, 255);
				g = rnd.Next(0, 255);
				b = rnd.Next(0, 255);
				tempColor = Color.FromArgb(r, g, b);
			} while (provinces.Count(p => p && p.Color == tempColor) > 0);

			RedTB.Text = r.ToString();
			GreenTB.Text = g.ToString();
			BlueTB.Text = b.ToString();
			GenColL.BackColor = tempColor;

			if (!NextProvNameTB.ReadOnly)
				NextProvNumberTB.Text = provinces.Length.ToString();
		}

		/// <summary>
		/// Default file max provinces.
		/// </summary>
		/// <param name="scope"></param>
		/// <returns><see langword="true"/> upon failure.</returns>
		private bool MaxProvinces(Scope scope)
		{
			var filePath = gamePath;
			if (scope == Scope.Mod) filePath = steamModPath;

			string d_file;
			try
			{
				d_file = File.ReadAllText(filePath + defMapPath, UTF8);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefMapRead);
				return true;
			}
			var match = maxProvRE.Match(d_file);

			if (!match.Success)
			{
				switch (scope)
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

			switch (scope)
			{
				case Scope.Game:
					GameMaxProvTB.Text = match.Value;
					break;
				case Scope.Mod:
					ModMaxProvTB.Text = match.Value;
					break;
				default:
					break;
			}

			// Update TB colors
			ProvCountColor();

			return false;
		}

		/// <summary>
		/// Handles colorization of province count text-boxes.
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

				ModProvCountTB.BackColor = ModProvCountTB.Text.Ge(GameProvCountTB.Text)
					? Colors.LightBackground : Color.Maroon;
			}
		}

		/// <summary>
		/// Prepare the duplicate provinces for display.
		/// </summary>
		private void DupliPrep()
		{
			if (!CheckDupliMCB.State() || !selectedMod)
			{
				PaintDupli(true);
				return;
			}
			var colors = new int[provinces.Length];
			var indexes = new int[provinces.Length];

			for (int prov = 0; prov < provinces.Length; prov++)
			{
				if (!provinces[prov]) continue;
				colors[prov] = provinces[prov].Color;
				indexes[prov] = provinces[prov];
			}

			Array.Sort(colors, indexes);
			var empty = colors.Count(c => c == 0);
			Array.Resize(ref colors, colors.Length - empty);
			Array.Resize(ref indexes, indexes.Length - empty);

			if (colors.Distinct().Count() == indexes.Length) return; // No duplicates
			for (int prov = 1; prov < colors.Length; prov++)
			{
				if (colors[prov] != colors[prov - 1]) continue;
				if (!provinces[indexes[prov]].Show || !provinces[indexes[prov - 1]].Show) continue;
				if (IgnoreRnwMCB.State() && (provinces[indexes[prov]].IsRNW(false) || provinces[indexes[prov - 1]].IsRNW(false))) continue;

				duplicates.Add(new Dupli(provinces[indexes[prov]], provinces[indexes[prov - 1]]));
			}

			PaintDupli();
		}

		/// <summary>
		/// Paint rows of duplicate provinces in the table, and add markers
		/// </summary>
		/// <param name="forceClear"><see langword="true"/> to remove all duplicate coloring.</param>
		private void PaintDupli(bool forceClear = false)
		{
			for (int prov = 0; prov < duplicates.Count; prov++)
			{
				Color prov1Color, prov2Color;
				
				if (!forceClear && CheckDupliMCB.State() && selectedMod)
				{
					UpdateMarkers(duplicates[prov].Dupli1, true);
					UpdateMarkers(duplicates[prov].Dupli2, true);
					prov1Color =
					prov2Color = Color.Maroon;
				}
				else
				{
					UpdateMarkers(duplicates[prov].Dupli1, false);
					UpdateMarkers(duplicates[prov].Dupli2, false);

					if (duplicates[prov].Dupli1.Prov.TableIndex % 2 == 0)
						prov1Color = Colors.HeaderBackground;
					else
						prov1Color = Colors.GreyBackground;

					if (duplicates[prov].Dupli2.Prov.TableIndex % 2 == 0)
						prov2Color = Colors.HeaderBackground;
					else
						prov2Color = Colors.GreyBackground;
				}

				for (int col = 1; col < 6; col++)
				{
					ProvTable[col, duplicates[prov].Dupli1.Prov.TableIndex].Style.BackColor = prov1Color;
					ProvTable[col, duplicates[prov].Dupli2.Prov.TableIndex].Style.BackColor = prov2Color;
				}
			}
		}

		/// <summary>
		/// Creates or destroys markers of the given duplicate provinces.
		/// </summary>
		/// <param name="dupli">The <see cref="Dupli"/> object to update from.</param>
		/// <param name="create"><see langword="true"/> to create the markers, <see langword="false"/> to destroy them.</param>
		public void UpdateMarkers(DupliProv dupli, bool create)
		{
			if (create) // Constructor
			{
				if (dupli.DupliLabel == null)
				{
					dupli.DupliLabel = new Label
					{
						BackColor = Color.Maroon,
						Size = MARKER_SIZE
					};

					this.Controls.Add(dupli.DupliLabel);
					dupli.DupliLabel.BringToFront();
					dupli.DupliLabel.Click += new EventHandler(DupliLabel_Click);
				}
				dupli.DupliLabel.Location = new Point(ProvTableSB.Location.X - 1,
						(int)(ProvTableSB.Location.Y + MARKER_Y_OFFSET +
						(((float)dupli.Prov.TableIndex /
						ProvTable.RowCount) * (ProvTableSB.Height - HEIGHT_OFFSET_SB))));
			}
			else // Destructor
			{
				this.Controls.Remove(dupli.DupliLabel);
				dupli.DupliLabel.Dispose();
				dupli.DupliLabel = null;
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
		/// <param name="enabled"><see langword="true"/> to ignore bookmarks being enabled or disabled 
		/// (tied to dynamic enabled).</param>
		/// <returns><see langword="true"/> if Bookmarks should be updated.</returns>
		private bool BookStatus(bool enabled)
		{
			return (enabled || DynNamesMCB.State()) && !(
				GameBookmarkCB.SelectedIndex > 0 || (
				ModBookmarkCB.SelectedIndex > 0 && selectedMod));
		}

		/// <summary>
		/// Writes all provinces to the ProvTable and paints it accordingly.
		/// </summary>
		private void PopulateTable()
		{
			Province[] selProv = provinces.Where(prov => prov && prov.Show).ToArray();
			var oldCount = ProvTable.RowCount;
			ProvTable.RowCount = selProv.Length;

			PaintTable(selProv.Length, oldCount);

			for (int prov = 0; prov < selProv.Length; prov++)
			{
				ProvTable[0, prov].Style.BackColor = selProv[prov].Color;
				ProvTable.Rows[prov].SetValues(selProv[prov].ToRow());
				provinces[selProv[prov].Index].TableIndex = prov;
			}
			ProvTableSB.Maximum = ProvTable.RowCount - ProvTable.DisplayedRowCount(false) + 1;
			ProvTable.ClearSelection();
			if (ProvTableSB.Maximum < 1)
				ProvTableSB.Visible = false;
			else
				ProvTableSB.Visible = true;
		}

		/// <summary>
		/// Paints the new rows in ProvTable in alternate colors.
		/// </summary>
		/// <param name="newCount">New row count.</param>
		/// <param name="oldCount">Row count before last change.</param>
		private void PaintTable(int newCount, int oldCount)
		{
			if (oldCount < newCount)
			{
				for (int row = oldCount; row < newCount; row++)
				{
					if (row % 2 == 0)
					{
						for (int col = 1; col < 6; col++)
						{
							ProvTable[col, row].Style.BackColor = Colors.HeaderBackground;
						}
					}
				}
			}
		}

		/// <summary>
		/// Update the bookmark CBs with all relevant bookmarks.
		/// </summary>
		/// <param name="scope">Game / Mod.</param>
		private void PopulateBooks(Scope scope)
		{
			if (!BookStatus(false)) return;

			if (!bookmarks.Where(b => b.Code != null).Any())
			{
				ModBookmarkCB.DataSource = null;
				return;
			}

			var books = new List<string>(bookmarks.Select(b => b.Name));

			switch (scope)
			{
				case Scope.Game:
					GameBookmarkCB.DataSource = books;
					break;
				case Scope.Mod:
					ModBookmarkCB.DataSource = books;
					ModBookmarkCB.SelectedIndex = 0;
					break;
				default:
					break;
			}
		}

		/// <summary>
		///  Decides whether to enable / disable bookmark CBs.
		/// </summary>
		/// <param name="scope">Game / Mod</param>
		/// <returns><see langword="true"/> if bookmark CBs should be enabled.</returns>
		private bool PrepEnableBooks(Scope scope)
		{
			if (!DynNamesMCB.State()) return false;
			return scope switch
			{
				Scope.Game => GameBookmarkCB.Items.Count > 0,
				Scope.Mod => ModBookmarkCB.Items.Count > 0,
				_ => throw new NotImplementedException()
			};
		}

		/// <summary>
		/// Enables / disables bookmark CBs.
		/// </summary>
		/// <param name="scope">Game / Mod.</param>
		private void EnableBooks(Scope scope)
		{
			var enable = PrepEnableBooks(scope);
			switch (scope)
			{
				case Scope.Game:
					GameBookmarkCB.Enabled = enable;
					if (!enable) GameBookmarkCB.DataSource = null;
					break;
				case Scope.Mod:
					ModBookmarkCB.Enabled = enable;
					if (!enable) ModBookmarkCB.DataSource = null;
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// A smart count of overall provinces and shown provinces. <br />
		/// </summary>
		/// <param name="scope"></param>
		private void CountProv(Scope scope)
		{
			switch (scope)
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
		/// <param name="scope">Game / Mod.</param>
		private void EnactBook(Scope scope)
		{
			if (lockdown) return;
			Critical(CriticalType.Begin, CriticalScope.Bookmark);

			switch (scope)
			{
				case Scope.Game:
					startDate = bookmarks.First(book => book.Name == GameBookmarkCB.SelectedItem.ToString()).StartDate;
					GameStartDateTB.Text = startDate.ToString(DATE_FORMAT);
					break;
				case Scope.Mod:
					startDate = bookmarks.First(book => book.Name == ModBookmarkCB.SelectedItem.ToString()).StartDate;
					ModStartDateTB.Text = startDate.ToString(DATE_FORMAT);
					break;
				default:
					break;
			}

			showRnw = ShowAllProvsMCB.State();
			updateCountries = true;
			CountryCulSetup();
			OwnerSetup(true);
			ProvNameSetup();
			DynamicSetup();
			PopulateTable();

			Critical(CriticalType.Finish, true);
		}

		/// <summary>
		/// Handles the folder browser.
		/// </summary>
		/// <param name="scope">Game / Mod.</param>
		private void FolderBrowse(Scope scope)
		{
			if (BrowserFBD.ShowDialog() != DialogResult.OK) return;
			Critical(CriticalType.Begin);

			switch (scope)
			{
				case Scope.Game:
					GamePathMTB.Text = BrowserFBD.SelectedPath;
					break;
				case Scope.Mod:
					ModPathMTB.Text = BrowserFBD.SelectedPath;
					break;
				default:
					break;
			}
			PathHandler(scope, Mode.Write);

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
				Critical(CriticalType.Begin, CriticalScope.Mod);

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
							GameStartDateTB.Text = startDate.ToString(DATE_FORMAT);
						}
					}
					ColorPickerGB.Enabled =
					ModInfoGB.Visible = true;
				}
				selectedMod = mods[ModSelCB.SelectedIndex - 1];
				steamModPath = selectedMod.Path;
			}

			var success = MainSequence();
			if (!success)
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

			Critical(CriticalType.Finish, success);
		}

		/// <summary>
		/// Adds the new <see cref="Province"/> to the definition.csv file, and updates the default.map file.
		/// </summary>
		private void NewProv()
		{
			byte[] byteStream;
			string textStream;

			try
			{
				byteStream = File.ReadAllBytes(steamModPath + definPath);
				textStream = File.ReadAllText(steamModPath + definPath, UTF7);
			}
			catch (Exception)
			{
				ErrorMsg(ErrorType.DefinRead);
				return;
			}

			var defFile = textStream.Split(SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
			var newLine = newLineRE.Match(textStream.Substring(textStream.Length - 2)).Success ? "" : "\r\n";

			if (NextProvNameTB.ReadOnly) // Update duplicate province
			{
				var oldColor = provinces[NextProvNumberTB.Text.ToInt()].Color.ToCsv();
				var streamIndex = textStream.IndexOf(provinces[NextProvNumberTB.Text.ToInt()].Index.ToString());

				provinces[NextProvNumberTB.Text.ToInt()].Color = new P_Color(RedTB.Text, GreenTB.Text, BlueTB.Text);
				var newColor = provinces[NextProvNumberTB.Text.ToInt()].Color.ToCsv();

				var colorIndex = streamIndex + NextProvNumberTB.Text.Length + 1;
				if (oldColor.Length != newColor.Length)
				{
					
					var endIndex = colorIndex + oldColor.Length;
					var endStream = new byte[byteStream.Length - endIndex];
					
					Array.Copy(byteStream, endIndex, endStream, 0, endStream.Length);
					Array.Resize(ref byteStream, byteStream.Length + (newColor.Length - oldColor.Length));
					Array.Copy(endStream, 0, byteStream, colorIndex + newColor.Length, endStream.Length);
				}
				Array.Copy(newColor.ToCharArray().Select(c => (byte)c).ToArray(), 0, byteStream, colorIndex, newColor.Length);

				try
				{
					File.WriteAllBytes(steamModPath + definPath, byteStream);
					File.AppendAllText(steamModPath + definPath, newLine);
				}
				catch (Exception)
				{
					ErrorMsg(ErrorType.DefinWrite);
					return;
				}
			}
			else // Add province
			{
				var newProv = new Province
				{
					Index = NextProvNumberTB.Text.ToInt(),
					DefName = NextProvNameTB.Text,
					Color = new P_Color(RedTB.Text, GreenTB.Text, BlueTB.Text)
				};

				Array.Resize(ref provinces, provinces.Length + 1);
				provinces[newProv] = newProv;

				ModMaxProvTB.Text = Inc(ModMaxProvTB.Text, 1);
				provinces[NextProvNumberTB.Text.ToInt()].IsRNW();

				try
				{
					File.AppendAllText(steamModPath + definPath, newLine + provinces[NextProvNumberTB.Text.ToInt()].ToCsv() + "\r\n");
				}
				catch (Exception)
				{
					ErrorMsg(ErrorType.DefinWrite);
					return;
				}

				string defMap;
				try
				{
					defMap = File.ReadAllText(steamModPath + defMapPath);
				}
				catch (Exception)
				{
					ErrorMsg(ErrorType.DefMapRead);
					return;
				}
				defMap = defMapRE.Replace(defMap, $"max_provinces = {ModMaxProvTB.Text}");
				try
				{
					File.WriteAllText(steamModPath + defMapPath, defMap, UTF8);
				}
				catch (Exception)
				{
					ErrorMsg(ErrorType.DefMapWrite);
				}
			}
			
			PopulateTable();

			if (NextProvNameTB.ReadOnly)
			{
				PaintDupli(true);
				duplicates.Clear();
				DupliPrep();
			}
			else
			{
				CountProv(Scope.Mod);
				ProvCountColor();
				PaintDupli();
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

		private void ModPathMB_Click(object sender, EventArgs e)
		{
			if (lockdown) return;
			if (ModPathMTB.Text != "") { BrowserFBD.SelectedPath = ModPathMTB.Text; }
			FolderBrowse(Scope.Mod);
		}

		private void GamePathMB_Click(object sender, EventArgs e)
		{
			if (lockdown) return;
			if (GamePathMTB.Text != "") { BrowserFBD.SelectedPath = GamePathMTB.Text; }
			FolderBrowse(Scope.Game);
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
			var success = MainSequence();
			Critical(CriticalType.Finish, success);

			if (!success)
				DefinNamesMCB_Click(this, e);
		}

		private void DynNamesMCB_Click(object sender, EventArgs e)
		{
			DefinNamesMCB.State(true);
			LocNamesMCB.State(true);
			DynNamesMCB.State(true);
			Settings.Default.ProvNames = 2;

			if (ProvTable.Rows.Count == 0) return;
			Critical(CriticalType.Begin);
			var success = MainSequence();
			Critical(CriticalType.Finish, success);

			if (!success)
				LocNamesMCB_Click(this, e);
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

		private void GameBookmarkCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnactBook(Scope.Game);
		}

		private void ModBookmarkCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			EnactBook(Scope.Mod);
		}

		private void BookmarkCB_DropDown(object sender, EventArgs e)
		{
			var cb = sender as ComboBox;

			foreach (var item in cb.Items)
			{
				TempL.Text = item.ToString();
				if (TempL.Width + 5 > cb.DropDownWidth)
					cb.DropDownWidth = TempL.Width + 5;
			}
			if (cb.Items.Count > cb.MaxDropDownItems)
				cb.DropDownWidth += WIDTH_SB;
		}

		private void CB_MouseHover(object sender, EventArgs e)
		{
			var cb = sender as ComboBox;

			TextBoxTT.SetToolTip(cb, cb.Text);
		}

		private void StartDateTB_MouseHover(object sender, EventArgs e)
		{
			var tb = sender as TextBox;

			TextBoxTT.SetToolTip(tb, DATE_FORMAT);
		}

		private void Menu_DropDownOpening(object sender, EventArgs e)
		{
			var menu = sender as ToolStripMenuItem;

            foreach (ToolStripItem item in menu.DropDownItems)
            {
				item.Enabled = !lockdown;
            }
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

		private void ModSelCB_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lockdown) return;
			ChangeMod();
		}

		private void ModSelCB_DropDown(object sender, EventArgs e)
		{
			foreach (var item in ModSelCB.Items)
			{
				TempL.Text = item.ToString();
				if (TempL.Width + 5 > ModSelCB.DropDownWidth)
					ModSelCB.DropDownWidth = TempL.Width + 5;
			}
			if (ModSelCB.Items.Count > ModSelCB.MaxDropDownItems)
				ModSelCB.DropDownWidth += WIDTH_SB;
		}

		private void CheckDupliMCB_Click(object sender, EventArgs e)
		{
			CheckDupliMCB.State(!CheckDupliMCB.State());
			Settings.Default.ColorDupli = CheckDupliMCB.State();

			if (ProvTable.Rows.Count == 0) return;
			DupliPrep();
		}

		private void IgnoreRnwMCB_Click(object sender, EventArgs e)
		{
			IgnoreRnwMCB.State(!IgnoreRnwMCB.State());
			Settings.Default.IgnoreRNW = IgnoreRnwMCB.State();

			if (ProvTable.Rows.Count == 0) return;
			DupliPrep();
		}

		private void MaxProvTB_MouseHover(object sender, EventArgs e)
		{
			var tb = sender as TextBox;

			string text;
			if (tb.BackColor == Color.Maroon)
				text = "Amount of provinces exceeds the limit.";
			else
				text = "Amount of provinces is within the limit.";

			TextBoxTT.SetToolTip(tb, text);
		}

		private void ModProvCountTB_MouseHover(object sender, EventArgs e)
		{
			string text = "";
			if (ModProvCountTB.BackColor == Color.Maroon)
				text = "The game has more provinces, so name conflicts may occur.";

			TextBoxTT.SetToolTip(ModProvCountTB, text);
		}

		private void ProvTableSB_ValueChanged(object sender, DarkUI.Controls.ScrollValueEventArgs e)
		{
			ProvTable.FirstDisplayedCell = ProvTable.Rows[e.Value].Cells[0];
		}

		private void ProvTable_MouseDown(object sender, MouseEventArgs e)
		{
			ProvTable.ClearSelection();
			ProvTable.Rows[ProvTable.HitTest(e.X, e.Y).RowIndex].Selected = true;
			ChangeColorSM.Visible =
				ProvTable.SelectedRows[0].Cells[1].Style.BackColor == Color.Maroon;
		}

		private void DupliLabel_Click(object sender, EventArgs e)
		{
			var dupliLabel = sender as Label;
			ProvTableSB.Value = (int)((float)(dupliLabel.Location.Y - ProvTableSB.Location.Y - MARKER_Y_OFFSET + 1) /
				(ProvTableSB.Height - HEIGHT_OFFSET_SB) *
				ProvTableSB.Maximum);
		}

		private void SelectInPickerMB_Click(object sender, EventArgs e)
		{
			NextProvNumberTB.Text = provinces[ProvTable.SelectedRows[0].Cells[1].Value.ToString().ToInt()].Index.ToString();
			NextProvNameTB.Text = provinces[ProvTable.SelectedRows[0].Cells[1].Value.ToString().ToInt()].ToString();
			NextProvNameTB.ReadOnly = true;
			AddProvB.Text = "Update Province";
			NextProvNameTB.BackColor = Colors.BlueBackground;
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
