<%@ Language="C#" %>
<script runat="server" language="C#">
    public void Page_Load(object sender, EventArgs e) {
        MPWebStream.Streamer.run(Response);
    }
</script>