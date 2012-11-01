using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace iStatServerService
{
    internal class StatNetworkItem
    {   
    	public readonly string name;
        public string address;

        public FixedSizeQueue<NetworkStatQueue> historyTotals { get; set; }
        public FixedSizeQueue<NetworkStatQueue> history { get; set; }
        public FixedSizeQueue<NetworkStatQueue> historyHour { get; set; }
        public FixedSizeQueue<NetworkStatQueue> historyDay { get; set; }
        
        public StatNetworkItem(string n)
        {
            historyTotals = new FixedSizeQueue<NetworkStatQueue> { MaxSize = 602 };
            history = new FixedSizeQueue<NetworkStatQueue> { MaxSize = 602 };
            historyHour = new FixedSizeQueue<NetworkStatQueue>{ MaxSize = 602 };
            historyDay = new FixedSizeQueue<NetworkStatQueue>{ MaxSize = 602 };
            name = n;
        }

        // Takes the last 2 samples and gets the current bandwidth values
        // The converted bandwidth is used to create the hour and day history instead of using the totals
        public void UpdateBandwidth()
        {
            if (historyTotals.Count() < 2)
                return;

            NetworkStatQueue sample = historyTotals.ElementAt(historyTotals.Count() - 1);
            NetworkStatQueue sample2 = historyTotals.ElementAt(historyTotals.Count() - 2);
            double down = sample.Download - sample2.Download;
            double up = sample.Upload - sample2.Upload;
            var hourSample = new NetworkStatQueue
            {
                SampleID = sample.SampleID,
                Download = down,
                Upload = up
            };
            history.Enqueue(hourSample);
        }
        
        public void UpdateHour(long index)
        {
            AddHistoryItem(history, 6, historyHour, index);
        }
        
        public void UpdateDay(long index)
        {
            AddHistoryItem(history, 144, historyDay, index);
        }

        public FixedSizeQueue<NetworkStatQueue> HistoryForMode(int mode)
        {
            if(mode == 0)
                return historyTotals;
            if(mode == 1)
                return historyHour;
            if(mode == 2)
                return historyDay;
            return null;
        }

        public void AddHistoryItem(FixedSizeQueue<NetworkStatQueue> from, int count, FixedSizeQueue<NetworkStatQueue> to, long index)
        {
            if (from.Count() < count)
                return;
            double down = 0;
            double up = 0;
            for (int i = 1; i <= count; i++)
            {
                NetworkStatQueue sample = from.ElementAt(history.Count() - i);
                down += sample.Download;
                up += sample.Upload;
            }
            down /= count;
            up /= count;

            var hourSample = new NetworkStatQueue
            {
                SampleID = index,
                Download = down,
                Upload = up
            };
            to.Enqueue(hourSample);
        }
    }
    
    internal class NetworkStatQueue
    {
        public long SampleID { get; set; }
        public double Upload { get; set; }
        public double Download { get; set; }
    }

    internal class StatNetwork
    {
        Timer _timer;
        public readonly List<StatNetworkItem> _interfaces = new List<StatNetworkItem>();
        public long sessionID;
        public long historyIndex;
        public long historyIndexHour;
        public long historyIndexDay;
        public bool resetAddresses;
        private System.Object _lock = new System.Object();

        public StatNetwork()
        {
        	sessionID = DateTime.Now.Ticks;
        	historyIndex = -1;
        	historyIndexHour = -1;
        	historyIndexDay = -1;
            resetAddresses = true;
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(AddressChangedCallback);
  
            _timer = new Timer(Update, null, 0, 1000);
            Update(null);
        }

        public void AddressChangedCallback(object sender, EventArgs e)
        {
            resetAddresses = true;
            sessionID = DateTime.Now.Ticks;
        }

        public void Update(object sender)
        {
            lock (_lock)
            {
                historyIndex++;
                if (historyIndex >= 6 && (historyIndex % 6) == 0)
                    historyIndexHour++;
                if (historyIndex >= 144 && (historyIndex % 144) == 0)
                    historyIndexDay++;

                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in nics)
                {
                    AddInterface(adapter);
                }

                foreach (StatNetworkItem inf in _interfaces)
                {
                    if (historyIndex >= 6 && (historyIndex % 6) == 0)
                        inf.UpdateHour(historyIndexHour);

                    if (historyIndex >= 144 && (historyIndex % 144) == 0)
                        inf.UpdateDay(historyIndexDay);
                }
            }
            resetAddresses = false;
        }

        public void AddInterface(NetworkInterface adapter)
        {
            if (adapter.NetworkInterfaceType == NetworkInterfaceType.Tunnel || adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                return;
            if (adapter.OperationalStatus == OperationalStatus.Down)
                return;
            
            	bool found = false;
				foreach (var s in _interfaces){
                    if (s.name == adapter.Name)
                    {
						found = true;
					}
				}
				if(found == false){
                    var inf = new StatNetworkItem(adapter.Name);
                     _interfaces.Add(inf);
                    sessionID = DateTime.Now.Ticks;
                }

				foreach (var s in _interfaces){
					if(s.name == adapter.Name){
                        if (resetAddresses)
                        {
                            s.address = null;
                            IPInterfaceProperties properties = adapter.GetIPProperties();
                            UnicastIPAddressInformationCollection uniCast = properties.UnicastAddresses;
                            foreach (UnicastIPAddressInformation uni in uniCast)
                            {
                                var address = uni.Address.ToString();
                                if (address.Contains("127."))
                                    continue;

                                Match match = Regex.Match(address, @"([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})", RegexOptions.IgnoreCase);
                                if (match.Success)
                                {
                                    s.address = address;
                                }
                            }
                        }

						 var ns = new NetworkStatQueue
                         {
                        	 Upload = adapter.GetIPv4Statistics().BytesSent,
                             Download = adapter.GetIPv4Statistics().BytesReceived,
                             SampleID = historyIndex
                         };
            			s.historyTotals.Enqueue(ns);
                        s.UpdateBandwidth();
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

        public string samples(int mode, long index)
        {
            if (mode > 2)
                return "";

            var data = new StringBuilder();
            lock (_lock)
            {
                foreach (var s in _interfaces)
                {
                    FixedSizeQueue<NetworkStatQueue> from = s.HistoryForMode(mode);

                    var address = s.address;
                    if (address == null)
                        address = "";
                    data.Append(string.Format("<item name=\"{0}\" ip=\"{1}\" uuid=\"{2}\">", s.name, address, s.name));

                    foreach (var ns in from.Where(c => c.SampleID > index))
                    {
                        data.Append(string.Format("<s id=\"{0}\" d=\"{1}\" u=\"{2}\"></s>", ns.SampleID, ns.Download, ns.Upload));
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
