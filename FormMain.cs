﻿using System;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Security.Principal;
using System.Drawing;

namespace SaveOrganizer
{
    public partial class FormMain : Form
    {
        //Unnecessary additions to a functional program
        public static string AppDataRoamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SaveOrganizer";
        public static string ConfigurationFile = AppDataRoamingPath + @"\Config.xml";
        KeyHooker Hooker;
        List<Hotkeys> LoadedHotkeys = new List<Hotkeys>();
        private static ListSortDirection SaveSortOrder;
        private static DataGridViewColumn SaveSortColumn;
        private string PreviousFileName = "";
        private bool PreviousReadOnly;
        private bool EnableGlobalHotkeys = false;
        Point StartPoint()
        {
            Point Inter = new Point(this.Location.X + Width / 2 - 186, Location.Y + Height - 170);
            return Inter;
        }

        public FormMain()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            CreateConfigurationFile();
            ReadGlobalConfigurations();
            ComboBoxSelectGame.SelectedIndex = 0;
            ComboBoxSelectSubDirectory.SelectedValue = "Default";
            Hooker = new KeyHooker();
            Hooker.Initialize();
            Hooker.PropertyChanged += new PropertyChangedEventHandler(KeyPressed);
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void KeyPressed(object sender, PropertyChangedEventArgs e)
        {
            if (EnableGlobalHotkeys)
            {
                string Modifier = ((Keys)Hooker.Modifier).ToString();
                string KeyCode = ((Keys)Hooker.CurrentKey).ToString();
                foreach (Hotkeys HotK in LoadedHotkeys)
                {
                    if (HotK.Modifier == Modifier && HotK.KeyPress == KeyCode && HotK.Enabled == true)
                    {
                        switch (HotK.HotkeyName)
                        {
                            case "ExportSave":
                                ExportSave();
                                break;
                            case "ImportSave":
                                ImportCurrentSave();
                                break;
                            case "ToggleReadOnly":
                                FileInfo SelectedSave = new FileInfo(GetGameSaveLocation(ComboBoxSelectGame.Text));
                                SetReadOnly(GetGameSaveLocation(ComboBoxSelectGame.Text), !SelectedSave.IsReadOnly);
                                break;
                        }
                    }
                }

            }
        }

        private string CurrentDirectory()
        {
            return AppDataRoamingPath + "\\" + ComboBoxSelectGame.Text;
        }

        private string CurrentSubDirectory()
        {
            return AppDataRoamingPath + "\\" + ComboBoxSelectGame.Text + "\\" + ComboBoxSelectSubDirectory.Text;
        }

        private string CurrentFile()
        {
            return DGVSaveFiles.CurrentRow.Cells[0].Value.ToString();
        }

        private string CurrentPath()
        {
            return AppDataRoamingPath + "\\" + ComboBoxSelectGame.Text + "\\" + ComboBoxSelectSubDirectory.Text + "\\" + DGVSaveFiles.CurrentRow.Cells[0].Value.ToString();
        }

        private void CreateConfigurationFile()
        {
            Directory.CreateDirectory(AppDataRoamingPath);
            Directory.CreateDirectory(AppDataRoamingPath + "\\Dark Souls\\Default");
            Directory.CreateDirectory(AppDataRoamingPath + "\\Dark Souls II\\Default");
            Directory.CreateDirectory(AppDataRoamingPath + "\\Dark Souls II SotFS\\Default");
            Directory.CreateDirectory(AppDataRoamingPath + "\\Dark Souls III\\Default");
            if (!File.Exists(ConfigurationFile))
            {
                XmlTextWriter Writer = new XmlTextWriter(ConfigurationFile, Encoding.UTF8);
                Writer.Formatting = Formatting.Indented;
                Writer.WriteStartElement("Configs");
                Writer.WriteStartElement("Games");
                Writer.WriteStartElement("Game");
                Writer.WriteStartElement("Name");
                Writer.WriteString("Dark Souls");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Path");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteStartElement("Game");
                Writer.WriteStartElement("Name");
                Writer.WriteString("Dark Souls II");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Path");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteStartElement("Game");
                Writer.WriteStartElement("Name");
                Writer.WriteString("Dark Souls II SotFS");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Path");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteStartElement("Game");
                Writer.WriteStartElement("Name");
                Writer.WriteString("Dark Souls III");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Path");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteEndElement();

                Writer.WriteStartElement("EnableHotkeys");
                Writer.WriteStartElement("Enabled");
                Writer.WriteString("False");
                Writer.WriteEndElement();
                Writer.WriteEndElement();

                Writer.WriteStartElement("AlwaysOnTop");
                Writer.WriteStartElement("Enabled");
                Writer.WriteString("False");
                Writer.WriteEndElement();
                Writer.WriteEndElement();

                Writer.WriteStartElement("Hotkeys");
                Writer.WriteStartElement("Hotkey");
                Writer.WriteStartElement("Name");
                Writer.WriteString("ImportSave");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Modifier");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("KeyCode");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Enabled");
                Writer.WriteString("False");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteStartElement("Hotkey");
                Writer.WriteStartElement("Name");
                Writer.WriteString("ExportSave");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Modifier");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("KeyCode");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Enabled");
                Writer.WriteString("False");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteStartElement("Hotkey");
                Writer.WriteStartElement("Name");
                Writer.WriteString("ToggleReadOnly");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Modifier");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("KeyCode");
                Writer.WriteString("None");
                Writer.WriteEndElement();
                Writer.WriteStartElement("Enabled");
                Writer.WriteString("False");
                Writer.WriteEndElement();
                Writer.WriteEndElement();
                Writer.WriteEndElement();

