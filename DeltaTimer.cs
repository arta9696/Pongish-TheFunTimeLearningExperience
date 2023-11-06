using Pongish;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Pongish
{
    internal class DeltaTimer
    {
        Stopwatch updateTimer = new Stopwatch();
        List<double> deltatimes = new List<double>();
        double last = 0;
        public double DeltaTime { get {
                var ret = (updateTimer.ElapsedMilliseconds - last) / 1000;
                last = updateTimer.ElapsedMilliseconds;
                deltatimes.Add(ret);

                if (deltatimes.Count > 10000)
                {
                    double avg = deltatimes.Average();
                    deltatimes.Clear();
                    deltatimes.Add(avg);

                    updateTimer.Restart();
                    last = 0;
                }

                return ret;
            } }

        public DeltaTimer()
        {
            deltatimes.Add(0);
        }

        public void Start()
        {
            updateTimer.Start();
        }

        public double Average()
        {
            return deltatimes.Average();
        }

        public void Stop()
        {
            updateTimer.Stop();
        }
    }
}
