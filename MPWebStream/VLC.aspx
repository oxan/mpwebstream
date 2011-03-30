<%
#region Copyright
/* 
 *  Copyright (C) 2009, 2010 Gemx
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
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VLC.aspx.cs" Inherits="MPWebStream.Site.VLC" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title>MPWebStream -- VLC Player</title>
  <script type="text/javascript">
      function getVlcPlayer(vlc_id) {
          if (document.all) vlc_id += '_ie';
          return document.getElementById(vlc_id);
      }

      function init() {
          document.getElementById('medianame').innerHTML = document.getElementById('name').value;
          var vlc = getVlcPlayer('vlcplayer');
          vlc.playlist.add(document.getElementById('url').value);
          vlc.playlist.play();
      }

      function enableDeinterlace(type) {
          var vlc = getVlcPlayer('vlcplayer');
          if (typeof (vlc.video.deinterlace) !== "undefined") {
              vlc.video.deinterlace.enable(type);
          } else {
              alert("Your VLC version is too old to support deinterlacing in the browser. Update to at least version 1.1.0");
          }
      }
</script>
</head>
<body onload="init();">
  <b>Media:</b> <span id="medianame"></span><br /><br />

  <input type="button" value="play" onclick="getVlcPlayer('vlcplayer').playlist.play();" /> 
  <input type="button" value="pause" onclick="getVlcPlayer('vlcplayer').playlist.togglePause();" />
  <input type="button" value="stop" onclick="getVlcPlayer('vlcplayer').playlist.stop();" />
  <input type="button" value="switch fullscreen" onclick="getVlcPlayer('vlcplayer').video.fullscreen=true;" /> 
  <input type="button" value="enable deinterlacing" onclick="enableDeinterlace('linear');" /> 
  <hr style="border: solid 1px black" />

  <object id="vlcplayer_ie" classid="clsid:9BE31822-FDAD-461B-AD51-BE1D1C159921" width="400" height="300" events="True">
    <param name="ShowDisplay" value="false" ></param>
    <param name="AutoLoop" value="no"></param>
    <param name="AutoPlay" value="no"></param>
    <embed id="vlcplayer" type="application/x-vlc-plugin" pluginspage="http://www.videolan.org" version="VideoLAN.VLCPlugin.2" autoplay="no" loop="no" width="400" height="300" />
  </object>

  <input runat="server" type="hidden" id="url" />
  <input runat="server" type="hidden" id="name" />
</body>
</html>

