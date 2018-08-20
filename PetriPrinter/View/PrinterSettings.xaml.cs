using PetriPrinter.Model;
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

namespace PetriPrinter.View
{
    /// <summary>
    /// Interaction logic for PrinterSettings.xaml
    /// </summary>
    public partial class PrinterSettings : Window
    {
        private PrinterConfig _config;
        public PrinterConfig Config { get { return _config; } }

        public double Temperature { get { return _config.Temperature; } set { _config.Temperature = value; } }
        public double Zoff { get { return _config.Zoff; } set { _config.Zoff = value; } }

        public PrinterSettings(PrinterConfig config)
        {
            _config = config;
            InitializeComponent();
            this.DataContext = this;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
