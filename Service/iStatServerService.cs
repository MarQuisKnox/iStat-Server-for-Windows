using System;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ServiceProcess;
using System.ServiceModel;
using System.Diagnostics;

namespace iStatServerService
{
     public class iStatServerService : ServiceBase
    {
        private const string CLIENTS_SUBDIRECTORY = @"\iStatServer";
        ServiceHost host;

        static void Main()
        {
            ServiceBase.Run(new iStatServerService());
        }
                     
        public iStatServerService()
        {
            Logger.Instance.OpenLog();
            
            this.ServiceName = "iStatServerService";
            this.CanShutdown = true;
            this.CanStop = true;
        }
        
        protected override void OnStart(string[] args)
        {
            Logger.Instance.eventLog.WriteEntry("iStat Server Starting...", EventLogEntryType.Information);
            string dataBasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + CLIENTS_SUBDIRECTORY;
 
            // Check is support folder exists
            if (!Directory.Exists(dataBasePath))
                Directory.CreateDirectory(dataBasePath);

            Logger.Instance.eventLog.WriteEntry("Loading Preferences", EventLogEntryType.Information);
            Preferences.Instance.LoadDefaults();
            Preferences.Instance.Load(dataBasePath);

            Logger.Instance.eventLog.WriteEntry("Loading Clients Database", EventLogEntryType.Information);
            Clients.Instance.LoadUUID(dataBasePath);
            Clients.Instance.LoadAuthorizedClients(dataBasePath);
            Logger.Instance.eventLog.WriteEntry("Loading SSL Certificate", EventLogEntryType.Information);
            Clients.Instance.LoadCertificate(dataBasePath);
            Logger.Instance.eventLog.WriteEntry("Loading Stat Collectors", EventLogEntryType.Information);
            Stat.Instance.LoadStats();

            Logger.Instance.eventLog.WriteEntry("Creating Pipe", EventLogEntryType.Information);
            host = new ServiceHost(typeof(IstatServerProxy), new Uri[] { new Uri("net.pipe://localhost") });
            host.AddServiceEndpoint(typeof(IIstatServerProxy), new NetNamedPipeBinding(), "istatserver");
            host.Open();

            Logger.Instance.eventLog.WriteEntry("Opening Socket", EventLogEntryType.Information);
            SocketListener.Instance.StartSocket();
            Logger.Instance.eventLog.WriteEntry("iStat Server Started", EventLogEntryType.Information);
        }

        protected override void Dispose(bool disposing)
        {
        }

        protected override void OnStop()
        {
            Logger.Instance.eventLog.WriteEntry("iStat Server Stopping...", EventLogEntryType.Information);
            try
            {
                Stat.Instance.Dispose();
                Clients.Instance.Dispose();
                SocketListener.Instance.Dispose();
                host.Close();
            }
            catch
            {
                Logger.Instance.eventLog.WriteEntry("iStat Server Stopping - Exception", EventLogEntryType.Warning);
            }
            finally
            {
            }
            Logger.Instance.eventLog.WriteEntry("Stopped", EventLogEntryType.Information);
        }

        protected override void OnShutdown()
        {
        }
    }

     [ServiceContract]
     public interface IIstatServerProxy
     {
         [OperationContract]
         void SetPasscode(string passcode);
         [OperationContract]
         void ResetAuthorizations();
         [OperationContract]
         void SetPort(string port);
         [OperationContract]
         void SetUPNPPort(string port);
         [OperationContract]
         void SetUPNPEnabled(bool enabled);
         [OperationContract]
         string Value(string key);
     }

     public class IstatServerProxy : IIstatServerProxy
     {
         public string Value(string key)
         {
             return Preferences.Instance.Value(key);
         }

         public void SetPasscode(string passcode)
         {
             Preferences.Instance.Set("pin", passcode);
         }

         public void ResetAuthorizations()
         {
             Clients.Instance.ResetAuthorizations();
         }

         public void SetPort(string port)
         {
             Preferences.Instance.Set("port", port);
             SocketListener.Instance.StopSocket();
             Clients.Instance.CloseConnections();
             SocketListener.Instance.StartSocket();
             SocketListener.Instance.ReloadUPNP();
         }


         public void SetUPNPPort(string port)
         {
             Preferences.Instance.Set("upnpPort", port);
             SocketListener.Instance.ReloadUPNP();
         }

         public void SetUPNPEnabled(bool enabled)
         {
             if(enabled)
                 Preferences.Instance.Set("upnpEnabled", "1");
             else
                 Preferences.Instance.Set("upnpEnabled", "0");
             SocketListener.Instance.ReloadUPNP();
         }
     }
}
