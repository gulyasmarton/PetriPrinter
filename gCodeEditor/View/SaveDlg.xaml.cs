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
using GCodeAPI;

namespace gCodeEditor.View
{
    /// <summary>
    /// Interaction logic for SaveDlg.xaml
    /// </summary>
    public partial class SaveDlg : Window
    {
        public string CodeName { get { return cName.Text; } }
        public string Description { get { return cDescription.Text; } }

        public SaveDlg()
        {
            InitializeComponent();
        }

        public SaveDlg(PetriTask task) : this()
        {
            cName.Text = task.Name;
            cDescription.Text = task.Description;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
