using System;
using System.Threading;

namespace Progress.Time.Latch
{
    public class ProgressTimeLatchBuilder
    {
        public TimeSpan Delay { get; set; } = TimeSpan.FromMilliseconds(500);
        public TimeSpan MinShowTime { get; set; } = TimeSpan.FromMilliseconds(500);
        public Func<DateTime> CurrentTimeProvider { get; set; } = () => DateTime.Now;
        
        private readonly Action<bool> _viewRefreshingToggle;
        private readonly SynchronizationContext _context;
    
        public ProgressTimeLatchBuilder(Action<bool> viewRefreshingToggle, SynchronizationContext context)
        {
            _viewRefreshingToggle = viewRefreshingToggle;
            _context = context;
        }

        public ProgressTimeLatch Build() => new(
            _viewRefreshingToggle,
            CurrentTimeProvider,
            Delay,
            MinShowTime,
            _context
        );
    }
}
