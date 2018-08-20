using GCodeAPI;
using Microsoft.Win32;
using PetriPrinter.Model;
using PetriPrinter.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace PetriPrinter.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region buttons
        public BasicVezerloBtn CreateNewBtn { get; private set; }
        public BasicVezerloBtn OpenBtn { get; private set; }
        public BasicVezerloBtn SaveBtn { get; private set; }
        public BasicVezerloBtn SaveAsBtn { get; private set; }
        public BasicVezerloBtn ImportBtn { get; private set; }
        public BasicVezerloBtn LoadGridMapBtn { get; private set; }
        public BasicVezerloBtn ShowGridSettingsBtn { get; private set; }
        public BasicVezerloBtn ShowPrinterSettingsBtn { get; private set; }
        public BasicVezerloBtn showHelpBtn { get; private set; }
        public BasicVezerloBtn showAboutBtn { get; private set; }
        public BasicVezerloBtn AddBtn { get; private set; }
        public BasicVezerloBtn RemoveBtn { get; private set; }
        public BasicVezerloBtn GenerateBtn { get; private set; }
        #endregion


        public ObservableCollection<PetriTask> AvailableTaskList { get; private set; }

        public event Action<GridProject> GridProjectChanged;

        public GridProject CurrentTask
        {
            get
            {
                return _CurrentTask;
            }
            set
            {
                _CurrentTask = value;
                SelectedItems.Clear();
                NotifyPropertyChanged("WinTitle");
                if (GridProjectChanged != null) GridProjectChanged(value);
                if (value != null)
                    value.Changed += b => { if (GridProjectChanged != null) GridProjectChanged(value); NotifyPropertyChanged("WinTitle"); };

            }
        }

        public GridConfig CurrentGridConfig
        {
            get
            {
                return _CurrentGridConfig;
            }
            set
            {
                _CurrentGridConfig = value;
                if (CurrentTask != null)
                    CurrentTask.Grid = value;
                NotifyPropertyChanged("CurrentGridConfig");
            }
        }

        public PrinterConfig CurrentPrinterConfig
        {
            get
            {
                return _CurrentPrinterConfig;
            }
            set
            {
                _CurrentPrinterConfig = value;
                if (CurrentTask != null)
                    CurrentTask.Printer = value;
                NotifyPropertyChanged("CurrentPrinterConfig");
            }
        }

        public string WinTitle
        {
            get
            {

                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                var title = "Petri Printer " + version.Major + "." + version.Minor;
                if (CurrentTask != null) title += " - " + CurrentTask.Name + (CurrentTask.isChanged ? "*" : "");
                return title;
            }
        }

        public int StateValue { get { return _stateValue; } set { _stateValue = value; NotifyPropertyChanged("StateValue"); } }
        public string StateString { get { return _stateString; } set { _stateString = value; NotifyPropertyChanged("StateString"); } }

        public List<PetriControl> SelectedItems { get; private set; }
        public PetriTask SelectedTask { get; set; }

        #region private inner variables

        private GridProject _CurrentTask;
        private GridConfig _CurrentGridConfig;
        private PrinterConfig _CurrentPrinterConfig;
        private int _stateValue;
        private string _stateString;
        private object files;

       // private List<double?> GridMap;
        #endregion

        public MainViewModel()
        {
            CreateNewBtn = new BasicVezerloBtn(() => CreateNew());
            OpenBtn = new BasicVezerloBtn(() => Open());
            SaveBtn = new BasicVezerloBtn(() => Save());
            SaveAsBtn = new BasicVezerloBtn(() => SaveAs());
            ShowGridSettingsBtn = new BasicVezerloBtn(() => ShowGridSettings());
            ImportBtn = new BasicVezerloBtn(() => Import());
            LoadGridMapBtn  = new BasicVezerloBtn(() => LoadGridMap());
            ShowPrinterSettingsBtn = new BasicVezerloBtn(() => ShowPrinterSettings());
            showHelpBtn = new BasicVezerloBtn(() => showHelp());
            showAboutBtn = new BasicVezerloBtn(() => showAbout());
            AddBtn = new BasicVezerloBtn(() => Add());
            RemoveBtn = new BasicVezerloBtn(() => Remove());
            GenerateBtn = new BasicVezerloBtn(() => Generate());

            AvailableTaskList = new ObservableCollection<PetriTask>();

            CurrentGridConfig = PetriIO.LoadDefaultGridConfig();
            CurrentPrinterConfig = PetriIO.LoadDefaultPrinterConfig();
            SelectedItems = new List<PetriControl>();

            LoadTasks();
        }

        private void LoadGridMap()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Grid Height Map file (*.txt)|*.txt";
            if (!openFileDialog.ShowDialog().Value)
                return;

            string line = null;
            List<double?> map = new List<double?>();
            using (var file = new System.IO.StreamReader(openFileDialog.FileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    double d;
                    if (double.TryParse(line, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out d))
                        map.Add(d);
                    else
                        map.Add(null);
                }
            }
           CurrentTask.GridMap = map;
           GridProjectChanged?.Invoke(CurrentTask);
        }

        private void Import()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "PetriTask files (*.ptf)|*.ptf|Python script files (*.py)|*.py";
            if (!openFileDialog.ShowDialog().Value)
                return;

            foreach (var file in openFileDialog.FileNames)
                PetriIO.CopyTaskFile(file);
            LoadTasks();
        }



        private void ShowGridSettings()
        {
            var gridsetting = new GridSettings(CurrentGridConfig);
            if (gridsetting.ShowDialog() != true) return;
            CurrentGridConfig = gridsetting.GridConfig;
        }

        private void Generate()
        {
            if (CurrentTask == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "g-code file (*.gcode)|*.gcode";
            if (saveFileDialog.ShowDialog() != true)
                return;
            //CurrentTask.GridMap = GridMap;
            CurrentTask.GenerateCode(saveFileDialog.FileName);
        }

        private void Remove()
        {
            foreach (var shape in SelectedItems)
            {
                CurrentTask.Remove(shape.GridX, shape.GridY);
            }

            SelectedItems.Clear();
        }

        private void Add()
        {
            if (SelectedTask == null)
                return;

            foreach (var shape in SelectedItems)
            {
                CurrentTask.Add(SelectedTask, shape.GridX, shape.GridY);
            }

            SelectedItems.Clear();


        }

        private void showAbout()
        {
            new AboutWindow().ShowDialog();
        }

        private void showHelp()
        {
            System.Diagnostics.Process.Start("http://www.google.com");
        }

        private void ShowPrinterSettings()
        {
            var printersettings = new PrinterSettings(CurrentPrinterConfig);
            if (printersettings.ShowDialog() != true) return;
            CurrentPrinterConfig = printersettings.Config;
        }

        private void SaveAs()
        {
            if (CurrentTask == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Petri Task File (*.ptf)|*.ptf";
            if (saveFileDialog.ShowDialog() != true)
                return;
            CurrentTask.Path = saveFileDialog.FileName;

            Save();
        }

        private void Save()
        {
            if (CurrentTask == null)
                return;
            if (CurrentTask.Path == null || CurrentTask.Path.Equals(""))
            {
                SaveAs();
                return;
            }
            CurrentTask.isChanged = false;

            PetriIO.SaveProject(CurrentTask.Path, CurrentTask);
            NotifyPropertyChanged("WinTitle");
        }

        private bool checkSave()
        {
            if (CurrentTask != null && CurrentTask.isChanged)
            {
                MessageBoxResult rsltMessageBox = MessageBox.Show("Do you want to save the current project?", "Save Poject", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                switch (rsltMessageBox)
                {
                    case MessageBoxResult.Yes:
                        Save();
                        return true;

                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;

        }

        private void Open()
        {
            if (!checkSave())
                return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Petri Printer Project file (*.ppp)|*.ppp";
            if (openFileDialog.ShowDialog() != true)
                return;

            CurrentTask = PetriIO.OpenProject(openFileDialog.FileName);

        }

        private void CreateNew()
        {
            if (!checkSave())
                return;
            CurrentTask = new GridProject(CurrentGridConfig, CurrentPrinterConfig);
        }

        private void LoadTasks()
        {
            var list = PetriIO.LoadAvailablePetriTaskList();

            var bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.DoWork += (s, e) =>
            {
                int c = 1;
                bw.ReportProgress(0, "Working...");

                AvailableTaskList.Clear();
                foreach (var l in list)
                {
                    AvailableTaskList.Add(l);
                    bw.ReportProgress((int)(c * 1d / list.Count * 100), l.Name);
                    c++;
                }

                if (AvailableTaskList.Count > 0) SelectedTask = AvailableTaskList[0];
                bw.ReportProgress(100, "Complete!");
                System.Threading.Thread.Sleep(1000);
            };

            bw.ProgressChanged += (s, e) =>
            {
                StateString = e.UserState.ToString();
                StateValue = e.ProgressPercentage;

            };

            bw.RunWorkerCompleted += (s, e) =>
            {
                StateString = "";
                StateValue = 0;

                CreateNew();
            };

            bw.RunWorkerAsync();

        }

        public void SaveAll()
        {
            PetriIO.SaveDefaultGridConfig(CurrentGridConfig);
            PetriIO.SaveDefaultPrinterConfig(CurrentPrinterConfig);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
