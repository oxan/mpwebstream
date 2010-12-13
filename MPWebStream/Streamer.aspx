<%@ Language="C#" %>
<script runat="server" language="C#">
    public void Page_Load(object sender, EventArgs e) {
        MPWebStream.Site.Streamer.run(Response);
    }
</script>