﻿#region Copyright
/* 
 *  Copyright (C) 2010, 2011 Oxan
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion

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
                Log.Info("MPWebStream: Wrote configuration to file {0}", this.ConfigPath);
            } catch (Exception ex) {
                Log.Write(ex);
            }
        }
    }
}
