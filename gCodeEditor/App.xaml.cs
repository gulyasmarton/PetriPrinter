using gCodeEditor.View;
using gCodeEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace gCodeEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var vm = new MainViewModel();

            MainView view = new MainView();
            view.DataContext = vm;

            //view.Closing += (sender, ev) => { vm.SaveAll(); };
            view.Closed += (sender, ev) => { this.Shutdown(); };
            view.Show();
        }
    }
}
