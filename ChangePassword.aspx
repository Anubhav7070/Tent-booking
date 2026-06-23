<%@ Page Title="Change Password" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="ChangePassword.aspx.vb" Inherits="ChangePassword" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-shield-lock me-2"></i>Change Password</h1></div>
<div class="card border-0 shadow-sm" style="max-width:480px">
    <div class="card-body p-4">
        <asp:Panel ID="pnlForced" runat="server" CssClass="alert alert-warning mb-3" Visible="false">
            <i class="bi bi-exclamation-triangle me-2"></i><strong>Password change required.</strong> Please set a new password before continuing.
        </asp:Panel>
        <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mb-3" Visible="false"/>
        <div class="mb-3"><label class="form-label fw-semibold">Current Password</label><asp:TextBox ID="txtCurrent" runat="server" CssClass="form-control" TextMode="Password"/></div>
        <div class="mb-3"><label class="form-label fw-semibold">New Password</label><asp:TextBox ID="txtNew" runat="server" CssClass="form-control" TextMode="Password" placeholder="Min 6 characters"/></div>
        <div class="mb-4"><label class="form-label fw-semibold">Confirm New Password</label><asp:TextBox ID="txtConfirm" runat="server" CssClass="form-control" TextMode="Password"/></div>
        <asp:Button ID="btnChange" runat="server" Text="Change Password" CssClass="btn btn-primary w-100" OnClick="btnChange_Click"/>
    </div>
</div>
</asp:Content>
