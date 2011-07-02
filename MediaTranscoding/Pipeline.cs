#region Copyright
/* 
 *  Copyright (C) 2011 Oxan
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MPWebStream.MediaTranscoding {
    class Pipeline {
        private Dictionary<int, IProcessingUnit> dataUnits = new Dictionary<int, IProcessingUnit>();
        private Dictionary<int, ILogProcessingUnit> logUnits = new Dictionary<int, ILogProcessingUnit>();

        public void AddDataProcessingUnit(IProcessingUnit process, int position) {
            dataUnits[position] = process;
            dataUnits[position].IsInputStreamConnected = false;
            dataUnits[position].IsDataStreamConnected = false;
            dataUnits[position].IsLogStreamConnected = false;
        }

        public void AddLogProcessingUnit(ILogProcessingUnit process, int position) {
            logUnits[position] = process;
        }

        public bool Assemble() {
            int lastKey = -1;
            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (dataUnits.ContainsKey(lastKey)) {
                    dataUnits[i].InputStream = dataUnits[lastKey].DataOutputStream;
                    dataUnits[i].IsInputStreamConnected = true;
                    dataUnits[lastKey].IsDataStreamConnected = true;
                }

                lastKey = i;
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k)) {
                int nr = dataUnits.Keys.Where(k => k < i).DefaultIfEmpty(-1).Max();
                if (dataUnits.ContainsKey(nr)) {
                    logUnits[i].InputStream = dataUnits[nr].LogOutputStream;
                    dataUnits[nr].IsLogStreamConnected = true;
                }
            }

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                dataUnits[i].Setup();
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Setup();

            return true;
        }

        public bool Start() {
            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                dataUnits[i].Start();
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Start();
            return true;
        }

        public bool Stop() {
            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                dataUnits[i].Stop();
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Stop();
            return true;
        }
    }
}
