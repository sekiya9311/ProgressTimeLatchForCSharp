using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Progress.Time.Latch.Tests
{
    public class ProgressTimeLatchTest
    {
        [Fact]
        public async Task Synchronize()
        {
            var cur = SynchronizationContext.Current!;
            SynchronizationContext c = new();
            int id = -1;
            await Task.Run(() => c.Send(_ => id = Thread.CurrentThread.ManagedThreadId, null));
            Assert.NotEqual(-1, id);
            cur.Send(_ => Assert.NotEqual(id, Thread.CurrentThread.ManagedThreadId), null);
            ProgressTimeLatchBuilder builder = new(f =>
            {
                Assert.Equal(id, Thread.CurrentThread.ManagedThreadId);
            }, c);
            var p = builder.Build();

            p.Loading = true;
            p.Loading = false;

            await Task.Delay(600);
        }

        [Fact]
        public async Task NotDisplayProgress()
        {
            var display = false;
            ProgressTimeLatchBuilder builder = new(f =>
            {
                display = f;
            }, SynchronizationContext.Current!);
            var p = builder.Build();
            
            // デフォルト値で、Loading の間隔が 500msec (delay) 未満の場合は、viewRefreshToggle は呼ばれない
            p.Loading = true;
            await Task.Delay(400);
            p.Loading = false;

            Assert.False(display);
            
            await Task.Delay(600);
            
            Assert.False(display);
        }

        [Fact]
        public async Task DisplayProgressAndExecuteQuickly()
        {
            var display = false;
            ProgressTimeLatchBuilder builder = new(f =>
            {
                display = f;
            }, SynchronizationContext.Current!);
            var p = builder.Build();

            // デフォルト値で、Loading の間隔が 500msec (delay) 以上の場合は、viewRefreshToggle が 500msec 後に呼ばれるが、
            // Loading が true の期間が 500msec (minShowTime) 未満の場合は、500msec (minShowTime) の間は、
            // viewRefreshToggle が呼ばれない
            
            p.Loading = true;
            Assert.False(display);
            
            await Task.Delay(600);
            Assert.True(display);
            
            p.Loading = false;
            Assert.True(display);
            
            await Task.Delay(600);
            Assert.False(display);
        }

        [Fact]
        public async Task DisplayProgressAndExecuteSlowly()
        {
            var display = false;
            ProgressTimeLatchBuilder builder = new(f =>
            {
                display = f;
            }, SynchronizationContext.Current!);
            var p = builder.Build();

            // デフォルト値で、Loading の間隔が 500msec (delay) 以上の場合は、viewRefreshToggle が 500msec 後に呼ばれ、
            // Loading が true の期間が 500msec (minShowTime) 以上の場合は、500msec (minShowTime) の場合は、即座に
            // viewRefreshToggle が呼ばれる
            
            p.Loading = true;
            Assert.False(display);
            
            await Task.Delay(1100);
            Assert.True(display);
            
            p.Loading = false;
            // NOTE: SynchronizationContext.Post で、非同期に viewRefreshToggle を呼んでいるので、それが実行されるように一瞬待機
            await Task.Delay(1);
            Assert.False(display);
        }
    }
}