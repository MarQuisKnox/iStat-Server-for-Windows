using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;
using System.Text;
using System.Xml;

namespace iStatServerService
{
    internal class StatSensorItem
    {   
    	public readonly string name;
        public readonly int type;
        public FixedSizeQueue<SensorStatQueue> history { get; set; }
        public FixedSizeQueue<SensorStatQueue> historyHour { get; set; }
        public FixedSizeQueue<SensorStatQueue> historyDay { get; set; }

        public StatSensorItem(string n, int t)
        {
        	name = n;
            type = t;
            history = new FixedSizeQueue<SensorStatQueue>{ MaxSize = 602 };
            historyHour = new FixedSizeQueue<SensorStatQueue> { MaxSize = 602 };
            historyDay = new FixedSizeQueue<SensorStatQueue> { MaxSize = 602 };
        }

        public void UpdateHour(long index)
        {
            AddHistoryItem(history, 6, historyHour, index);
        }
        
        public void UpdateDay(long index)
        {
            AddHistoryItem(history, 144, historyDay, index);
        }

        public FixedSizeQueue<SensorStatQueue> HistoryForMode(int mode)
        {
            if(mode == 0)
                return history;
            if(mode == 1)
                return historyHour;
            if(mode == 2)
                return historyDay;
            return null;
        }

        public void AddHistoryItem(FixedSizeQueue<SensorStatQueue> from, int count, FixedSizeQueue<SensorStatQueue> to, long index)
        {
            if (from.Count() < count)
                return;

	    	float value = 0;
	    	for (int i = 1; i <= count; i++)
      		{
      			SensorStatQueue sample = from.ElementAt(history.Count() - i);
                value += sample.Value;
      		}
            value /= count;

            var hourSample = new SensorStatQueue
            {
                SampleID = index,
                Value = value
            };
            to.Enqueue(hourSample);
        }
    }
    
    internal class SensorStatQueue
    {
        public long SampleID { get; set; }
        public float Value { get; set; }
    }



    internal class StatSensors
    {
        Timer _timer;
        private readonly Computer _computer = new Computer();
        private readonly UpdateVisitor _visitor = new UpdateVisitor();
        private System.Object _lock = new System.Object();

        public readonly List<StatSensorItem> _sensors = new List<StatSensorItem>();
        public long sessionID;
        public long historyIndex;
        public long historyIndexHour;
        public long historyIndexDay;

        public StatSensors()
        {
            _computer.CPUEnabled = true;
            _computer.GPUEnabled = true;
            _computer.MainboardEnabled = true;
            _computer.HDDEnabled = true;
            _computer.Open();
            _computer.Accept(_visitor);

        	sessionID = DateTime.Now.Ticks;
            historyIndex = -1;
            historyIndexHour = -1;
            historyIndexDay = -1;

            _timer = new Timer(Update, null, 0, 6000);
            Update(null);
        }

        public void Dispose()
        {
            _computer.Close();
        }

        public void Update(object sender)
        {
            lock (_lock)
            {
                if (historyIndex >= 6 && (historyIndex % 6) == 0)
                    historyIndexHour++;
                if (historyIndex >= 144 && (historyIndex % 144) == 0)
                    historyIndexDay++;

                historyIndex++;

                _computer.Accept(_visitor); //Update mechanism for OpenHardwareMonitorLib

                var moboTempSensors = from h in _computer.Hardware
                                      where h.HardwareType == HardwareType.Mainboard
                                      from sh in h.SubHardware
                                      where sh.HardwareType == HardwareType.SuperIO
                                      from ts in sh.Sensors
                                      where (ts.SensorType == SensorType.Temperature || ts.SensorType == SensorType.Fan)
                                      select ts;


                var gpuTempSensors = from h in _computer.Hardware
                                     where
                                         (h.HardwareType == HardwareType.GpuAti | h.HardwareType == HardwareType.GpuNvidia | h.HardwareType == HardwareType.HDD | h.HardwareType == HardwareType.CPU)
                                     from s in h.Sensors
                                     where (s.SensorType == SensorType.Temperature || s.SensorType == SensorType.Fan)
                                     select s;
                var allTemps = moboTempSensors.Concat(gpuTempSensors);
                foreach (var s in allTemps)
                {
                    if (s.Value == null || s.Name == null)
                        continue;
                    if (s.SensorType != SensorType.Fan && s.SensorType != SensorType.Temperature)
                        continue;
                    int type = 0;
                    if (s.SensorType == SensorType.Fan)
                        type = 2;
                    AddSensor(s.Name, s.Value.Value, type);
                }

                foreach (StatSensorItem sensor in _sensors)
                {
                    if (historyIndex >= 6 && (historyIndex % 6) == 0)
                        sensor.UpdateHour(historyIndexHour);

                    if (historyIndex >= 144 && (historyIndex % 144) == 0)
                        sensor.UpdateDay(historyIndexDay);
                }
            }
        }

        public void AddSensor(string name, float value, int type)
        {
            	bool found = false;
				foreach (var s in _sensors){
					if(s.name == name){
						found = true;
					}
				}
				if(found == false){
					var sensor = new StatSensorItem(name, type);
                    _sensors.Add(sensor);
        			sessionID = DateTime.Now.Ticks;
				}
				foreach (var s in _sensors){
					if(s.name == name){
						 var ss = new SensorStatQueue
                         {
                             SampleID = historyIndex,
                             Value = value
                         };
            			s.history.Enqueue(ss);
					}
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

        public string samples(int mode, int index)
        {
            if (mode > 2)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                foreach (var s in _sensors)
                {
                    FixedSizeQueue<SensorStatQueue> from = s.HistoryForMode(mode);

                    data.Append(string.Format("<item i=\"0\" n=\"{0}\" type=\"{1}\" uuid=\"{2}\">", s.name, s.type, s.name));
                    var sensorSample = from.Last();
                    if (index == -1)
                    {
                        if(sensorSample != null)
                            data.Append(string.Format("<s id=\"{0}\" v=\"{1}\"></s>", sensorSample.SampleID, sensorSample.Value));
                    }
                    else
                    {
                        foreach (var sample in from.Where(c => c.SampleID > index))
                        {
                            data.Append(string.Format("<s id=\"{0}\" v=\"{1}\"></s>", sample.SampleID, sample.Value));
                        }
                    }
                    data.Append("</item>");
                }
            }
			return data.ToString();
        }
    }
}
