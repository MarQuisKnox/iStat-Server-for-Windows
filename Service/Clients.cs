using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace iStatServerService
{
    /// <summary>
    /// Persists a list of authenticated clients
    /// </summary>
    public class Clients : IDisposable
    {
        static Clients instance = null;
        static readonly object padlock = new object();
        public string _path;
        public List<Client> _clients = new List<Client>();
        public List<string> _authorized = new List<string>();
        private const string CLIENTS_FILE_NAME = "clients.xml";
        static X509Certificate serverCertificate = null;
        private string _uuid;

        Clients()
        {
        }
        
        public static Clients Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new Clients();
                    }
                    return instance;
                }
            }
        }

        public X509Certificate ServerCertificate()
        {
            return serverCertificate;
        }

        public string ServerUUID()
        {
            return _uuid;
        }

        public void LoadUUID(string dataBasePath)
        {
             // Check if uuid exists
            if (!File.Exists(dataBasePath + @"\uuid.txt"))
            {
                // Create uuid
                Guid g = Guid.NewGuid();
                _uuid = g.ToString();
                File.WriteAllText(dataBasePath + @"\uuid.txt", _uuid);
             }

            // Read uuid
            _uuid = File.ReadAllText(dataBasePath + @"\uuid.txt");
        }

        public void LoadCertificate(string dataBasePath)
        {
            // Check if certificate exists
            if (!File.Exists(dataBasePath + @"\iss.pfx"))
            {
                // Create certificate
                byte[] c = Certificate.CreateSelfSignCertificatePfx(
                    "CN=istatserverdotnet", //host name
                    DateTime.Parse("2000-01-01"), //not valid before
                    DateTime.Parse("2020-01-01"), //not valid after
                    "abcdefghijklmnopqrstuvwxyz"); //password to encrypt key file

                // Save certiciate
                using (BinaryWriter binWriter = new BinaryWriter(File.Open(dataBasePath + @"\iss.pfx", FileMode.Create)))
                {
                    binWriter.Write(c);
                }
            }
            serverCertificate = new X509Certificate2(dataBasePath + @"\iss.pfx", "abcdefghijklmnopqrstuvwxyz");
        }

        public void LoadAuthorizedClients(string databasePath)
        {
            _path = databasePath + @"\" + CLIENTS_FILE_NAME;
            if (File.Exists(_path))
            {
                var ser = new XmlSerializer(typeof(List<string>));
                try
                {
                    using (TextReader tr = File.OpenText(_path))
                    {
                        _authorized = (List<String>)ser.Deserialize(tr);
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLineIf(Program.LogLevel.Level >= TraceLevel.Error, string.Format("Corrupted clients file at: {0}.  Error was: {1}", _path, ex.Message), "Exception");
                    File.Delete(_path);
                }
            }
        }


        public void AddClient(Client client)
        {
            lock (_clients)
            {
                _clients.Add(client);
            }
        }

        public void RemoveClient(Client client)
        {
            lock (_clients)
            {
                _clients.Remove(client);
            }
        }

        /// <summary>
        /// Adds a new client after they have been authenticated.  Persists to disk.
        /// </summary>
        /// <param name="duuid">duuid of client</param>
        public void AddAuthorizedClient(string duuid)
        {
            lock (_authorized)
            {
                if (!_authorized.Contains(duuid))
                {
                    _authorized.Add(duuid);
                    Save();
                }
            }
        }

        private void Save()
        {
            var ser = new XmlSerializer(typeof(List<string>));
            if (File.Exists(_path))
                File.Delete(_path);

            using (FileStream fs = File.OpenWrite(_path))
            {
                using (var sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    lock (_authorized)
                    {
                        ser.Serialize(sw, _authorized);
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if the clients has been previously authenticated
        /// </summary>
        /// <param name="duuid">duuid of client</param>
        /// <returns>true if authenticated, false if not</returns>
        public bool IsClientAuthenticated(string duuid)
        {
            lock (_authorized)
            {
                return _authorized.Contains(duuid);
            }
        }
        
        /// <summary>
        /// Removes all existing client associations
        /// </summary>
        public void ResetAuthorizations()
        {
            lock (_authorized)
            {
                _authorized.Clear();
            }
            Save();
        }

        public void CloseConnections()
        {
            foreach (var client in _clients)
            {
                client.Disconnect();
            }
        }

        public void Dispose()
        {
            CloseConnections();
        }
    }
}
