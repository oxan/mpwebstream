using System;
using TvLibrary.Log;

namespace MPWebStream.TvServerPlugin {
    class LoggingConfiguration : Configuration {
        protected new void Read() {
            try {
                base.Read();
            } catch (Exception ex) {
                Log.Write(ex);
            }
        }

        public new void Write() {
            try {
                base.Write();
                Log.Info("MPWebStream: Wrote configuration to file");
            } catch (Exception ex) {
                Log.Write(ex);
            }
        }
    }
}
