<%@ Page Title="" Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="askai.aspx.cs" Inherits="ELibraryManagement.askai" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container py-5">
        <div class="card ai-card">
            <div class="text-center mb-3">
                <img src="imgs/ai-librarian.svg" width="96" alt="AI Librarian" />
                <h3 class="mt-2">Ask the AI Librarian</h3>
                <p class="text-muted mb-0">Describe what you feel like reading &mdash; I'll search our catalog for you.</p>
            </div>
            <asp:TextBox ID="txtQuestion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                placeholder="e.g. Suggest a short fantasy book I can finish this week"></asp:TextBox>
            <asp:Button ID="btnAsk" runat="server" Text="Ask" CssClass="btn btn-primary btn-block mt-3" OnClick="btnAsk_Click" />
            <asp:Panel ID="pnlAnswer" runat="server" Visible="false" CssClass="ai-answer">
                <asp:Label ID="lblAnswer" runat="server"></asp:Label>
            </asp:Panel>
        </div>
    </div>
</asp:Content>
