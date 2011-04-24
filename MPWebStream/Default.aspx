<%
#region Copyright
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
%>
<%@ Page EnableViewState="false" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MPWebStream.Site.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>MPWebStream</title>
</head>
<body>
    <form runat="server">
        <p>
            <h2>Live TV</h2>
            <asp:Table ID="StreamTable" runat="server">
            </asp:Table>
        </p>

        <p>
            <h2>Recordings</h2>
            <asp:Table ID="RecordingTable" runat="server">
            </asp:Table>
        </p>

        <h2>Troubleshooting</h2>
        <p>
        If you have problems please report them to the thread on the MediaPortal forums. Include the following details:
        <ul>
            <li>The contents of the logfile: <strong><asp:Label ID="LogPath" runat="server"></asp:Label></strong></li>
            <li>The contents of the configuration file: <strong><asp:Label ID="Config" runat="server"></asp:Label></strong> (WARNING: this file contains your password. Remove it before posting).</li>
            <li>The version of MediaPortal</li>
            <li>The version of MPWebStream (when using a Git version also include the date of download)</li>
            <li>For transcoding-related problems: does it work without transcoding? Which transcoder and version do you use?</li>
        </ul>
        </p>
    </form>
</body>
</html>
