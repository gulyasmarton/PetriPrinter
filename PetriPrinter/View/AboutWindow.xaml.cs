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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {

        public string Licence
        {
            get { return "MIT licence"; }
        }

        public string Version
        {
            get
            {
                var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return "Version: " + ver;
            }
        }

        public AboutWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
