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
    public class Pipeline {
        private Dictionary<int, IProcessingUnit> dataUnits = new Dictionary<int, IProcessingUnit>();
        private Dictionary<int, ILogProcessingUnit> logUnits = new Dictionary<int, ILogProcessingUnit>();
        private bool isAssembled = false;
        private bool isStarted = false;
        private bool isStopped = false;

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
            Dictionary<int, int> dataConnections = new Dictionary<int,int>();
            Dictionary<int, int> logConnections = new Dictionary<int,int>();

            int lastKey = -1;
            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (dataUnits.ContainsKey(lastKey)) {
                    dataConnections[lastKey] = i;
                    dataUnits[i].IsInputStreamConnected = true;
                    dataUnits[lastKey].IsDataStreamConnected = true;
                }

                lastKey = i;
            }

            foreach (int i in logUnits.Keys.OrderBy(k => k)) {
                int nr = dataUnits.Keys.Where(k => k < i).DefaultIfEmpty(-1).Max();
                if (dataUnits.ContainsKey(nr)) {
                    logConnections[i] = nr;
                    dataUnits[nr].IsLogStreamConnected = true;
                }
            }

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                dataUnits[i].Setup();
                if(dataConnections.ContainsKey(i))
                    dataUnits[dataConnections[i]].InputStream = dataUnits[i].DataOutputStream;
            }
            foreach (int i in logUnits.Keys.OrderBy(k => k)) {
                logUnits[i].Setup();
                if (logConnections.ContainsKey(i))
                    logUnits[i].InputStream = dataUnits[logConnections[i]].LogOutputStream;
            }

            isAssembled = true;
            return true;
        }

        public bool Start() {
            if (!isAssembled)
                Assemble();

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                dataUnits[i].Start();
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Start();

            isStarted = true;
            return true;
        }

        public bool Stop() {
            if (!isStarted)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k))
                dataUnits[i].Stop();
            foreach (int i in logUnits.Keys.OrderBy(k => k))
                logUnits[i].Stop();

            isStopped = true;
            return true;
        }

        public bool RunBlocking() {
            if (!isStarted)
                Start();

            foreach (int i in dataUnits.Keys.OrderBy(k => k)) {
                if (dataUnits[i] is IBlockingProcessingUnit) {
                    ((IBlockingProcessingUnit)dataUnits[i]).RunBlocking();
                    return true;
                }
            }

            return false;
        }
    }
}
