using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Deusty.Net;

namespace iStatServerService
{
    public class Client
    {
        private const string SERVER_VERSION = "1.91";
        private const string HEADER = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        private const string AUTHORIZE = "<isr protocol=\"2\" ath=\"{0}\" ss=\"{1}\" uuid=\"{2}\" v=\"{3}\" pl=\"5\" model=\"windows\"></isr>";
        private const string NO_AUTHORIZE = "<isr protocol=\"2\" ss=\"{0}\" uuid=\"{1}\" v=\"{2}\" pl=\"5\" model=\"windows\"></isr>";
        private const string ACCEPT_CODE = "<isr ready=\"1\"></isr>";
        private const string REJECT_CODE = "<isr athrej=\"1\"></isr>";
        private const string SESSION = "<isr>";
        private const string STAT = "<stat type=\"{0}\" interval=\"{1}\" id=\"{2}\" sessionID=\"{3}\">";

		private AsyncSocket socket;

        private string uuid;
        private long sessionID;
        float version;
        private string hostname;
        
        public Client(AsyncSocket sock)
        {
        	socket = sock;
        	socket.DidRead += new AsyncSocket.SocketDidRead(asyncSocket_DidRead);
			socket.DidWrite += new AsyncSocket.SocketDidWrite(asyncSocket_DidWrite);
			socket.WillClose += new AsyncSocket.SocketWillClose(asyncSocket_WillClose);
			socket.DidClose += new AsyncSocket.SocketDidClose(asyncSocket_DidClose);

            socket.Read(Client.Terminator, 120 * 1000, 1);

            sessionID = DateTime.Now.Ticks;
            uuid = null;
        }

        public void Disconnect()
        {
            socket.Close();
        }

		public static byte[] Terminator
		{
			get { return Encoding.UTF8.GetBytes("</isr>"); }
		}

		private void asyncSocket_DidWrite(AsyncSocket sender, long tag)
		{
		}

		private void asyncSocket_DidRead(AsyncSocket sender, byte[] data, long tag)
		{
			String msg = null;
			try
			{
				msg = Encoding.UTF8.GetString(data);
                HandleMessage(msg);
			}
			catch(Exception e)
			{
//				LogError("Error converting received data into UTF-8 String: {0}", e);
			}
            socket.Read(Client.Terminator, 120 * 1000, 1);
        }

		private void asyncSocket_WillClose(AsyncSocket sender, Exception e)
		{
            Logger.Instance.eventLog.WriteEntry("Client got socket exception : " + e.Message, EventLogEntryType.Error);
        }

		private void asyncSocket_DidClose(AsyncSocket sender)
		{
            Logger.Instance.eventLog.WriteEntry("Client Disconnected", EventLogEntryType.Information);
            Clients.Instance.RemoveClient(this);
		}

        /// <summary>
        /// Determines the appropriate action to take for different message types
        /// </summary>
        /// <param name="message">Message from the client</param>
        /// <param name="stream">Active network stream to client</param>
        internal void HandleMessage(string message)
        {
            var doc = new XmlDocument();
            using (var sr = new StringReader(message))
            {
                try
                {
                    doc.Load(sr);
                }
                catch (Exception ex)
                {
                    Logger.Instance.eventLog.WriteEntry("Client got bad data", EventLogEntryType.Information);
                    Logger.Instance.eventLog.WriteEntry(message, EventLogEntryType.Information);
                   	return;
                }

            }

            XmlNode o = doc.GetElementsByTagName("isr")[0];
            if (o == null)
                return;

            if (o.ChildNodes.Count == 0)
            {
                Logger.Instance.eventLog.WriteEntry("Got empty request", EventLogEntryType.Information);
                ReturnData(o);
                return;
            }
            XmlNode n = o.FirstChild;

            switch (n.Name)
            {
                case "pcd":  //passcode
                    CheckPasscode(n.InnerText);
                    break;
                case "h":  //client initial connection
                    Authorize(doc);
                    break;
                default: //data request
                    ReturnData(o);
                    break;
            }
        }

        private void CheckPasscode(string message)
        {
            if (message == Preferences.Instance.Value("pin")) //code correct
            {
                Clients.Instance.AddAuthorizedClient(uuid);
                Trace.WriteLineIf(Program.LogLevel.Level >= TraceLevel.Verbose, "New client added: " + uuid, "Message");
                const string toClient = HEADER + ACCEPT_CODE;
                byte[] data = Encoding.UTF8.GetBytes(toClient);
                Trace.WriteLineIf(Program.LogLevel.Level >= TraceLevel.Verbose, "Server (accept authorization): " + toClient, "Message");
	 			socket.Write(data, 120 * 1000, 1);
            }
            else //code rejected
            {
                const string toClient = HEADER + REJECT_CODE;
                byte[] data = Encoding.UTF8.GetBytes(toClient);
                Trace.WriteLineIf(Program.LogLevel.Level >= TraceLevel.Verbose, "Server (reject authorization): " + toClient, "Message");
	 			socket.Write(data, 120 * 1000, 1);
            }
        }

