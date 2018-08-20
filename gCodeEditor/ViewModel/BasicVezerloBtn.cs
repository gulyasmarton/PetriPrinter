using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace gCodeEditor.ViewModel
{
    public class BasicVezerloBtn : ICommand
    {
        private Delegate eljaras;

        public Action Executed;

        public BasicVezerloBtn(Action eljaras) : this((Delegate)eljaras) { }


        public BasicVezerloBtn(Delegate eljaras)
        {
            this.eljaras = eljaras;

        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            try
            {
                eljaras.DynamicInvoke();
                Executed?.Invoke();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

    }
}
