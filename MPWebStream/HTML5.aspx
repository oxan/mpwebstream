<%
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
%>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HTML5.aspx.cs" Inherits="MPWebStream.Site.HTML5" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>MPWebStream -- HTML5 Player</title>
</head>
<body onload="init();">
  <b>Media:</b> <asp:Label ID="MediaLabel" runat="server"></asp:Label><br /><br />

  <video controls="controls" autoplay="autoplay" id="VideoPlayer" runat="server"></video>
</body>
</html>