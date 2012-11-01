using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace iStatServerService
{    
    internal class CpuStat
    {
        public long SampleID { get; set; }
        public float Idle { get; set; }
        public float User { get; set; }
        public float Priv { get; set; }
    }

    internal class StatCPU
    {
        private Timer _timer;
        public FixedSizeQueue<CpuStat> history { get; set; }
        public FixedSizeQueue<CpuStat> historyHour { get; set; }
        public FixedSizeQueue<CpuStat> historyDay { get; set; }
        public long sessionID;
        public long historyIndex;
        public long historyIndexHour;
        public long historyIndexDay;
        private readonly PerformanceCounter _cpuPrivCounter = new PerformanceCounter { CategoryName = "Processor", CounterName = "% Privileged Time", InstanceName = "_Total" };
        private readonly PerformanceCounter _cpuUserCounter = new PerformanceCounter { CategoryName = "Processor", CounterName = "% User Time", InstanceName = "_Total" };
        private System.Object _lock = new System.Object();

        public StatCPU()
        {
            history = new FixedSizeQueue<CpuStat>{ MaxSize = 602 };
            historyHour = new FixedSizeQueue<CpuStat>{ MaxSize = 602 };
            historyDay = new FixedSizeQueue<CpuStat>{ MaxSize = 602 };
        	sessionID = DateTime.Now.Ticks;
            historyIndex = -1;
            historyIndexHour = -1;
            historyIndexDay = -1;
            _cpuPrivCounter.NextValue();
            _cpuUserCounter.NextValue();

            _timer = new Timer(Update, null, 0, 1000);
            Update(null);
        }

        public void Update(object sender)
        {
            lock (_lock)
            {
                historyIndex++;

                var usr = _cpuUserCounter.NextValue();
                var priv = _cpuPrivCounter.NextValue();
                var cs = new CpuStat
                {
                    SampleID = historyIndex,
                    Idle = clamp(0, 100, (int)(100 - (usr + priv))),
                    Priv = (int)priv,
                    User = (int)usr,
                };
                history.Enqueue(cs);

                if (historyIndex >= 6 && (historyIndex % 6) == 0)
                {
                    historyIndexHour++;
                    AddHistoryItem(history, 6, historyHour, historyIndexHour);
                }
                if (historyIndex >= 144 && (historyIndex % 144) == 0)
                {
                    historyIndexDay++;
                    AddHistoryItem(history, 144, historyDay, historyIndexDay);
                }
            }
       }

        private void AddHistoryItem(FixedSizeQueue<CpuStat> from, int count, FixedSizeQueue<CpuStat> to, long index)
        {
	    	float cpuUser = 0;
	    	float cpuPriv = 0;
	    	for (int i = 1; i <= count; i++)
      		{
      			CpuStat sample = from.ElementAt(history.Count() - i);
                  cpuUser += sample.User;
                  cpuPriv += sample.Priv;
      		}
             cpuUser /= count;
             cpuPriv /= count;

             var hourSample = new CpuStat
             {
                 SampleID = index,
                 Idle = clamp(0, 100, (int)(100 - (cpuUser + cpuPriv))),
                 Priv = cpuPriv,
                 User = cpuUser,
             };
             to.Enqueue(hourSample);
        }

        private int clamp(int min, int max, int value)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        private FixedSizeQueue<CpuStat> HistoryForMode(int mode)
        {
            if(mode == 0)
                return history;
            if(mode == 1)
                return historyHour;
            if(mode == 2)
                return historyDay;
            return null;
        }

        public long HistoryIndexForMode(int mode)
        {
            if (mode == 0)
                return historyIndex;
            if (mode == 1)
                return historyIndexHour;
            if (mode == 2)
                return historyIndexDay;
            return 0;
        }

        public string samples(int mode, long index)
        {
            // Ignore any history modes we dont support
            if (mode > 2)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                FixedSizeQueue<CpuStat> from = HistoryForMode(mode);
                foreach (var s in from.Where(c => c.SampleID > index))
                {
                    data.Append(string.Format("<s id=\"{0}\" u=\"{1}\" p=\"{2}\"></s>", s.SampleID, s.User, s.Priv));
                }
            }
            return data.ToString();
        }

        public void Dispose()
        {
            _cpuPrivCounter.Close();
            _cpuPrivCounter.Dispose();

            _cpuUserCounter.Close();
            _cpuUserCounter.Dispose();
        }
    }
}