                Writer.WriteEndElement();
                Writer.Close();
            }
        }

        private void ReadGlobalConfigurations()
        {
            LoadedHotkeys.Clear();
            XmlDocument Xml = new XmlDocument();
            Xml.Load(ConfigurationFile);

            XmlNode AlwaysTopNode = Xml.SelectSingleNode("//AlwaysOnTop//Enabled");
            if (Convert.ToBoolean(AlwaysTopNode.InnerText))
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }

            XmlNode GlobalHotkeysNode = Xml.SelectSingleNode("//EnableHotkeys//Enabled");
            EnableGlobalHotkeys = Convert.ToBoolean(GlobalHotkeysNode.InnerText);

            XmlNodeList HotKeyNodes = Xml.SelectNodes("//Hotkeys//Hotkey");

            foreach (XmlNode Node in HotKeyNodes)
            {
                if (Node["Name"].InnerText == "ImportSave")
                {
                    AddHotkey(Node["Name"].InnerText, Node["Modifier"].InnerText, Node["KeyCode"].InnerText, Convert.ToBoolean(Node["Enabled"].InnerText));
                }
                if (Node["Name"].InnerText == "ExportSave")
                {
                    AddHotkey(Node["Name"].InnerText, Node["Modifier"].InnerText, Node["KeyCode"].InnerText, Convert.ToBoolean(Node["Enabled"].InnerText));

                }
                if (Node["Name"].InnerText == "ToggleReadOnly")
                {
                    AddHotkey(Node["Name"].InnerText, Node["Modifier"].InnerText, Node["KeyCode"].InnerText, Convert.ToBoolean(Node["Enabled"].InnerText));

                }
            }
        }

        private void GetFileNames(bool WithFilter)
        {
            DGVSaveFiles.Rows.Clear();

            DataTable FileTable = new DataTable();
            FileTable.Columns.Add("SaveName", typeof(string));
            FileTable.Columns.Add("DateCreated", typeof(string));
            FileTable.Columns.Add("ReadOnly", typeof(bool));

            string[] Files = Directory.GetFiles(CurrentSubDirectory());
            foreach (string File_ in Files)
            {
                FileInfo File_Info = new FileInfo(File_);
                FileTable.Rows.Add(File_Info.Name, File_Info.LastWriteTime, File_Info.IsReadOnly);
            }
            DataView FileView = new DataView(FileTable);
            FileView.RowFilter = string.Format("SaveName LIKE '%{0}%'", TxtFileSearch.Text);
            FileTable = FileView.ToTable();
            for (int i = 0; i < FileTable.Rows.Count; i++)
            {
                DGVSaveFiles.Rows.Add(FileTable.Rows[i][0], FileTable.Rows[i][1], FileTable.Rows[i][2]);
            }
        }

        public static void SaveSorting(DataGridView DGV)
        {
            SaveSortOrder = DGV.SortOrder == SortOrder.Ascending ?
                ListSortDirection.Ascending : ListSortDirection.Descending;
            SaveSortColumn = DGV.SortedColumn;
        }

        public static void RestoreSorting(DataGridView DGV)
        {
            if (SaveSortColumn != null)
            {
                DataGridViewColumn newCol = DGV.Columns[SaveSortColumn.Name];
                DGV.Sort(newCol, SaveSortOrder);
            }
            else
            {
                DGV.Sort(DGV.Columns[0], ListSortDirection.Ascending);
            }
        }

        private void GetFileNames()
        {
            SaveSorting(DGVSaveFiles);
            DGVSaveFiles.Rows.Clear();
            TxtFileSearch.Text = "Search...";
            DataTable FileTable = new DataTable();
            FileTable.Columns.Add("SaveName", typeof(string));
            FileTable.Columns.Add("DateCreated", typeof(DateTime));
            FileTable.Columns.Add("ReadOnly", typeof(bool));

            string[] Files = Directory.GetFiles(CurrentSubDirectory());
            foreach (string File_ in Files)
            {
                FileInfo File_Info = new FileInfo(File_);
                FileTable.Rows.Add(File_Info.Name, File_Info.LastWriteTime, File_Info.IsReadOnly);
            }

            for (int i = 0; i < FileTable.Rows.Count; i++)
            {
                DGVSaveFiles.Rows.Add(FileTable.Rows[i][0], FileTable.Rows[i][1], FileTable.Rows[i][2]);
            }
            RestoreSorting(DGVSaveFiles);
        }

        private void GetSubDirectories()
        {
            string[] SubDirs = Directory.GetDirectories(CurrentDirectory());
            ComboBoxSelectSubDirectory.Items.Clear();
            foreach (string Dir in SubDirs)
            {
                ComboBoxSelectSubDirectory.Items.Add(Path.GetFileName(Dir));
            }

            ComboBoxSelectSubDirectory.Items.Add("Add new ...");
        }

        private string GetGameSaveLocation(string Game)
        {
            XmlDocument Xml = new XmlDocument();
            Xml.Load(ConfigurationFile);

            XmlNodeList NodeList = Xml.SelectNodes("//Games//Game");
            foreach (XmlNode Node in NodeList)
            {
                if (Node["Name"].InnerText == ComboBoxSelectGame.Text)
                {
                    if (Node["Path"].InnerText != "")
                    {
                        return Node["Path"].InnerText;
                    }
                    else
                    {
                        string NewSaveLocation = GetNewGameSaveLocation();
                        Node["Path"].InnerText = NewSaveLocation;
                        Xml.Save(ConfigurationFile);
                        return NewSaveLocation;
                    }
                }
            }

            return GetNewGameSaveLocation();
        }

        private string GetNewGameSaveLocation()
        {
            OpenFileDialog GetPath = new OpenFileDialog();

            GetPath.Title = ComboBoxSelectGame.Text + " savefile location";

            if (GetPath.ShowDialog() == DialogResult.OK)
            {
                return GetPath.FileName;
            }
            return "";
        }

        private void SetGameSaveLocation(string Game)
        {
            XmlDocument Xml = new XmlDocument();
            Xml.Load(ConfigurationFile);

            XmlNodeList NodeList = Xml.SelectNodes("//Games//Game");
            foreach (XmlNode Node in NodeList)
            {
                if (Node["Name"].InnerText == ComboBoxSelectGame.Text)
                {
                    Node["Path"].InnerText = GetNewGameSaveLocation();
                    Xml.Save(ConfigurationFile);
                }
            }
        }

        private void CopyFile(string ReadPath, string WritePath)
        {
            string FullFilePath = WritePath + "\\CurrentSave";
            string FullFilePathTemp = WritePath + "\\CurrentSave";
            int Count = 0;
            while (File.Exists(FullFilePathTemp))
            {
                Count++;
                FullFilePathTemp = FullFilePath;
                FullFilePathTemp = FullFilePathTemp + "_" + Count;
            }

            FullFilePath = FullFilePathTemp;

            try
            {
                File.Copy(ReadPath, FullFilePath);
            }
            catch
            {

            }
        }

        private void FileEdit(string FileName, string NewFileName, bool ReadOnly)
        {
            FileAttributes OldAttributes = File.GetAttributes(FileName);
            if (ReadOnly)
            {
                OldAttributes = OldAttributes | FileAttributes.ReadOnly;
                File.SetAttributes(FileName, OldAttributes);
            }
            else
            {
                OldAttributes = OldAttributes & ~FileAttributes.ReadOnly;
                File.SetAttributes(FileName, OldAttributes);
            }

            File.Move(FileName, NewFileName);

        }

        private void SetReadOnly(string File_, bool ReadOnly)
        {
            FileAttributes OldAttributes = File.GetAttributes(File_);
            if (ReadOnly)
            {
                OldAttributes = OldAttributes | FileAttributes.ReadOnly;
                File.SetAttributes(File_, OldAttributes);
            }
            else
            {
                OldAttributes = OldAttributes & ~FileAttributes.ReadOnly;
                File.SetAttributes(File_, OldAttributes);
            }
        }

        private void ImportProfile()
        {
            FolderBrowserDialog SelectProfile = new FolderBrowserDialog();
            DialogResult DR = SelectProfile.ShowDialog();
            if (DR == DialogResult.OK)
            {
                string ProfileName = Path.GetFileName(SelectProfile.SelectedPath);
                DirectoryCopy(SelectProfile.SelectedPath, AppDataRoamingPath + "\\" + ComboBoxSelectGame.Text + "\\" + ProfileName, false);
                GetSubDirectories();
                ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf(ProfileName);

            }
        }

        private void AddNewProfile()
        {
            FormRename NewProfileName = new FormRename();
            NewProfileName.StartPosition = FormStartPosition.CenterParent;
            DialogResult DR = NewProfileName.ShowDialog();
            if (DR == DialogResult.OK)
            {
                Directory.CreateDirectory(CurrentDirectory() + "\\" + NewProfileName.NewName);
                GetSubDirectories();
                ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf(NewProfileName.NewName);
            }
            if (DR == DialogResult.Cancel)
            {
                ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf("Default");
            }
        }

        private void RenameProfile()
        {
            if (ComboBoxSelectSubDirectory.Text == "Default")
            {
                ActionCenter.Toast("You cannot rename the default profile.", StartPoint());
            }
            else
            {
                FormRename NewProfileName = new FormRename();
                NewProfileName.StartPosition = FormStartPosition.CenterParent;
                DialogResult DR = NewProfileName.ShowDialog();
                if (DR == DialogResult.OK)
                {
                    Directory.Move(CurrentSubDirectory(), CurrentDirectory() + "\\" + NewProfileName.NewName);
                    GetSubDirectories();
                    ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf(NewProfileName.NewName);
                }
            }
        }

        private void ImportCurrentSave()
        {
            CopyFile(GetGameSaveLocation(ComboBoxSelectGame.Text), CurrentSubDirectory());
            GetFileNames();

            DGVSaveFiles.ClearSelection();
            DateTime Latest = new DateTime(1999, 1, 1);
            int RowIndex = 0;
            foreach (DataGridViewRow Row in DGVSaveFiles.Rows)
            {
                if (Convert.ToDateTime(Row.Cells[1].Value) > Latest)
                {
                    Latest = Convert.ToDateTime(Row.Cells[1].Value);
                    RowIndex = Row.Index;
                }
            }

            DGVSaveFiles.CurrentCell = DGVSaveFiles.Rows[RowIndex].Cells[0];
        }

        private void ExportSave()
        {
            try
            {
                SetReadOnly(GetGameSaveLocation(ComboBoxSelectGame.Text), false);
                File.Copy(CurrentSubDirectory() + "\\" + DGVSaveFiles.CurrentRow.Cells[0].Value.ToString(), GetGameSaveLocation(ComboBoxSelectGame.Text), true);
            }
            catch (ArgumentException)
            {

            }
            catch (NullReferenceException)
            {

            }
        }

        private void DeleteCurrentProfile()
        {
            if (ComboBoxSelectSubDirectory.Text == "Default")
            {
                DialogResult DRe = ActionCenter.DialogResponse("You cannot delete the default profile. Would you like to clear its contents?");
                if (DRe == DialogResult.OK)
                {
                    string[] Files = Directory.GetFiles(CurrentSubDirectory());
                    foreach (string File_ in Files)
                    {
                        SetReadOnly(File_, false);
                    }
                    Directory.Delete(CurrentSubDirectory(), true);
                    Directory.CreateDirectory(CurrentSubDirectory());
                }

                GetSubDirectories();
                ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf("Default");
                return;
            }

            DialogResult DR = ActionCenter.DialogResponse("Are you sure you want to delete this profile?");
            if (DR == DialogResult.OK)
            {
                string[] Files = Directory.GetFiles(CurrentSubDirectory());
                foreach (string File_ in Files)
                {
                    SetReadOnly(File_, false);
                }

                Directory.Delete(CurrentSubDirectory(), true);
                GetSubDirectories();
                ComboBoxSelectSubDirectory.SelectedIndex = ComboBoxSelectSubDirectory.Items.IndexOf("Default");
            }
        }

        private void DeleteSelectedSave()
        {
            try
            {
                string FileName = CurrentSubDirectory() + "\\" + DGVSaveFiles.CurrentRow.Cells[0].Value.ToString();
                FileAttributes OldAttributes = File.GetAttributes(FileName);

                OldAttributes = OldAttributes & ~FileAttributes.ReadOnly;
                File.SetAttributes(FileName, OldAttributes);

                File.Delete(FileName);
                DGVSaveFiles.Rows.RemoveAt(DGVSaveFiles.CurrentRow.Index);
            }
            catch (NullReferenceException)
            {

            }
            catch (ArgumentNullException)
            {

            }
        }

        private void OpenSettingsForm()
        {
            EnableGlobalHotkeys = false;
            FormSettings Settings = new FormSettings();
            Settings.StartPosition = FormStartPosition.CenterParent;
            Settings.ShowDialog();
            ReadGlobalConfigurations();
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: " + sourceDirName);
            }

            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);

                file.CopyTo(temppath, false);
            }

            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void AddHotkey(string HotkeyName, string Modifier, string KeyPress, bool Enabled)
        {
            Hotkeys Hotkey = new Hotkeys();
            Hotkey.HotkeyName = HotkeyName;
            Hotkey.Modifier = Modifier;
            Hotkey.KeyPress = KeyPress;
            Hotkey.Enabled = Enabled;
            LoadedHotkeys.Add(Hotkey);
        }

        private void ComboBoxSelectGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetSubDirectories();
            ComboBoxSelectSubDirectory.Text = "Default";
            GetFileNames();
        }

        private void ComboBoxSelectSubDirectory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ComboBoxSelectSubDirectory.Text == "Add new ...")
            {
                AddNewProfile();
            }
            else
            {
                GetFileNames();
            }
        }

        private void BtnImportSave_Click(object sender, EventArgs e)
        {
            ImportCurrentSave();
        }

        private void TxtFileSearch_TextChanged(object sender, EventArgs e)
        {
            if (TxtFileSearch.Text != "Search...")
            {
                GetFileNames(true);
            }
        }

        private void DGVSaveFiles_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            DGVSaveFiles.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGVSaveFiles.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGVSaveFiles.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            DGVSaveFiles.Columns[0].FillWeight = 40;
            DGVSaveFiles.Columns[1].FillWeight = 40;
            DGVSaveFiles.Columns[2].FillWeight = 20;
            DGVSaveFiles.Columns[0].HeaderText = "Name";
            DGVSaveFiles.Columns[1].HeaderText = "Date Created";
            DGVSaveFiles.Columns[2].HeaderText = "Read Only";
            DGVSaveFiles.Columns[1].ReadOnly = true;
            DGVSaveFiles.Columns[2].SortMode = DataGridViewColumnSortMode.Automatic;
        }

        private void DGVSaveFiles_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            PreviousFileName = DGVSaveFiles.CurrentRow.Cells[0].Value.ToString();
            PreviousReadOnly = Convert.ToBoolean(DGVSaveFiles.CurrentRow.Cells[2].Value);
        }

        private void DGVSaveFiles_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string FileName = DGVSaveFiles.CurrentRow.Cells[0].Value.ToString();
                bool ReadOnly = Convert.ToBoolean(DGVSaveFiles.CurrentRow.Cells[2].Value);
                if (FileName != PreviousFileName || ReadOnly != PreviousReadOnly)
                {
                    FileEdit(CurrentSubDirectory() + "\\" + PreviousFileName, CurrentSubDirectory() + "\\" + FileName, ReadOnly);
                }
            }
            catch (NullReferenceException)
            {
                DGVSaveFiles.CurrentRow.Cells[0].Value = PreviousFileName;
            }

        }

        private void renameCurrentProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameProfile();
        }

        private void addNewProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewProfile();
        }

        private void deleteCurrentProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCurrentProfile();
        }

        private void importExistingProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportProfile();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hooker.CloseHooker();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            OpenSettingsForm();
        }

        private void BtnDeleteSave_Click(object sender, EventArgs e)
        {
            DeleteSelectedSave();
        }

        private void BtnExportSave_Click(object sender, EventArgs e)
        {
            ExportSave();
        }

        private void changeSavefileSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGameSaveLocation(ComboBoxSelectGame.Text);
        }

        private void DGVSaveFiles_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (DGVSaveFiles.IsCurrentCellDirty)
            {
                DGVSaveFiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DGVSaveFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (DGVSaveFiles.CurrentCell != null)
            {
                if (DGVSaveFiles.CurrentCell == DGVSaveFiles.CurrentRow.Cells[2])
                {
                    SetReadOnly(CurrentPath(), Convert.ToBoolean(DGVSaveFiles.CurrentCell.Value));
                }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettingsForm();
        }

        private void TSBtnHelp_Click(object sender, EventArgs e)
        {
            FormHelp Help = new FormHelp();
            Help.StartPosition = FormStartPosition.CenterParent;
            Help.ShowDialog();
        }

        private void DGVSaveFiles_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if(e.Column.Index == 1)
            {
                DateTime Cell1Date = Convert.ToDateTime(e.CellValue1.ToString());
                DateTime Cell2Date = Convert.ToDateTime(e.CellValue2.ToString());
                e.SortResult = DateTime.Compare(Cell2Date, Cell1Date);
                if (e.SortResult == 0)
                {
                    string NumbersCell1 = GetLeadingDigits(DGVSaveFiles.Rows[e.RowIndex1].Cells[0].Value.ToString());


                    string NumbersCell2 = GetLeadingDigits(DGVSaveFiles.Rows[e.RowIndex2].Cells[0].Value.ToString());

                    if (NumbersCell1 == "" || NumbersCell2 == "")
                    {
                        e.SortResult = System.String.Compare(e.CellValue1.ToString(), e.CellValue2.ToString());
                    }
                    else
                    {
                        e.SortResult = int.Parse(NumbersCell1).CompareTo(int.Parse(NumbersCell2));
                    }

                    e.Handled = true;
                }
            }
            else if(e.Column.Index == 2)
            {
                e.SortResult = System.String.Compare(e.CellValue2.ToString(), e.CellValue1.ToString());
                if (e.SortResult == 0)
                {
                    string NumbersCell1 = GetLeadingDigits(DGVSaveFiles.Rows[e.RowIndex1].Cells[0].Value.ToString());


                    string NumbersCell2 = GetLeadingDigits(DGVSaveFiles.Rows[e.RowIndex2].Cells[0].Value.ToString());

                    if (NumbersCell1 == "" || NumbersCell2 == "")
                    {
                        e.SortResult = System.String.Compare(e.CellValue1.ToString(), e.CellValue2.ToString());
                    }
                    else
                    {
                        e.SortResult = int.Parse(NumbersCell1).CompareTo(int.Parse(NumbersCell2));
                    }

                    e.Handled = true;
                }
            }
            else
            {
                string NumbersCell1 = GetLeadingDigits(e.CellValue1.ToString());

                string NumbersCell2 = GetLeadingDigits(e.CellValue2.ToString());

                if (NumbersCell1 == "" || NumbersCell2 == "")
                {
                    e.SortResult = System.String.Compare(e.CellValue1.ToString(), e.CellValue2.ToString());
                }
                else
                {
                    e.SortResult = int.Parse(NumbersCell1).CompareTo(int.Parse(NumbersCell2));
                }

                e.Handled = true;
            }
        }

        private string GetLeadingDigits(string CellStringValue)
        {
            string CellNumberValue = "";
            int CurrentIndex = 0;
            int NameLength2 = CellStringValue.Length;
            while (CurrentIndex < NameLength2 && char.IsDigit(CellStringValue[CurrentIndex]))
            {
                CellNumberValue = CellNumberValue + CellStringValue[CurrentIndex];
                CurrentIndex++;
            }
            return CellNumberValue;
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameProfile();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteCurrentProfile();
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewProfile();
        }

        private void importExistingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportProfile();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DeleteSelectedSave();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExportSave();
        }

        private void openInFileExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(CurrentSubDirectory());
        }

        private void setSavefileDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetGameSaveLocation(ComboBoxSelectGame.Text);
        }

        private void TxtFileSearch_Enter(object sender, EventArgs e)
        {
            if (TxtFileSearch.Text == "Search...")
            {
                TxtFileSearch.Text = "";
            }
        }

        private void TxtFileSearch_Leave(object sender, EventArgs e)
        {
            if (TxtFileSearch.Text == "")
            {
                TxtFileSearch.Text = "Search...";
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            if (!IsAdministrator())
            {
                ActionCenter.Toast("Not running as admin, global hotkeys will not work while in game", StartPoint(), 2);
            }
        }
    }
}
