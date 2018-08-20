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
    /// Interaction logic for InsertCommandsDialog.xaml
    /// </summary>
    public partial class InsertCommandsDialog : Window
    {
        public InsertCommandsDialog()
        {
            InitializeComponent();
            this.ContentRendered += InsertCommandsDialog_ContentRendered;
        }

        private void InsertCommandsDialog_ContentRendered(object sender, EventArgs e)
        {
            txtAnswer.SelectAll();
            txtAnswer.Focus();

        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }

        public string Answer
        {
            get { return txtAnswer.Text; }
        }

    }
}
