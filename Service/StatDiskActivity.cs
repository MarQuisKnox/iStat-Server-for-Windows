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
    internal class StatDiskActivityItem
    {   
    	public readonly string name;
    	public readonly string uuid;
        public FixedSizeQueue<DiskActivityStatQueue> history { get; set; }
        public FixedSizeQueue<DiskActivityStatQueue> historyHour { get; set; }
        public FixedSizeQueue<DiskActivityStatQueue> historyDay { get; set; }
  
        private PerformanceCounter diskRead = new PerformanceCounter();
		private PerformanceCounter diskWrite = new PerformanceCounter();
        private PerformanceCounter diskIOPSRead = new PerformanceCounter();
        private PerformanceCounter diskIOPSWrite = new PerformanceCounter();

        public void Dispose()
        {
            diskRead.Close();
            diskRead.Dispose();

            diskWrite.Close();
            diskWrite.Dispose();

            diskIOPSRead.Close();
            diskIOPSRead.Dispose();

            diskIOPSWrite.Close();
            diskIOPSWrite.Dispose();
        }

        public StatDiskActivityItem(string n)
        {
            history = new FixedSizeQueue<DiskActivityStatQueue>{ MaxSize = 602 };
            historyHour = new FixedSizeQueue<DiskActivityStatQueue> { MaxSize = 602 };
            historyDay = new FixedSizeQueue<DiskActivityStatQueue> { MaxSize = 602 };
            name = n;
            uuid = n.GetHashCode().ToString();

            diskRead.CategoryName = "PhysicalDisk";
			diskRead.CounterName = "Disk Read Bytes/sec";
			diskRead.InstanceName = n;

            diskWrite.CategoryName = "PhysicalDisk";
			diskWrite.CounterName = "Disk Write Bytes/sec";
			diskWrite.InstanceName = n;

            diskIOPSRead.CategoryName = "PhysicalDisk";
            diskIOPSRead.CounterName = "Disk Reads/sec";
            diskIOPSRead.InstanceName = n;

            diskIOPSWrite.CategoryName = "PhysicalDisk";
            diskIOPSWrite.CounterName = "Disk Writes/sec";
            diskIOPSWrite.InstanceName = n;

            diskRead.NextValue();
            diskWrite.NextValue();
            diskIOPSRead.NextValue();
            diskIOPSWrite.NextValue();
        }
        
        public void Update(long historyIndex)
        {
            var read = diskRead.NextValue();
            var write = diskWrite.NextValue();
            var readIOPS = diskIOPSRead.NextValue();
            var writeIOPS = diskIOPSWrite.NextValue();

  			var ns = new DiskActivityStatQueue
            {
				read = read,
				write = write,
                readIOPS = readIOPS,
                writeIOPS = writeIOPS,
                SampleID = historyIndex
			};
			history.Enqueue(ns);
        }
        
        public void UpdateHour(long index)
        {
            AddHistoryItem(history, 6, historyHour, index);
        }
        
        public void UpdateDay(long index)
        {
            AddHistoryItem(history, 144, historyDay, index);
        }

        public FixedSizeQueue<DiskActivityStatQueue> HistoryForMode(int mode)
        {
            if(mode == 0)
                return history;
            if(mode == 1)
                return historyHour;
            if(mode == 2)
                return historyDay;
            return null;
        }

        private void AddHistoryItem(FixedSizeQueue<DiskActivityStatQueue> from, int count, FixedSizeQueue<DiskActivityStatQueue> to, long index)
        {
            if (from.Count() < count)
                return;
            double read = 0;
            double write = 0;
            for (int i = 1; i <= count; i++)
      		{
      			DiskActivityStatQueue sample = from.ElementAt(history.Count() - i);
                read += sample.read;
                write += sample.write;
      		}
            read /= count;
            write /= count;

            var hourSample = new DiskActivityStatQueue
            {
                SampleID = index,
                read = read,
                write = write,
                readIOPS = 0,
                writeIOPS = 0
            };
            to.Enqueue(hourSample);
        }

    }
    
    internal class DiskActivityStatQueue
    {
        public long SampleID { get; set; }
        public double read { get; set; }
        public double write { get; set; }
        public double readIOPS { get; set; }
        public double writeIOPS { get; set; }
    }

    internal class StatDiskActivity
    {
        Timer _timer;
        public readonly List<StatDiskActivityItem> _disks = new List<StatDiskActivityItem>();
        public long sessionID;
        public long historyIndex;
        public long historyIndexHour;
        public long historyIndexDay;
        private System.Object _lock = new System.Object();

        public StatDiskActivity()
        {
        	sessionID = DateTime.Now.Ticks;
            historyIndex = -1;
            historyIndexHour = -1;
            historyIndexDay = -1;
        	
        	var cat = new System.Diagnostics.PerformanceCounterCategory("PhysicalDisk");
            var instNames = cat.GetInstanceNames();
            foreach (var s in instNames)
            {
                if (s == "_Total")
                    continue;

                var item = new StatDiskActivityItem(s);
                _disks.Add(item);
            }

            _timer = new Timer(Update, null, 0, 1000);
            Update(null);
        }

        public void Update(object sender)
        {
                historyIndex++;
                if (historyIndex >= 6 && (historyIndex % 6) == 0)
                    historyIndexHour++;
                if (historyIndex >= 144 && (historyIndex % 144) == 0)
                    historyIndexDay++;

                foreach (StatDiskActivityItem disk in _disks)
                {
                    disk.Update(historyIndex);

                    if (historyIndex >= 6 && (historyIndex % 6) == 0)
                        disk.UpdateHour(historyIndexHour);

                    if (historyIndex >= 144 && (historyIndex % 144) == 0)
                        disk.UpdateDay(historyIndexDay);
                }
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
            if (mode > 2)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                foreach (var s in _disks)
                {
                    FixedSizeQueue<DiskActivityStatQueue> from = s.HistoryForMode(mode);

                    data.Append(string.Format("<item n=\"{0}\" uuid=\"{1}\">", s.name, s.uuid));
                    foreach (var sample in from.Where(c => c.SampleID > index))
                    {
                        data.Append(string.Format("<s id=\"{0}\" r=\"{1}\" w=\"{2}\" rIOPS=\"{3}\" wIOPS=\"{4}\"></s>", sample.SampleID, sample.read, sample.write, sample.readIOPS, sample.writeIOPS));
                    }
                    data.Append("</item>");
                }
            }
            return data.ToString();
        }

        public void Dispose()
        {
            foreach (var disk in _disks){
                disk.Dispose();
            }
        }
    }
}
