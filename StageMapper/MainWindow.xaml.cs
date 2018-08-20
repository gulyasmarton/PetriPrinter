using PetriPrinter.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StageMapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker bw;

        CalibratorModel model;

        public GridConfig _grid;
        public GridConfig GridConfig { get { return _grid; } }

        #region rács tulajdonságok

        public int Rows = 4;


        public int Columns = 4;

        public double DistanceRows = 50;

        public double DistanceColumns = 50;

        public double GridOffX = 1;

        public double GridOffY = 1;

        #endregion

        //private int zUtIdo = 5500;
        //private int zUt = 35;

        private double kapcsoloTetejeZ = 23;
        private Point kapcsoloPozicio = new Point(194.4, 203);

        private double calLapZ = 25;
        private Point calLapPozicio = new Point(173, 160);

        private Point adapterBetoltoAllas = new Point(196, 150);

        private double utazoMagassag = 37;

        private double CalibratorFejTavolsag = 0;

        double magicNumber = 15.525; //a felépítmény magassága

        public MainWindow()
        {
#if DEBUG
           
            Debugger.Debug.DebugMode = Debugger.Modes.Console;
            Debugger.Debug.ErrorMode = Debugger.Modes.Console;
#else
            Debugger.Debug.DebugMode = Debugger.Modes.File;
            Debugger.Debug.ErrorMode = Debugger.Modes.File;
#endif

            InitializeComponent();

            SetGrid();

            DataContext = this;

            model = new CalibratorModel();
            this.Closing += MainWindow_Closing;
        }

        private void SetGrid()
        {
            Stage.Children.Clear();
            Stage.ColumnDefinitions.Clear();
            Stage.RowDefinitions.Clear();

            int w = 4;
            int h = 4;

            for (int y = 0; y < h; y++)
            {
                var c = new ColumnDefinition();
                c.Width = new GridLength(1, GridUnitType.Star);
                Stage.ColumnDefinitions.Add(c);
            }

            for (int x = 0; x < w; x++)
            {
                var r = new RowDefinition();
                r.Height = new GridLength(1, GridUnitType.Star);
                Stage.RowDefinitions.Add(r);
            }

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    PetriControl petri = new PetriControl();
                    Grid.SetColumn(petri, x);
                    Grid.SetRow(petri, y);

                    if(x==3 && y == 0)
                        petri.IsSelected = true;

                    petri.MouseUp += (s, e) =>
                    {
                        Console.WriteLine("{0} {1}", petri.GridX, petri.GridY);
                        petri.IsSelected = !petri.IsSelected;
                    };

                    Stage.Children.Add(petri);
                    petri.GridX = x;
                    petri.GridY = h - 1 - y;

                }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            Debugger.Debug.WriteDebug("closing");

            if (model != null)
                model.Dispose();
            //SaveDefaultGridConfig();
        }

        private void OkBtnClick(object sender, RoutedEventArgs er)
        {
            if (bw != null)
            {
                if (bw.IsBusy)
                    return;
                bw.Dispose();
            }

            StartBtn.IsEnabled = false;
            var r = MessageBox.Show("Please remove calibartion head then click to OK!", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (r != MessageBoxResult.OK)
            {
                StartBtn.IsEnabled = true;
                return;
            }
            StopBtn.Content = "Cancel";

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += (s, e) =>
            {
#if DEBUG
                if (!model.isConnected)
                {
                    e.Result = false;
                }
#endif
                model.Init();
                model.printerWriteLine("G1 Z" + utazoMagassag.ToString("F6", CultureInfo.InvariantCulture) + " F6000");
                MessageBox.Show("Initialization was finished?", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                model.CurrentPosition.Z = utazoMagassag;
                
                if (bw.CancellationPending) return;
                model.GoTo(utazoMagassag);
                if (bw.CancellationPending) return;
                model.GoTo(adapterBetoltoAllas.X, adapterBetoltoAllas.Y);
                if (bw.CancellationPending) return;
                e.Result = true;

            };
            bw.RunWorkerCompleted += (s, e) =>
            {
                if (e.Cancelled || (bool)e.Result == false)
                {
                    StopBtn.Content = "Close";
                    StartBtn.IsEnabled = true;
                    return;
                }

                StartMapper();
            };
            bw.RunWorkerAsync();


        }

        private void StartMapper()
        {
            var r = MessageBox.Show("Please install the calibrator!", "Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);

            if (r != MessageBoxResult.OK)
            {
                StartBtn.IsEnabled = true;
                StopBtn.Content = "Close";
                return;
            }

            if (bw != null)
            {
                if (bw.IsBusy)
                    return;
                bw.Dispose();
            }

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += Bw_ProgressChanged;

            bool[] csesze = new bool[16];
            foreach (var p in Stage.Children)
            {
                var pp = p as PetriControl;
                csesze[pp.GridX + pp.GridY * 4] = (pp.Stat != PetriControl.Stats.Full);
            }

            bw.DoWork += (s, e) =>
            {


                double?[] list = new double?[16];
                e.Result = list;

                double? hegy = HeadCalibration();


#if !DEBUG
                if (hegy == null)
                    return;
#endif

                CalibratorFejTavolsag = hegy.Value + magicNumber;

                Debugger.Debug.WriteDebug("hegytavolsag: " + CalibratorFejTavolsag);

                int h = Rows;
                int w = Columns;
                int c = 0;


                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        var yk = (x % 2 != 0) ? h - y - 1 : y;

                        if (csesze[x + yk * 4])
                        {
                            list[x + yk * 4] = null;
                            continue;
                        }


                        if (bw.CancellationPending)
                            return;




                        double cX = x * DistanceColumns + GridOffX;
                        double cY = yk * DistanceRows + GridOffY;
                        double? value = null;
#if DEBUG
                        CalibratorFejTavolsag = 0;
                        c = c + 1;// x + yk * 4 + 1;
                        value = CalibratePosition(cX, cY);
                        value = (c * 2);
                        list[x + yk * 4] = value;
                        System.Threading.Thread.Sleep(500);

#else
                        value = CalibratePosition(cX, cY);
                        list[x + yk * 4] = value;
#endif
                        if (value != null)
                            bw.ReportProgress(0, new double[] { x, yk, value.Value });
                        if (bw.CancellationPending)
                            return;
                        model.GoTo(utazoMagassag);
                    }
                model.GoTo(utazoMagassag);
                model.GoTo(adapterBetoltoAllas.X, adapterBetoltoAllas.Y);



            };
            bw.RunWorkerCompleted += (s, e) =>
            {
                StopBtn.Content = "Close";
                StartBtn.IsEnabled = true;
                if (e.Cancelled)
                    return;
                StringBuilder sb = new StringBuilder();

                var list = e.Result as double?[];
                foreach (var n in list)
                {
                    if (n != null) sb.AppendLine((n.Value - CalibratorFejTavolsag).ToString("F6", CultureInfo.InvariantCulture));
                    else
                        sb.AppendLine("null");
                }


                var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
                saveFileDialog.Filter = "gridmap file (*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() != true)
                    return;


                using (StreamWriter outfile = new StreamWriter(saveFileDialog.FileName))
                {
                    outfile.Write(sb.ToString());
                }

            };
            bw.RunWorkerAsync();
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var arr = e.UserState as double[];
            int x = (int)arr[0];
            int y = 3 - (int)arr[1];
            double v = arr[2];

            var c = Stage.Children[x + y * 4] as PetriControl;
            c.ZPosition.Text = v.ToString();
        }

        private double? HeadCalibration()
        {
            if (bw.CancellationPending) return null;
            model.GoTo(kapcsoloPozicio.X, kapcsoloPozicio.Y);
            if (bw.CancellationPending) return null;
            model.GoTo(kapcsoloTetejeZ);
            if (bw.CancellationPending) return null;
            double? d1 = model.getZMacro(kapcsoloTetejeZ, CalibratorModel.Buttons.Button2);
            if (d1 == null || bw.CancellationPending) return null;
            model.GoTo(d1.Value + 2);
            model.GoTo(d1.Value + 0.2);
            d1 = model.getZMicro(d1.Value + 0.2, CalibratorModel.Buttons.Button2);
            if (d1 == null || bw.CancellationPending) return null;
            model.GoTo(utazoMagassag);

            if (bw.CancellationPending) return null;
            model.GoTo(adapterBetoltoAllas.X, adapterBetoltoAllas.Y);

            if (bw.CancellationPending) return null;
            model.GoTo(calLapPozicio.X, calLapPozicio.Y);
            if (bw.CancellationPending) return null;
            model.GoTo(calLapZ);
            if (bw.CancellationPending) return null;
            double? d2 = model.getZMacro(calLapZ, CalibratorModel.Buttons.Button1);
            if (d2 == null || bw.CancellationPending) return null;
            model.GoTo(d2.Value + 2);
            model.GoTo(d2.Value + 0.2);
            d2 = model.getZMicro(d2.Value + 0.2, CalibratorModel.Buttons.Button1);
            if (d2 == null || bw.CancellationPending) return null;
            model.GoTo(utazoMagassag);


            return d2.Value - d1.Value;
        }

        private double? CalibratePosition(double x, double y)
        {
            if (bw.CancellationPending) return null;
            model.GoTo(x, y);
            if (bw.CancellationPending) return null;
            model.GoTo(CalibratorFejTavolsag + 2);
            if (bw.CancellationPending) return null;
            double? d = model.getZMacro(CalibratorFejTavolsag + 2, CalibratorModel.Buttons.Button1);
            if (d == null || bw.CancellationPending) return null;
            model.GoTo(d.Value + 2);
            model.GoTo(d.Value + 0.2);
            d = model.getZMicro(d.Value + 0.2, CalibratorModel.Buttons.Button1);
            model.GoTo(utazoMagassag);
            return d;

        }

        private void StopBtnClick(object sender, RoutedEventArgs e)
        {
            if (bw != null)
            {
                if (bw.IsBusy)
                {
                    bw.CancelAsync();
                    return;
                }
            }
            this.Close();
        }



    }
}