        private void ReturnData(XmlNode isr)
        {
            var data = new StringBuilder();
            data.Append(HEADER);
            data.Append(SESSION);
            foreach (XmlNode n in isr.ChildNodes)
            {
                if (n.Name == "stat")
                {
                    string statType = n.Attributes["type"].Value;
                    string[] statSamples = n.Attributes["samples"].Value.Split('|');

                    int index = 0;
                    switch (statType)
                    {
                        case "cpu":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "cpu", index, Stat.Instance.CPU.HistoryIndexForMode(index), Stat.Instance.SENSORS.sessionID));
                                    data.Append(Stat.Instance.CPU.samples(index, int.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "memory":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "memory", index, Stat.Instance.MEMORY.HistoryIndexForMode(index), Stat.Instance.MEMORY.sessionID));
                                    data.Append(Stat.Instance.MEMORY.samples(index, int.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "sensors":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "sensors", index, Stat.Instance.SENSORS.HistoryIndexForMode(index), Stat.Instance.SENSORS.sessionID));
                                    data.Append(Stat.Instance.SENSORS.samples(index, int.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "network":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "network", index, Stat.Instance.NET.HistoryIndexForMode(index), Stat.Instance.NET.sessionID));
                                    data.Append(Stat.Instance.NET.samples(index, long.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "disks":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "disks", index, Stat.Instance.DISKS.HistoryIndexForMode(index), Stat.Instance.DISKS.sessionID));
                                    data.Append(Stat.Instance.DISKS.samples(index, long.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "diskactivity":
                            foreach (var sample in statSamples)
                            {
                                data.Append(string.Format(STAT, "diskactivity", index, Stat.Instance.ACTIVTY.HistoryIndexForMode(index), Stat.Instance.ACTIVTY.sessionID));
                                    data.Append(Stat.Instance.ACTIVTY.samples(index, long.Parse(sample)));
                                data.Append("</stat>");
                                index++;
                            }
                            break;
                        case "uptime":
                            data.Append(string.Format("<stat type=\"uptime\"><s u=\"{0}\"></s></stat>", Stat.Instance.CurrentUptime));
                            break;
                    }
                }
            }
            data.Append("</isr>");

            var lengthData = new StringBuilder();
            lengthData.Append(HEADER);
            lengthData.Append("<isr>");
            lengthData.Append(string.Format("<response_len type=\"0\" len=\"{0}\"></response_len>", data.Length));
            lengthData.Append("</isr>");

            byte[] bdataLength = Encoding.UTF8.GetBytes(lengthData.ToString());
            byte[] bdata = Encoding.UTF8.GetBytes(data.ToString());

 			socket.Write(bdataLength, 120 * 1000, 1);
 			socket.Write(bdata, 120 * 1000, 1);
        }

        /// <summary>
        /// Handles authorizing clients.  On first connect a server set pin code should be requested, and then validated.  Otherwise the duuid is stored and no token is requested.
        /// </summary>
        /// <param name="doc">Client XML message</param>
        /// <param name="stream">Open network stream to client</param>
        /// <returns>Indicates if the client was recognized (if not next response will be plaintext (non-xml) auth token)</returns>
        private bool Authorize(XmlDocument doc)
        {
            // turn on tls as soon as we get the welcome message
            var data = new StringBuilder();
            data.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><isr sec=\"1\"></isr>");
            byte[] bdata = Encoding.UTF8.GetBytes(data.ToString());
 			socket.Write(bdata, 120 * 1000, 1);

            Logger.Instance.eventLog.WriteEntry("Starting TLS", EventLogEntryType.Information);
            socket.StartTLSAsServer(Clients.Instance.ServerCertificate(), null, null);

            XmlNodeList duuidNodes = doc.GetElementsByTagName("duuid");
            uuid = duuidNodes[0].InnerText;

            XmlNodeList versionNodes = doc.GetElementsByTagName("v");
            version = float.Parse(versionNodes[0].InnerText);

            XmlNodeList nameNodes = doc.GetElementsByTagName("h");
            hostname = nameNodes[0].InnerText;

            if (!Clients.Instance.IsClientAuthenticated(uuid)) //new client
            {
                string aS = GetAuthorizeString(true);
                byte[] data2 = Encoding.UTF8.GetBytes(aS);
 				socket.Write(data2, 120 * 1000, 1);
                return true;
            }
            else
            {
                string aS = GetAuthorizeString(false);
                byte[] data2 = Encoding.UTF8.GetBytes(aS);
 				socket.Write(data2, 120 * 1000, 1);
                return true;
            }
        }

        /// <summary>
        /// Generates the authorize response string
        /// </summary>
        /// <param name="newConnection">Has the client been previously authenticated</param>
        /// <param name="uptime">Current uptime</param>
        /// <returns></returns>
        private string GetAuthorizeString(bool newConnection)
        {
            if (newConnection == true)
                return HEADER + String.Format(AUTHORIZE, 0, sessionID, Clients.Instance.ServerUUID(), SERVER_VERSION);
            return HEADER + String.Format(NO_AUTHORIZE, sessionID, Clients.Instance.ServerUUID(), SERVER_VERSION);
        }
    }
}
