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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PetriPrinter.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window
    {
        List<PetriControl> selectedItems;

        Brush selectedColor = Brushes.Orange;
        Brush setColor = Brushes.Red;
        Brush emptyColor = Brushes.Green;

        ViewModel.MainViewModel vm;

        public MainView()
        {
            InitializeComponent();
            
            this.DataContextChanged += MainView_DataContextChanged;
            
        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            vm = this.DataContext as ViewModel.MainViewModel;
            vm.GridProjectChanged += Vm_GridProjectChanged;
            selectedItems = vm.SelectedItems;
        }

        private void Vm_GridProjectChanged(Model.GridProject proj)
        {          
            Stage.Children.Clear();
            Stage.ColumnDefinitions.Clear();
            Stage.RowDefinitions.Clear();
            if (proj == null)
                return;
            int w = proj.Grid.Columns;
            int h = proj.Grid.Rows;

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
                    var task = proj.Get(x, h - 1 - y);
                    
                    PetriControl petri = new PetriControl();
                    petri.Task = task;

                    if (proj.GridMap != null && proj.GridMap.Count > x + y * w && proj.GridMap[x + (h - 1 - y) * w] != null)
                        petri.ZPosition.Text = proj.GridMap[x + (h-1-y) * w].Value.ToString();
                    else
                        petri.ZPosition.Text = "";
                    //Ellipse shape = new Ellipse();

                    //shape.Stroke = Brushes.Black;
                    /*
                    if (task == null)
                    {
                        //shape.Fill = emptyColor;

                    }
                    else
                    {
                        shape.Fill = setColor;
                        var tp = new StackPanel();
                        var txt1 = new TextBlock();
                        txt1.Text = task.Name;
                        txt1.FontWeight = FontWeights.Bold;

                        var txt2 = new TextBlock();
                        txt2.Text = task.Description;

                        tp.Children.Add(txt1);
                        tp.Children.Add(txt2);

                        shape.ToolTip = tp;
                    }
                    */
                    Grid.SetColumn(petri, x);
                    Grid.SetRow(petri, y);

                   // shape.Margin = new Thickness(5);

                    petri.MouseUp += (s, e) =>
                    {
                        petri.IsSelected = !petri.IsSelected;
                        if(petri.IsSelected)
                            selectedItems.Add(petri);
                        else
                            selectedItems.Remove(petri);
                        /*
                        if(shape.Fill != selectedColor)
                        {
                            selectedItems.Add(shape);
                            shape.Fill = selectedColor;
                        }
                        else
                        {
                            selectedItems.Remove(shape);
                            shape.Fill = task==null?emptyColor:setColor;
                        }*/
                    };

                    Stage.Children.Add(petri);
                    petri.GridX = x;
                    petri.GridY = h - 1 - y;
                    //shape.DataContext = new Point(x,y);
                }



        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
