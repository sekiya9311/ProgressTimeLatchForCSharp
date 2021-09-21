using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Progress.Time.Latch;

namespace ProgressTimeLatchTest.Sandbox
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _useProgressTimeLatch;
        public bool UseProgressTimeLatch
        {
            get => _useProgressTimeLatch;
            set
            {
                if (value == _useProgressTimeLatch) return;
                _useProgressTimeLatch = value;
                OnPropertyChanged();
            }
        }

        private string _delay = "";
        public string Delay
        {
            get => _delay;
            set
            {
                if (value == _delay) return;
                _delay = value;
                OnPropertyChanged();
            }
        }

        public ICommand Execute => new Command(async () =>
        {
            if (int.TryParse(Delay, out var delayMs))
            {
                if (UseProgressTimeLatch)
                {
                    _progressTimeLatch.Loading = true;
                    await Task.Delay(delayMs);
                    _progressTimeLatch.Loading = false;   
                }
                else
                {
                    _progressWindow?.Close();
                    _progressWindow = new();
                    _progressWindow.Show();
                    await Task.Delay(delayMs);
                    _progressWindow.Close();
                    _progressWindow = null;
                }
            }
        });
        
        private readonly ProgressTimeLatch _progressTimeLatch;

        private ProgressWindow? _progressWindow;
        
        public MainWindowViewModel()
        {
            ProgressTimeLatchBuilder builder = new(f =>
            {
                _progressWindow?.Close();
                if (f)
                {
                    _progressWindow = new();
                    _progressWindow.Show();
                }
            }, SynchronizationContext.Current!);
            _progressTimeLatch = builder.Build();
        }
    }
}