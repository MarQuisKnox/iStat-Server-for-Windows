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
    internal class StatDiskItem
    {   
    	public readonly string name;
    	public readonly string uuid;
        public FixedSizeQueue<DiskStatQueue> history { get; set; }
        public FixedSizeQueue<DiskStatQueue> historyDay { get; set; }

        public StatDiskItem(string n)
        {
            history = new FixedSizeQueue<DiskStatQueue>{ MaxSize = 602 };
            historyDay = new FixedSizeQueue<DiskStatQueue>{ MaxSize = 602 };
            name = n;
            uuid = n.GetHashCode().ToString();
        }
        
        public void UpdateDay(long index)
        {
            AddHistoryItem(history, 4, historyDay, index);
        }

        public FixedSizeQueue<DiskStatQueue> HistoryForMode(int mode)
        {
            if(mode == 0)
                return history;
            if(mode == 2)
                return historyDay;
            return null;
        }

        public void AddHistoryItem(FixedSizeQueue<DiskStatQueue> from, int count, FixedSizeQueue<DiskStatQueue> to, long index)
        {
            if (from.Count() < count)
                return;
            double used = 0;
            double free = 0;
            double size = 0;
	    	for (int i = 1; i <= count; i++)
      		{
      			DiskStatQueue sample = from.ElementAt(history.Count() - i);
                used += sample.used;
                free += sample.free;
                size += sample.size;
      		}
            used /= count;
            free /= count;
            size /= count;

            var hourSample = new DiskStatQueue
            {
                SampleID = index,
                used = used,
                free = free,
                size = size
            };
            to.Enqueue(hourSample);
        }
    }
    
    internal class DiskStatQueue
    {
        public long SampleID { get; set; }
        public double size { get; set; }
        public double used { get; set; }
        public double free { get; set; }
        public float percentage { get; set; }
    }

    internal class StatDisks
    {
        Timer _timer;
        public readonly List<StatDiskItem> _disks = new List<StatDiskItem>();
        public long sessionID;
        public long historyIndex;
        public long historyIndexDay;
        private System.Object _lock = new System.Object();

        public StatDisks()
        {
        	sessionID = DateTime.Now.Ticks;
            historyIndex = -1;
            historyIndexDay = -1;

            _timer = new Timer(Update, null, 0, 36000);
            Update(null);
        }

        public void Update(object sender)
        {
            lock (_lock)
            {
                if (historyIndex >= 4 && (historyIndex % 4) == 0)
                    historyIndexDay++;

                historyIndex++;

                DriveInfo[] allDrives = DriveInfo.GetDrives();
                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveType != DriveType.Fixed)
                        continue;

                    AddDisk(d);
                }

                foreach (StatDiskItem disk in _disks)
                {
                    if (historyIndex >= 4 && (historyIndex % 4) == 0)
                        disk.UpdateDay(historyIndexDay);
                }
            }
        }

        public void AddDisk(DriveInfo d)
        {
            	bool found = false;
				foreach (var s in _disks){
                    if (s.name == d.Name)
                    {
						found = true;
					}
				}
				if(found == false){
                    var disk = new StatDiskItem(d.Name);
                    _disks.Add(disk);
        			sessionID = DateTime.Now.Ticks;
				}
				foreach (var s in _disks){
					if(s.name == d.Name){
						 var ns = new DiskStatQueue
                         {
                             size = d.TotalSize / 1048576,
                        	 used = (d.TotalSize - d.AvailableFreeSpace) / 1048576,
                             free = d.AvailableFreeSpace / 1048576,
                             percentage = 100 - ((d.TotalFreeSpace * 100 / d.TotalSize)),
                             SampleID = historyIndex
                         };
            			 s.history.Enqueue(ns);
					}
				}
        }
       
        public long HistoryIndexForMode(int mode)
        {
            if (mode == 0)
                return historyIndex;
             if (mode == 2)
                return historyIndexDay;
            return 0;
        }

        public string samples(int mode, long index)
        {
            if (mode > 2 || mode == 1)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                foreach (var s in _disks)
                {
                    FixedSizeQueue<DiskStatQueue> from = s.HistoryForMode(mode);

                    data.Append(string.Format("<item n=\"{0}\" uuid=\"{1}\">", s.name, s.uuid));
                    var diskSample = from.Last();
                    if (index == -1)
                    {
                        data.Append(string.Format("<s id=\"{0}\" p=\"{1}\" u=\"{2}\" s=\"{3}\" f=\"{4}\"></s>", diskSample.SampleID, diskSample.percentage, diskSample.used, diskSample.size, diskSample.free));
                    }
                    else
                    {
                        foreach (var sample in from.Where(c => c.SampleID > index))
                        {
                            data.Append(string.Format("<s id=\"{0}\" p=\"{1}\" u=\"{2}\" s=\"{3}\" f=\"{4}\"></s>", sample.SampleID, sample.percentage, sample.used, sample.size, sample.free));
                        }
                    }
                    data.Append("</item>");
                }
            }
            return data.ToString();
        }

        public void Dispose()
        {
        }
    }
}
