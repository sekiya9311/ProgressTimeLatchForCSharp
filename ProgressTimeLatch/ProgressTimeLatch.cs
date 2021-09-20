using System;
using System.Threading;
using System.Threading.Tasks;

namespace Progress.Time.Latch
{
    public class ProgressTimeLatch
    {
        private readonly Action<bool> _viewRefreshingToggle;
        private readonly Func<DateTime> _currentTimeProvider;
        private readonly TimeSpan _delay;
        private readonly TimeSpan _minShowTime;
        private readonly SynchronizationContext _context;

        private DateTime? _showTime;

        private CancellationTokenSource _showCancellationTokenSource = new();
        private CancellationTokenSource _hideCancellationTokenSource = new();
        
        private bool _loading;
        
        internal ProgressTimeLatch(
            Action<bool> viewRefreshingToggle,
            Func<DateTime> currentTimeProvider,
            TimeSpan delayMs,
            TimeSpan minShowTimeMs,
            SynchronizationContext context)
        {
            _viewRefreshingToggle = viewRefreshingToggle;
            _currentTimeProvider = currentTimeProvider;
            _delay = delayMs;
            _minShowTime = minShowTimeMs;
            _context = context;
        }
        
        public bool Loading
        {
            get => _loading;
            set
            {
                if (_loading == value) return;
                _loading = value;
                Cancel();

                if (value)
                {
                    _ = ExecuteWithDelay(
                        Show,
                        _delay,
                        _showCancellationTokenSource.Token
                    );
                }
                else if (_showTime.HasValue)
                {
                    var showTime = _currentTimeProvider.Invoke() - _showTime.Value;
                    if (showTime < _minShowTime)
                    {
                        _ = ExecuteWithDelay(
                            HideAndReset,
                            _minShowTime - showTime,
                            _hideCancellationTokenSource.Token
                        );
                    }
                    else
                    {
                        HideAndReset();
                    }
                }
                else
                {
                    HideAndReset();
                }
            }
        }
        
        private void Show()
        {
            _context.Post(_ => _viewRefreshingToggle.Invoke(true), null);
            _showTime = _currentTimeProvider.Invoke();
        }

        private void HideAndReset()
        {
            _context.Post(_ => _viewRefreshingToggle.Invoke(false), null);
            _showTime = null;
        }

        private void Cancel()
        {
            _showCancellationTokenSource.Cancel();
            _hideCancellationTokenSource.Cancel();
            _showCancellationTokenSource.Dispose();
            _hideCancellationTokenSource.Dispose();

            _showCancellationTokenSource = new();
            _hideCancellationTokenSource = new();
        }
        
        private static async Task ExecuteWithDelay(
            Action action,
            TimeSpan delaySpan,
            CancellationToken cancellationToken)
        {
            await Task.Delay(delaySpan, cancellationToken);
            
            cancellationToken.ThrowIfCancellationRequested();

            action.Invoke();
        }
    }
}
