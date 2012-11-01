using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace iStatServerService
{
    internal class Stat : IDisposable
    {
        static Stat instance = null;
        static readonly object padlock = new object();

        public long FirstUptime;
        public long CurrentUptime;

        public StatSensors SENSORS;
        public StatNetwork NET;
        public StatDisks DISKS;
        public StatCPU CPU;
        public StatMemory MEMORY;
        public StatDiskActivity ACTIVTY;
        
        private readonly PerformanceCounter _uptimeCounter = new PerformanceCounter { CategoryName = "System", CounterName = "System Up Time" };
      
        Timer _statUpdateTimer;

        Stat()
        {
        }
        
        public static Stat Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Stat();
                    }
                    return instance;
                }
            }
        }

        public void LoadStats()
        {
            _uptimeCounter.NextValue();

            Logger.Instance.eventLog.WriteEntry("Loading CPU", EventLogEntryType.Information);
            CPU = new StatCPU();
            Logger.Instance.eventLog.WriteEntry("Loading Memory", EventLogEntryType.Information);
            MEMORY = new StatMemory();
            Logger.Instance.eventLog.WriteEntry("Loading Sensors", EventLogEntryType.Information);
            SENSORS = new StatSensors();
            Logger.Instance.eventLog.WriteEntry("Loading Network", EventLogEntryType.Information);
            NET = new StatNetwork();
            Logger.Instance.eventLog.WriteEntry("Loading Disks", EventLogEntryType.Information);
            DISKS = new StatDisks();
            Logger.Instance.eventLog.WriteEntry("Loading Activity", EventLogEntryType.Information);
            ACTIVTY = new StatDiskActivity();

            PopulateUptime();
            FirstUptime = CurrentUptime;
 
            _statUpdateTimer = new Timer(TimerTick, null, 0, 1000);
            Logger.Instance.eventLog.WriteEntry("Finished Loading Stat Collectors", EventLogEntryType.Information);
        }
        
        private double GetUnixSeconds()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public void TimerTick(object sender)
        {
            PopulateUptime();
        }

        private void PopulateUptime()
        {
            CurrentUptime = Convert.ToInt64(_uptimeCounter.NextValue());
        }

        public void Dispose()
        {
            _statUpdateTimer.Dispose();
            CPU.Dispose();
            MEMORY.Dispose();
            NET.Dispose();
            DISKS.Dispose();
            ACTIVTY.Dispose();
            SENSORS.Dispose();
            PerformanceCounter.CloseSharedResources();
        }
    }
}
