using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ProgressTimeLatchTest.Sandbox
{
    public class ProgressWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Max => 100;
        public int Min => 0;
        
        private int _value;
        public int Value
        {
            get => _value;
            set
            {
                if (value == _value) return;
                _value = value;
                OnPropertyChanged();
            }
        }

        private readonly Task _task;
        
        public ProgressWindowViewModel()
        {
            _task = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10);
                    Dispatcher.CurrentDispatcher.Invoke(() => Value = (Value + 1) % Max);
                }
            });
        }

        public void Dispose()
        {
            _task.Dispose();
        }
    }
}