using System;
using System.Windows.Forms;

namespace MPWebStream.TvServerPlugin {
    public partial class ConfigurationInterface : SetupTv.SectionSettings {
        private LoggingConfiguration config;

        public ConfigurationInterface() {
            InitializeComponent();
            config = new LoggingConfiguration();
        }

        public override void OnSectionActivated() {
            // load settings
            port.Value = config.Port;
            useWebserver.Checked = config.UseWebserver;
            manageTV4Home.Checked = config.ManageTV4Home;
            requireAuthentication.Checked = config.EnableAuthentication;
            userName.Text = config.Username;
            password.Text = config.Password;

            base.OnSectionActivated();
        }

        public override void OnSectionDeActivated() {
            // save settings
            config.Port = (int)port.Value;
            config.UseWebserver = useWebserver.Checked;
            config.ManageTV4Home = manageTV4Home.Checked;
            config.Username = userName.Text;
            config.Password = password.Text;
            config.EnableAuthentication = requireAuthentication.Checked;
            config.Write();

            base.OnSectionDeActivated();
        }

        private void useWebserver_CheckedChanged(object sender, EventArgs e) {
            if (useWebserver.Checked) {
                port.Enabled = labelPort.Enabled = true;
            } else {
                port.Enabled = labelPort.Enabled = false;
            }
        }

        private void requireAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            if (requireAuthentication.Checked) {
                userName.Enabled = true;
                password.Enabled = true;
            } else {
                userName.Enabled = false;
                password.Enabled = false;
            }
        }
    }
}
