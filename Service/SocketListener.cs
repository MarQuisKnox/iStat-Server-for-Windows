using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deusty.Net;
using System.Net;
using System.Net.Sockets;
using Bonjour;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
using NATUPNPLib;
using System.Text.RegularExpressions;

namespace iStatServerService
{
    class SocketListener : IDisposable
    {
        static SocketListener instance = null;
        static readonly object padlock = new object();

        private AsyncSocket listenSocket;
        Bonjour.DNSSDService bonjourService;
        Bonjour.DNSSDService bonjourServiceRegistrar;
 
        SocketListener()
        {
        }

        public static SocketListener Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new SocketListener();
                    }
                    return instance;
                }
            }
        }

        public void Dispose()
        {
            StopSocket();
        }

        [HandleProcessCorruptedStateExceptions]
        public void StopSocket()
        {
            listenSocket.Close();
            listenSocket = null;
        }

        public void StartSocket()
        {
            listenSocket = new AsyncSocket();
            listenSocket.AllowMultithreadedCallbacks = true;
            listenSocket.DidAccept += new AsyncSocket.SocketDidAccept(listenSocket_DidAccept);
            Exception error;
            if (!listenSocket.Accept(ushort.Parse(Preferences.Instance.Value("port")), out error))
            {
                Logger.Instance.eventLog.WriteEntry(String.Format("Error starting server: {0}", error), EventLogEntryType.Information);
                return;
            }

            PublishBonjour();
        }

        private void listenSocket_DidAccept(AsyncSocket sender, AsyncSocket newSocket)
        {
            Client client = new Client(newSocket);
            Clients.Instance.AddClient(client);
        }

        public void PublishBonjour()
        {
            Logger.Instance.eventLog.WriteEntry("Starting Bonjour", EventLogEntryType.Information);
            try
            {
                bonjourService = new DNSSDService();
            }
            catch
            {
                return;
            }
            String type = "_istatserv._tcp";
            String name = String.Format("{0},{1}", Clients.Instance.ServerUUID(), Environment.MachineName);

            Bonjour.TXTRecord record = new TXTRecord();
            record.SetValue("model", "windows");
            record.SetValue("time", String.Format("{0}", DateTime.Now.Ticks));
            //manager.ServiceRegistered += new _IDNSSDEvents_ServiceRegisteredEventHandler(this.ServiceRegistered);
            Logger.Instance.eventLog.WriteEntry("Registering Bonjour Service", EventLogEntryType.Information);
            bonjourServiceRegistrar = bonjourService.Register(0, 0, name, type, null, null, ushort.Parse(Preferences.Instance.Value("port")), record, null);
        }

        public void ServiceRegistered(DNSSDService service, DNSSDFlags flags, String name, String regType, String domain)
        {
            Logger.Instance.eventLog.WriteEntry("Bonjour service running", EventLogEntryType.Information);
        }

        public void RemoveUPNPMappings()
        {
            Logger.Instance.eventLog.WriteEntry("Removing Exisiting UPNP Mappings", EventLogEntryType.Information);
            string description = String.Format("iStatServerDotNet-{0}", Preferences.Instance.Value("upnpDescription"));
            NATUPNPLib.UPnPNAT upnpnat = new NATUPNPLib.UPnPNAT();
            NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;
            foreach (NATUPNPLib.IStaticPortMapping portMapping in mappings)
            {
                if (portMapping.Description.Contains(description))
                {
                    mappings.Remove(portMapping.ExternalPort, "TCP");
                }
            }
            Logger.Instance.eventLog.WriteEntry("Finished Removing UPNP Mappings", EventLogEntryType.Information);
        }

        public void ReloadUPNP()
        {
            Logger.Instance.eventLog.WriteEntry("Updating UPNP", EventLogEntryType.Information);
            RemoveUPNPMappings();

            if (int.Parse(Preferences.Instance.Value("upnpEnabled")) == 0)
                return;

            int publicPort = int.Parse(Preferences.Instance.Value("upnpPort"));
            int privatePort = int.Parse(Preferences.Instance.Value("port"));
            string description = String.Format("iStatServerDotNet-{0}", Preferences.Instance.Value("upnpDescription"));

            NATUPNPLib.UPnPNAT upnpnat = new NATUPNPLib.UPnPNAT();
            NATUPNPLib.IStaticPortMappingCollection mappings = upnpnat.StaticPortMappingCollection;

            Logger.Instance.eventLog.WriteEntry("Fetching UPNP Addresses", EventLogEntryType.Information);
            var addresses = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
            foreach (var address in addresses)
            {
                string addressString = address.ToString();
                Match match = Regex.Match(addressString, @"([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    mappings.Add(publicPort, "TCP", privatePort, addressString, true, description);
                }
            }
            Logger.Instance.eventLog.WriteEntry("UPNP Complete", EventLogEntryType.Information);
        }
    }
}
