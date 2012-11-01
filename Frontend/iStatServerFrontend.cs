using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace iStat_Server
{
    class iStatServerFrontend : IDisposable
    {

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var a = new iStatServerFrontend())
            {
                Application.Run();
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

        private readonly Settings _settingsForm;
        private IIstatServerProxy pipeProxy;

        public iStatServerFrontend()
        {
            _settingsForm = new Settings();
            _settingsForm.PortChanged += (sender, e) => ChangedPort(e.Port);
            _settingsForm.AuthCodeChanged += (sender, e) => ChangedPasscode(e.AuthCode);
            _settingsForm.AuthReset += (sender, e) => ResetAuthorizations();
            _settingsForm.UPNPPortChanged += (sender, e) => UPNPPortChanged(e.Port);
            _settingsForm.UPNPChanged += (sender, e) => UPNPChanged(e.Enabled);

            try
            {
                String pipeName = "net.pipe://localhost/istatserver";
                ChannelFactory<IIstatServerProxy> pipeFactory = new ChannelFactory<IIstatServerProxy>(new NetNamedPipeBinding(), new EndpointAddress(pipeName));
                pipeProxy = pipeFactory.CreateChannel();

                _settingsForm.SetPasscodeText(pipeProxy.Value("pin"));
                _settingsForm.SetPortText(pipeProxy.Value("port"));
                _settingsForm.SetUPNPPortText(pipeProxy.Value("upnpPort"));
                if (int.Parse(pipeProxy.Value("upnpEnabled")) == 1)
                    _settingsForm.SetUPNPEnabled(true);
                else
                    _settingsForm.SetUPNPEnabled(false);
            }
            catch
            {
            }
            _settingsForm.Show();
        }

        private void UPNPChanged(bool enabled)
        {
            try
            {
                pipeProxy.SetUPNPEnabled(enabled);
            }
            catch (Exception e)
            {
            }
        }

        private void UPNPPortChanged(string port)
        {
            try
            {
                pipeProxy.SetUPNPPort(port);
            }
            catch (Exception e)
            {
            }
        }

        private void ResetAuthorizations()
        {
            try
            {
                pipeProxy.ResetAuthorizations();
            }
            catch (Exception e)
            {
            }
        }

        private void ChangedPort(string port)
        {
            try
            {
                pipeProxy.SetPort(port);
            }
            catch (Exception e)
            {
            }
        }
        
        private void ChangedPasscode(string authcode)
        {
            try
            {
                pipeProxy.SetPasscode(authcode);
            }
            catch (Exception e)
            {
            }
        }

        public void Dispose()
        {
        }
    }
}
