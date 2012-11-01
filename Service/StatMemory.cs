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
    internal class MemStat
    {
        public long SampleID { get; set; }
        public long Used { get; set; }
        public long Free { get; set; }
        public long Total { get; set; }
        public long SwapUsed { get; set; }
        public long SwapTotal { get; set; }
        public long PageInCount { get; set; }
        public long PageOutCount { get; set; }
    }

    internal class StatMemory
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        Timer _timer;
        public FixedSizeQueue<MemStat> history { get; set; }
        public FixedSizeQueue<MemStat> historyHour { get; set; }
        public FixedSizeQueue<MemStat> historyDay { get; set; }
        public long sessionID;
        public long historyIndex;
        public long historyIndexHour;
        public long historyIndexDay;
        private readonly PerformanceCounter _pageInSec = new PerformanceCounter { CategoryName = "Memory", CounterName = "Pages Input/sec" };
        private readonly PerformanceCounter _pageOutSec = new PerformanceCounter { CategoryName = "Memory", CounterName = "Pages Output/sec" };
        private System.Object _lock = new System.Object();

        public StatMemory()
        {
            history = new FixedSizeQueue<MemStat>{ MaxSize = 602 };
            historyHour = new FixedSizeQueue<MemStat>{ MaxSize = 602 };
            historyDay = new FixedSizeQueue<MemStat>{ MaxSize = 602 };
        	sessionID = DateTime.Now.Ticks;
            historyIndex = -1;
            historyIndexHour = -1;
            historyIndexDay = -1;
            _pageInSec.NextValue();
            _pageOutSec.NextValue();

            _timer = new Timer(Update, null, 0, 1000);
            Update(null);
        }

        public void Update(object sender)
        {
            lock (_lock)
            {
                historyIndex++;

                var status = new MEMORYSTATUSEX();
                GlobalMemoryStatusEx(status);
                long pageInSec = Convert.ToInt64(_pageInSec.NextValue());
                long pageOutSec = Convert.ToInt64(_pageOutSec.NextValue());

                var ms = new MemStat
                {
                    SampleID = historyIndex,
                    Free = Convert.ToInt64(status.ullAvailPhys) / 1048576,
                    Total = Convert.ToInt64(status.ullTotalPhys) / 1048576,
                    PageInCount = pageInSec, //sample time is 1 second so should be ~
                    PageOutCount = pageOutSec, //sample time is 1 second so should be ~
                    SwapTotal = Convert.ToInt64(status.ullTotalPageFile) / 1048576,
                    SwapUsed = Convert.ToInt64(status.ullTotalPageFile - status.ullAvailPageFile) / 1048576
                };

                ms.Used = ms.Total - ms.Free;
                history.Enqueue(ms);


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

        private void AddHistoryItem(FixedSizeQueue<MemStat> from, int count, FixedSizeQueue<MemStat> to, long index)
        {

            long free = 0;
            long total = 0;
            long used = 0;

            for (int i = 1; i <= count; i++)
            {
                MemStat sample = from.ElementAt(history.Count() - i);
                free += sample.Free;
                used += sample.Used;
                total += sample.Total;
            }

            free /= count;
            total /= count;
            used /= count;

            var hourSample = new MemStat
            {
                SampleID = index,
                Free = free,
                Total = total,
                Used = used,
                PageInCount = 0,
                PageOutCount = 0,
                SwapTotal = 0,
                SwapUsed = 0
            };
            to.Enqueue(hourSample);
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

        private FixedSizeQueue<MemStat> HistoryForMode(int mode)
        {
            if (mode == 0)
                return history;
            if (mode == 1)
                return historyHour;
            if (mode == 2)
                return historyDay;
            return null;
        }

        public string samples(int mode, long index)
        {
            if (mode > 2)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                FixedSizeQueue<MemStat> from = HistoryForMode(mode);
                foreach (var s in from.Where(c => c.SampleID > index))
                {
                    data.Append(string.Format("<s id=\"{0}\" u=\"{1}\" f=\"{2}\" t=\"{3}\" su=\"{4}\" st=\"{5}\" pi=\"{6}\" po=\"{7}\"></s>",
                       s.SampleID, s.Used, s.Free, s.Total, s.SwapUsed, s.SwapTotal, s.PageInCount, s.PageOutCount));
                }

            }
            return data.ToString();
        }

        public void Dispose()
        {
            _pageInSec.Close();
            _pageInSec.Dispose();

            _pageOutSec.Close();
            _pageOutSec.Dispose();
        }
    }
}
