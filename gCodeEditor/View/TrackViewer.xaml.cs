using GCodeAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace gCodeEditor.View
{
    /// <summary>
    /// Interaction logic for TrackViewer.xaml
    /// </summary>
    public partial class TrackViewer : Window
    {
        public static System.Windows.Point ShiftView { get; set; }

        public event Action<int> RemovePoint;
        public event Action<int, System.Windows.Point> AddPoint;

        System.Windows.Point? lastCenterPositionOnTarget;
        System.Windows.Point? lastMousePositionOnTarget;
        System.Windows.Point? lastDragPoint;
        Ellipse jelolo;
        bool mozog;
        int _p;
        public void SetPosition(int value)
        {


            int lastZ = PreviousZ(value);
            //int nextZ = NextZ(value);

            if (lastZ == -1)
                return;

            var zLine = (GCodeLine)Code.Codes[lastZ];

            zLabel.Text = zLine.z.Value.ToString();

            System.Windows.Point? p1 = null;

            if ((zLine.x == null || zLine.y == null) && lastZ - 1 >= 0)
            {
                zLine = Code.Codes[lastZ - 1] as GCodeLine;
                if (zLine != null && zLine.x != null && zLine.y != null)
                    p1 = new System.Windows.Point(zLine.x.Value+ShiftView.X, zLine.y.Value+ShiftView.Y);
            }

            double speed = 9000;

            Vaszon.Children.Clear();

            for (int i = lastZ + 1; i < value; i++)
            {
                var line = Code.Codes[i] as GCodeLine;
                if (p1 == null && line != null && line.x != null && line.y != null)
                {
                    p1 = new System.Windows.Point(line.x.Value + ShiftView.X, line.y.Value + ShiftView.Y);
                }
                else if (line != null && line.x != null && line.y != null)
                {
                    if (line.f != null)
                        speed = line.f.Value;

                    var _Lines = new Line();
                    _Lines.DataContext = line;

                    _Lines.X1 = p1.Value.X;
                    _Lines.Y1 = p1.Value.Y;

                    _Lines.X2 = line.x.Value + ShiftView.X;
                    _Lines.Y2 = line.y.Value + ShiftView.X;
                    _Lines.Stroke = new SolidColorBrush(MapRainbowColor(speed, 9000, 100));
                    _Lines.StrokeThickness = 0.2;
                    _Lines.StrokeStartLineCap = PenLineCap.Round;
                    _Lines.StrokeEndLineCap = PenLineCap.Round;
                    //_Lines.Visibility = System.Windows.Visibility.Hidden;
                    p1 = new System.Windows.Point(line.x.Value + ShiftView.X, line.y.Value + ShiftView.X);
                    Vaszon.Children.Add(_Lines);
                }
            }
            if (p1 != null)
            {
                Canvas.SetLeft(jelolo, p1.Value.X - jelolo.Width / 2);
                Canvas.SetTop(jelolo, p1.Value.Y - jelolo.Height / 2);
                Vaszon.Children.Add(jelolo);
            }
            _p = value;
        }



        GCodeCollector Code;

        public TrackViewer(GCodeCollector code)
        {
            InitializeComponent();
            Code = code;

            zoomSlider.ValueChanged += OnSliderValueChanged;
            scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            scrollViewer.MouseMove += Vaszon_MouseMove;
            scrollViewer.PreviewMouseRightButtonUp += OnMouseRightButtonUp;
            scrollViewer.PreviewMouseRightButtonDown += OnMouseRightButtonDown;
            scrollViewer.PreviewMouseWheel += OnPreviewMouseWheel;


            RemoveBtn.Click += (s, e) =>
            {
                RemovePoint?.Invoke(_p - 1);
                SetPosition(_p);
            };

            Vaszon.MouseUp += (s, e) =>
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl)) return;
                System.Windows.Point currentPosition = e.GetPosition(Vaszon);

                currentPosition.X -= ShiftView.X;
                currentPosition.Y -= ShiftView.Y;

                AddPoint?.Invoke(_p, currentPosition);

                var line = Vaszon.Children[Vaszon.Children.Count - 2] as Line;

                SetPosition(_p + 1);
            };

            jelolo = new Ellipse();
            jelolo.Fill = Brushes.Yellow;
            jelolo.Stroke = Brushes.Black;
            jelolo.Width = 1;
            jelolo.Height = 1;
            jelolo.StrokeThickness = 0.2;
            jelolo.MouseEnter += (s, e) => { Mouse.OverrideCursor = Cursors.Hand; };
            jelolo.MouseLeave += (s, e) => { Mouse.OverrideCursor = null; };

            jelolo.MouseDown += (s, e) =>
            {
                mozog = true;
                jelolo.CaptureMouse();
            };
            jelolo.MouseUp += (s, e) =>
            {
                mozog = false;
                jelolo.ReleaseMouseCapture();
            };
            jelolo.MouseMove += (s, e) =>
            {
                if (!mozog) return;
                System.Windows.Point currentPosition = e.GetPosition(Vaszon);
                
               

                Canvas.SetLeft(jelolo, currentPosition.X - jelolo.Width / 2);
                Canvas.SetTop(jelolo, currentPosition.Y - jelolo.Height / 2);

                var line = Vaszon.Children[Vaszon.Children.Count - 2] as Line;

                line.X2 = currentPosition.X;
                line.Y2 = currentPosition.Y;

                var l = line.DataContext as GCodeLine;
                currentPosition.X -= ShiftView.X;
                currentPosition.Y -= ShiftView.Y;
                l.x = currentPosition.X;
                l.y = currentPosition.Y;
            };

        }

        #region ScrollViewer
        void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            lastMousePositionOnTarget = Mouse.GetPosition(grid);

            if (e.Delta > 0)
            {
                zoomSlider.Value += 1;
            }
            if (e.Delta < 0)
            {
                zoomSlider.Value -= 1;
            }

            e.Handled = true;
        }
        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                System.Windows.Point? targetBefore = null;
                System.Windows.Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        System.Windows.Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, grid);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(grid);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / grid.Width;
                    double multiplicatorY = e.ExtentHeight / grid.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }

        void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }
        void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }

        void Vaszon_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(Vaszon);
            if (!(p.X < 0 || p.Y < 0 || p.X > 200 || p.Y > 200))
            {
                xLabel.Text = Math.Round(p.X).ToString();
                yLabel.Text = Math.Round(p.Y).ToString();
            }

            if (lastDragPoint.HasValue)
            {
                System.Windows.Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }
        #endregion


        private int NextZ(int CodePosition)
        {
            for (int i = CodePosition; i <= Code.Codes.Count; i++)
            {
                var line = Code.Codes[i] as GCodeLine;
                if (line != null && line.z != null)
                {
                    CodePosition = i + 1;
                    return i;
                }
            }
            return -1;
        }

        private int PreviousZ(int CodePosition)
        {
            for (int i = CodePosition - 2; i >= 0; i--)
            {
                var line = Code.Codes[i] as GCodeLine;
                if (line != null && line.z != null)
                {
                    CodePosition = i + 1;
                    return i;
                }
            }
            return -1;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            var centerOfViewport = new System.Windows.Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, grid);
        }

        private Color MapRainbowColor(double value, double red_value, double blue_value)
        {
            return MapRainbowColor((float)value, (float)red_value, (float)blue_value);
        }
        private Color MapRainbowColor(float value, float red_value, float blue_value)
        {
            // Convert into a value between 0 and 1023.
            int int_value = (int)(1023 * (value - red_value) /
                (blue_value - red_value));

            // Map different color bands.
            if (int_value < 256)
            {
                // Red to yellow. (255, 0, 0) to (255, 255, 0).
                return Color.FromRgb(255, (byte)int_value, 0);
            }
            else if (int_value < 512)
            {
                // Yellow to green. (255, 255, 0) to (0, 255, 0).
                int_value -= 256;
                return Color.FromRgb((byte)(255 - int_value), 255, 0);
            }
            else if (int_value < 768)
            {
                // Green to aqua. (0, 255, 0) to (0, 255, 255).
                int_value -= 512;
                return Color.FromRgb(0, 255, (byte)int_value);
            }
            else
            {
                // Aqua to blue. (0, 255, 255) to (0, 0, 255).
                int_value -= 768;
                return Color.FromRgb(0, (byte)(255 - int_value), 255);
            }
        }
    }
}
