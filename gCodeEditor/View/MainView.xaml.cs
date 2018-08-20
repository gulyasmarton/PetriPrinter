using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        ViewModel.MainViewModel vm;

        public MainView()
        {
            InitializeComponent();

            CodeList.SizeChanged += CodeList_SizeChanged;
            /*
            for (int i = 0; i < 50; i++)
                CodeList.Items.Add(i);*/
            this.DataContextChanged += MainView_DataContextChanged;
            CodeList.Items.Add(0);
            this.Loaded += (s, e) =>
            {
                CodeList.Items.Clear();
                CodeSlider.Focus();
            };


            this.PreviewKeyDown += MainView_KeyDown;
        }

       

        private void MainView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                case Key.Up:
                    CodeSlider.Value = Math.Max(CodeSlider.Minimum, CodeSlider.Value - 1);
                    // e.IsInputKey = true;
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.Right:
                    CodeSlider.Value = Math.Min(CodeSlider.Maximum, CodeSlider.Value + 1);
                    // e.IsInputKey = true;
                    e.Handled = true;
                    break;
            }            
        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            vm = this.DataContext as ViewModel.MainViewModel;
            vm.PositionChanged += Vm_PositionChanged;
        }

        private void Vm_PositionChanged(int obj)
        {
            int size = vm.ListSize / 2;
            CodeList.SelectedIndex = Math.Min(obj - 1, size);

            // ListBoxItem selectedListBoxItem = CodeList.ItemContainerGenerator.ContainerFromIndex(size) as ListBoxItem;
            //selectedListBoxItem.Background = Brushes.Green;
        }

        private void CodeList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (CodeList.Items.IsEmpty)
                return;

            var h1 = CodeList.ActualHeight;
            //var r = CodeList.Items[0] as ListBoxItem;
            ListBoxItem container = CodeList.ItemContainerGenerator.ContainerFromItem(CodeList.Items[0]) as ListBoxItem;
            var h2 = container.ActualHeight;

            var row = (int)Math.Floor(h1 / h2);
            /* CodeList.Items.Clear();
             for (int i = 0; i < row; i++)
                 CodeList.Items.Add(i);

             Console.WriteLine(row);*/
            vm.ListSize = row;

        }
    }
}
