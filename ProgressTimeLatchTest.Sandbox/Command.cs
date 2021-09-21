using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProgressTimeLatchTest.Sandbox
{
    public class Command : ICommand
    {
        private readonly Func<Task> _act;

        public Command(Func<Task> act)
        {
            _act = act;
        }

        private bool _canExecute = true;
        public bool CanExecute(object? parameter) => _canExecute;

        public async void Execute(object? parameter)
        {
            _canExecute = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            try
            {
                await _act.Invoke();
            }
            finally
            {
                _canExecute = true;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
