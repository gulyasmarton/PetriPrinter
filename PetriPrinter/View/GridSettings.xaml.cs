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
    /// Interaction logic for GridSettings.xaml
    /// </summary>
    public partial class GridSettings : Window
    {
        public GridConfig _grid;
        public GridConfig GridConfig { get { return _grid; } }

        public int Rows
        {
            get { return _grid.Rows; }
            set { _grid.Rows = value; }
        }

        public int Columns
        {
            get { return _grid.Columns; }
            set { _grid.Columns = value; }
        }

        public double DistanceRows
        {
            get { return _grid.DistanceRows; }
            set { _grid.DistanceRows = value; }
        }

        public double DistanceColumns
        {
            get { return _grid.DistanceColumns; }
            set { _grid.DistanceColumns = value; }
        }

        public double GridOffX
        {
            get { return _grid.GridOffX; }
            set { _grid.GridOffX = value; }
        }

        public double GridOffY
        {
            get { return _grid.GridOffY; }
            set { _grid.GridOffY = value; }
        }


        public GridSettings(GridConfig config)
        {
            _grid = config;
            InitializeComponent();
            DataContext = this;
        }


        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            if (GridConfig.Rows < 1 || GridConfig.Columns < 1)
            {
                MessageBox.Show("Minimum row and column number is one!");
                return;
            }
            if (GridConfig.DistanceColumns <= 0 || GridConfig.DistanceRows <= 0)
            {
                MessageBox.Show("The distance has to be bigger than zero!");
                return;
            }
            DialogResult = true;
        }
    }
}
