
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PetriPrinter.View
{
    /// <summary>
    /// Interaction logic for PetriControl.xaml
    /// </summary>
    public partial class PetriControl : UserControl
    {
        public enum Stats { Empty, Selected, Full }
        private Stats _stat = Stats.Full;
        public Stats Stat
        {
            get { return _stat; }
            private set
            {
                _stat = value;
                path8.Fill = Color1;
                path4150.Fill = Color4;
                path4146.Fill = Color3;
                path4152.Fill = Color2;
                ZPosition.Foreground = Color5;
            }
        }

        
       
        private bool _selected;
        public bool IsSelected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (value)
                    Stat = Stats.Selected;
                else
                    Stat =  Stats.Full;
            }
        }

        public Brush Color1
        {
            get
            {
                switch (Stat)
                {
                    case Stats.Empty:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)65, (byte)76, (byte)82));
                    case Stats.Selected:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)65, (byte)0, (byte)0));
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)76, (byte)0));
                }
            }
        }

        public Brush Color5
        {
            get
            {
                switch (Stat)
                {
                    case Stats.Empty:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
                    case Stats.Selected:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)255, (byte)255, (byte)255));
                    default:
                        return new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)0, (byte)0));
                }
            }
        }

        public Brush Color2 { get { return GetSubColor(193); } }
        public Brush Color3 { get { return GetSubColor(211); } }
        public Brush Color4 { get { return GetSubColor(232); } }

        private Brush GetSubColor(byte value)
        {
            switch (Stat)
            {
                case Stats.Empty:
                    return new SolidColorBrush(Color.FromArgb(255, (byte)value, (byte)value, (byte)value));
                case Stats.Selected:
                    return new SolidColorBrush(Color.FromArgb(255, (byte)value, (byte)0, (byte)0));
                default:
                    return new SolidColorBrush(Color.FromArgb(255, (byte)0, (byte)value, (byte)0));
            }
        }

        //public System.Windows.Point Position {get;set;}
        public int GridX { get; set; }
        public int GridY { get; set; }


        public PetriControl()
        {
            InitializeComponent();
            Stat = Stats.Full;


        }
    }
}
