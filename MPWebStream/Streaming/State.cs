#region Copyright
/* 
 *  Copyright (C) 2008, 2009 StreamTv, http://code.google.com/p/mpstreamtv/
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


namespace MPWebStream.Streaming
{
    #region Enums
    /// <summary>
    /// Media objects state.
    /// </summary>
    public enum State
    {
        FileNotFound,
        NoFileLoaded,
        Loaded,
        Playing,
        Transcoding,
        Paused,
        EndOfFile,
        InvalidFile,
        FileLocked,
        Loading,
        TranscodingFailed,
        UnknownError,
    }
    #endregion
}