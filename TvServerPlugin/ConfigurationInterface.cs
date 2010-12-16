using System;
using System.Windows.Forms;

namespace MPWebStream.TvServerPlugin {
    public partial class ConfigurationInterface : SetupTv.SectionSettings {
        public ConfigurationInterface() {
            InitializeComponent();
        }

        public override void OnSectionActivated() {
            // load settings
            Configuration.Read();
            port.Value = Configuration.Port;
            useWebserver.Checked = Configuration.UseWebserver;
            manageTV4Home.Checked = Configuration.ManageTV4Home;

            base.OnSectionActivated();
        }

        public override void OnSectionDeActivated() {
            // save settings
            Configuration.Port = (int)port.Value;
            Configuration.UseWebserver = useWebserver.Checked;
            Configuration.ManageTV4Home = manageTV4Home.Checked;
            Configuration.Write();

            base.OnSectionDeActivated();
        }

        private void useWebserver_CheckedChanged(object sender, EventArgs e) {
            if (useWebserver.Checked) {
                port.Enabled = labelPort.Enabled = true;
            } else {
                port.Enabled = labelPort.Enabled = false;
            }
        }
    }
}
