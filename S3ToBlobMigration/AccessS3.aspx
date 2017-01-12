<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccessS3.aspx.cs" Inherits="S3ToBlobMigration.AccessS3" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div>
        <br />
        Select file to upload: <asp:FileUpload ID="FileUpload1" runat="server" />
        <button id="upload" runat="server">upload</button>
        <br />
        <br />
        
        List of items in bucket:<br />
        <asp:ListBox ID="ListBox1" runat="server" OnSelectedIndexChanged="ListBox1_SelectedIndexChanged">

        </asp:ListBox>
        <br />
        <button id="btnGetS3Objects" runat="server">Get List of Blobs</button>
        <br />
        <br />
        <asp:Label ID="Label1" runat="server" Text="File Name to download: "></asp:Label>
        <br />
        <asp:TextBox ID="txtFileName" runat="server"></asp:TextBox>
        <asp:Button ID="Button1" runat="server" Text="Download" OnClick="Button1_Click" />
    </div>
        
</asp:Content>