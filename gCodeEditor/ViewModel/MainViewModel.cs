using GCodeAPI;
using gCodeEditor.Model;
using gCodeEditor.View;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace gCodeEditor.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region buttons
        public BasicVezerloBtn CreateNewBtn { get; private set; }
        public BasicVezerloBtn OpenBtn { get; private set; }
        public BasicVezerloBtn SaveBtn { get; private set; }
        public BasicVezerloBtn SaveAsBtn { get; private set; }
        public BasicVezerloBtn ImportBtn { get; private set; }
        public BasicVezerloBtn ShowGridSettingsBtn { get; private set; }
        public BasicVezerloBtn ShowPrinterSettingsBtn { get; private set; }
        public BasicVezerloBtn showHelpBtn { get; private set; }
        public BasicVezerloBtn showAboutBtn { get; private set; }
        public BasicVezerloBtn AddBtn { get; private set; }
        public BasicVezerloBtn RemoveBtn { get; private set; }
        public BasicVezerloBtn GenerateBtn { get; private set; }

        public BasicVezerloBtn Up1 { get; private set; }
        public BasicVezerloBtn Up10 { get; private set; }
        public BasicVezerloBtn Up100 { get; private set; }
        public BasicVezerloBtn UpZ { get; private set; }

        public BasicVezerloBtn Down1 { get; private set; }
        public BasicVezerloBtn Down10 { get; private set; }
        public BasicVezerloBtn Down100 { get; private set; }
        public BasicVezerloBtn DownZ { get; private set; }

        public BasicVezerloBtn RemoveFrom { get; private set; }
        public BasicVezerloBtn RemoveTo { get; private set; }
        public BasicVezerloBtn Remove { get; private set; }

        public BasicVezerloBtn InsertCommands { get; private set; }
        #endregion

        GCodeCollector _code;
        private int _codePos = 1;
        private int _listSize = 0;
        private ObservableCollection<IGCode> _rowList;

        public event Action<int> PositionChanged;

        public int? rFrom;
        public int? rTo;

        public string RemoveFromStr { get { if (rFrom == null) return ""; else return "From: " + rFrom.Value; } }
        public string RemoveToStr { get { if (rTo == null) return ""; else return "To: " + rTo.Value; } }

        public double? objW;
        public double? objH;

        public string ObjWidth { get { if (objW == null) return ""; else return string.Format("width: {0:N2}", objW.Value); } }
        public string ObjHeight { get { if (objH == null) return ""; else return string.Format("height: {0:N2}", objH.Value); } }


        public int ListSize
        {
            get { return _listSize; }
            set
            {
                _listSize = value;
                renderList();
            }
        }

        private string filePath;
        private bool _isChanged;
        private bool isChanged
        {
            get
            {
                return _isChanged;
            }
            set
            {
                _isChanged = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Title
        {
            get
            {
                if (Code == null)
                    return "Track Viewer";
                return "Track Viewer - " + System.IO.Path.GetFileName(filePath) + (isChanged ? "*" : "");
            }
        }

        GCodeCollector Code
        {
            get { return _code; }
            set
            {
                _code = value;
                NotifyPropertyChanged("CodePosition");
                NotifyPropertyChanged("CodeCount");
            }
        }

        public string CurrentCodeString
        {
            get
            {
                if (Code == null)
                    return "";
                return Code.Codes[Math.Min(CodePosition - 1, Code.Codes.Count - 1)].ToString();
                return Code.Codes[CodePosition - 1].ToString();
            }
        }

        public int CodePosition
        {
            get
            {
                if (Code == null)
                    return 1;

                return _codePos;
            }
            set
            {
                _codePos = value;
                NotifyPropertyChanged("CodePosition");
                NotifyPropertyChanged("CurrentCodeString");
                renderList();
                if (PositionChanged != null) PositionChanged(value);
                viewer.SetPosition(value);
            }
        }


        public int CodeCount
        {
            get
            {
                if (Code == null)
                    return 1;
                return Code.Codes.Count;
            }
        }

        private PetriTask Task;
        private string TaskPath;

        public ObservableCollection<IGCode> RowList { get { return _rowList; } private set { _rowList = value; NotifyPropertyChanged("RowList"); } }
        private TrackViewer viewer;

        public MainViewModel()
        {
            TrackViewer.ShiftView = new System.Windows.Point(50, 50);
            // CreateNewBtn = new BasicVezerloBtn(() => CreateNew());
            OpenBtn = new BasicVezerloBtn(() => Open());

            Up1 = new BasicVezerloBtn(() => ChangePosition(1));
            Up10 = new BasicVezerloBtn(() => ChangePosition(10));
            Up100 = new BasicVezerloBtn(() => ChangePosition(100));
            Down1 = new BasicVezerloBtn(() => ChangePosition(-1));
            Down10 = new BasicVezerloBtn(() => ChangePosition(-10));
            Down100 = new BasicVezerloBtn(() => ChangePosition(-100));

            UpZ = new BasicVezerloBtn(() => NextZ());
            DownZ = new BasicVezerloBtn(() => PreviousZ());

            RemoveFrom = new BasicVezerloBtn(() => { rFrom = CodePosition; NotifyPropertyChanged("RemoveFromStr"); });
            RemoveTo = new BasicVezerloBtn(() => { rTo = CodePosition; NotifyPropertyChanged("RemoveToStr"); });
            Remove = new BasicVezerloBtn(() =>
            {
                if (rTo == null || rFrom == null)
                    return;
                Code.Codes.RemoveRange(Math.Min(rTo.Value, rFrom.Value) - 1, Math.Abs(rTo.Value - rFrom.Value) + 1);
                rTo = null;
                rFrom = null;

                UpdateObjSize();

                NotifyPropertyChanged("RemoveFromStr");
                NotifyPropertyChanged("RemoveToStr");
                NotifyPropertyChanged("CodeCount");

                NotifyPropertyChanged("CodePosition");
                NotifyPropertyChanged("CurrentCodeString");
                renderList();
                if (PositionChanged != null) PositionChanged(CodePosition);
            });

            InsertCommands = new BasicVezerloBtn(() =>
            {
                InsertCommandsDialog icd = new InsertCommandsDialog();
                if (icd.ShowDialog() == true)
                {
                    string[] lines = icd.Answer.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    var list = new List<IGCode>(lines.Length);
                    foreach (var l in lines)
                    {
                        var code = GCodeReader.Read(l);
                        if (code != null) list.Add(code);
                    }
                    Code.Codes.InsertRange(CodePosition, list);
                    NotifyPropertyChanged("CodeCount");
                    UpdateObjSize();
                    NotifyPropertyChanged("CodePosition");
                    NotifyPropertyChanged("CurrentCodeString");
                    renderList();
                    PositionChanged?.Invoke(CodePosition);
                }


            });
            SaveBtn = new BasicVezerloBtn(() => Save());
            /*  SaveAsBtn = new BasicVezerloBtn(() => SaveAs());
             ShowGridSettingsBtn = new BasicVezerloBtn(() => ShowGridSettings());
             ImportBtn = new BasicVezerloBtn(() => Import());
             ShowPrinterSettingsBtn = new BasicVezerloBtn(() => ShowPrinterSettings());
             showHelpBtn = new BasicVezerloBtn(() => showHelp());
             showAboutBtn = new BasicVezerloBtn(() => showAbout());
             AddBtn = new BasicVezerloBtn(() => Add());
             RemoveBtn = new BasicVezerloBtn(() => Remove());
             GenerateBtn = new BasicVezerloBtn(() => Generate());

             AvailableTaskList = new ObservableCollection<PetriTask>();*/
        }

        private void UpdateObjSize()
        {
            var rec = GCodeTransforms.bound(Code);
            objW = rec.Width;
            objH = rec.Height;
            NotifyPropertyChanged("ObjWidth");
            NotifyPropertyChanged("ObjHeight");
        }

        private void NextZ()
        {
            for (int i = CodePosition; i <= CodeCount; i++)
            {
                var line = Code.Codes[i] as GCodeLine;
                if (line != null && line.z != null)
                {
                    CodePosition = i + 1;
                    return;
                }
            }

        }

        private void PreviousZ()
        {
            for (int i = CodePosition - 2; i >= 0; i--)
            {
                var line = Code.Codes[i] as GCodeLine;
                if (line != null && line.z != null)
                {
                    CodePosition = i + 1;
                    return;
                }
            }
        }

        private void ChangePosition(int v)
        {
            if (v > 0)
                CodePosition = Math.Min(CodeCount, CodePosition + v);
            else
                CodePosition = Math.Max(1, CodePosition + v);
        }

        private void Open()
        {

            if (!checkSave())
                return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Supported files|*.gcode; *.ptf|gcode file (*.gcode)|*.gcode|Petri Task File (*.ptf)|*.ptf";
            if (openFileDialog.ShowDialog() != true)
                return;

            filePath = openFileDialog.FileName;

            if (System.IO.Path.GetExtension(filePath).Contains("ptf"))
            {
                var task = IOTools.Open<PetriTask>(filePath);
                Code = task.Code;
                Task = task;
                TaskPath = filePath;
            }
            else
            {
                var r = new GCodeReader(openFileDialog.FileName);
                Code = r.Code;
            }
            RowList = new ObservableCollection<IGCode>();
            renderList();
            if (PositionChanged != null) PositionChanged(0);
            viewer = new TrackViewer(Code);

            viewer.AddPoint += CodeAddPoint;
            viewer.RemovePoint += CodeRemovePoint;

            viewer.Show();
            isChanged = true;
            UpdateObjSize();
        }
        /*
                private void SaveAs()
                {
                    if (Code == null)
                        return;

                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "Petri Printer Project file (*.ppp)|*.ppp";
                    if (saveFileDialog.ShowDialog() != true)
                        return;
                    CurrentTask.Path = saveFileDialog.FileName;

                    Save();
                }*/

        private List<IGCode> ConvertString2Code(string text)
        {
            string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            var list = new List<IGCode>(lines.Length);
            foreach (var l in lines)
            {
                var code = GCodeReader.Read(l);
                if (code != null) list.Add(code);
            }
            return list;
        }

        private void Save()
        {
            SaveDlg save = Task != null ? (new SaveDlg(Task)) : (new SaveDlg());


            if (save.ShowDialog() != true) return;

            if (Task == null)
            {
                Task = new PetriTask(save.CodeName);
            }

            Task.Name = save.CodeName;
            Task.Description = save.Description;
            Task.Code = Code;


            if (TaskPath == null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Petri Task File (*.ptf)|*.ptf";
                if (saveFileDialog.ShowDialog() != true)
                    return;
                TaskPath = saveFileDialog.FileName;
            }



            // var insideCode = GCodeTransforms.codeZnull(Task.Code);
            // insideCode = GCodeTransforms.codeShift(0.2,Task.Code);
            Task.Code = GCodeTransforms.codeShiftCenterTo(0, 0, Task.Code);


            IOTools.Save(TaskPath, Task);
        }

        private void CodeRemovePoint(int idx)
        {
            Code.Codes.RemoveAt(idx);
            NotifyPropertyChanged("CodeCount");

            NotifyPropertyChanged("CodePosition");
            NotifyPropertyChanged("CurrentCodeString");
            renderList();
            PositionChanged?.Invoke(CodePosition);
        }

        private void CodeAddPoint(int idx, System.Windows.Point p)
        {
            var code = Code.Codes[idx] as GCodeLine;
            var line = new GCodeLine(x: p.X, y: p.Y, speed: code.f, mode: code.SpeedMode);
            Code.Codes.Insert(idx, line);
            NotifyPropertyChanged("CodeCount");
            UpdateObjSize();
            NotifyPropertyChanged("CodePosition");
            NotifyPropertyChanged("CurrentCodeString");
            renderList();
            PositionChanged?.Invoke(CodePosition);
        }

        private bool checkSave()
        {
            return true;
        }


        public void renderList()
        {
            if (RowList == null)
                return;
            RowList.Clear();
            int start = Math.Max(0, CodePosition - 1 - ListSize / 2);
            int stop = Math.Min(Code.Codes.Count - 1, CodePosition - 1 + ListSize / 2);

            if (stop - start < ListSize)
                stop = Math.Min(Code.Codes.Count - 1, start + ListSize);

            for (int i = start; i <= stop; i++)
                RowList.Add(Code.Codes[i]);



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
