using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace iStat_Server
{
    public partial class Settings : Form
    {
        public class AuthCodeChangedArgs : EventArgs
        {
            public string AuthCode { get; set; }
        }

        public event EventHandler<AuthCodeChangedArgs> AuthCodeChanged;
        public event EventHandler AuthReset;
        public void OnAuthReset()
        {
            if (AuthReset != null)
                AuthReset(this, null);
        }
        
        public void OnAuthCodeChanged(string authCode)
        {
            if (AuthCodeChanged != null)
                AuthCodeChanged(this, new AuthCodeChangedArgs { AuthCode = authCode });
        }


        public class PortChangedArgs : EventArgs
        {
            public string Port { get; set; }
        }

        public event EventHandler<PortChangedArgs> PortChanged;
        public void OnPortChanged(string port)
        {
            if (PortChanged != null)
                PortChanged(this, new PortChangedArgs { Port = port });
        }

        //
        public class UPNPPortChangedArgs : EventArgs
        {
            public string Port { get; set; }
        }
        public event EventHandler<UPNPPortChangedArgs> UPNPPortChanged;
        public void OnUPNPPortChanged(string port)
        {
            if (UPNPPortChanged != null)
                UPNPPortChanged(this, new UPNPPortChangedArgs { Port = port });
        }
        //
        public class UPNPChangedArgs : EventArgs
        {
            public bool Enabled { get; set; }
        }

        public event EventHandler<UPNPChangedArgs> UPNPChanged;
        public void OnUPNPChanged(bool enabled)
        {
            if (UPNPChanged != null)
                UPNPChanged(this, new UPNPChangedArgs { Enabled = enabled });
        }

        public Settings()
        {
            InitializeComponent();

            this.FormClosed += Form1_FormClosed;
            pinText.KeyPress += PinTextKeyPress;
            portText.KeyPress += PinTextKeyPress;
            upnpText.KeyPress += PinTextKeyPress;
        }

        private void Form1_FormClosed(Object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        public void SetUPNPEnabled(bool enabled)
        {
            upnpCheckbox.Checked = enabled;
        }

        public void SetUPNPPortText(string port)
        {
            upnpText.Text = port;
        }

        public void SetPasscodeText(string passcode)
        {
            pinText.Text = passcode;
        }

        public void SetPortText(string port)
        {
            portText.Text = port;
        }

        private void PinTextKeyPress(object sender, KeyPressEventArgs e)
        {
            //only allow numeric value
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        
        private void PinTextChanged(object sender, EventArgs e)
        {
            var tb = (TextBox) sender;
            int val;
            //enable button if 5 numeric values are in the textbox
            if (tb.Text != null && tb.Text.Length == 5 && int.TryParse(tb.Text, out val))
            {
                saveButton.Enabled = true;
            }
            else
            {
                saveButton.Enabled = false;
            }
        }

        private void UPNPPortTextChanged(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            int val;
            //enable button if 5 numeric values are in the textbox
            if (tb.Text != null && tb.Text.Length >= 4 && int.TryParse(tb.Text, out val))
            {
                upnpSaveButton.Enabled = true;
            }
            else
            {
                upnpSaveButton.Enabled = false;
            }
        }

        private void PortTextChanged(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            int val;
            //enable button if 5 numeric values are in the textbox
            if (tb.Text != null && tb.Text.Length >= 4 && int.TryParse(tb.Text, out val))
            {
                portSaveButton.Enabled = true;
            }
            else
            {
                portSaveButton.Enabled = false;
            }
        }

        private void ToggleUPNP(object sender, EventArgs e)
        {
            OnUPNPChanged(upnpCheckbox.Checked);
        }

        private void SaveUPNPPort(object sender, EventArgs e)
        {
            OnUPNPPortChanged(upnpText.Text);
        }

        private void SavePort(object sender, EventArgs e)
        {
            OnPortChanged(portText.Text);
        }
        
        private void SavePin(object sender, EventArgs e)
        {
            OnAuthCodeChanged(pinText.Text);
        }

        private void CancelClick(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void ResetAuthButtonClick(object sender, EventArgs e)
        {
            OnAuthReset();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }
    }
}
