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
    /// Interaction logic for URLdialog.xaml
    /// </summary>
    public partial class URLdialog : Window
    {
        public URLdialog()
        {
            InitializeComponent();
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }


        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            if(ResponseText==null || ResponseText.Equals(""))
                {
                MessageBox.Show("Textbox is empty!");
                return;
            }
            Uri uriResult;
            bool result = Uri.TryCreate(ResponseText, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (!result)
            {
                MessageBox.Show("URL is invalid!");
                return;
            }

            DialogResult = true;
        }
    }
}
