<%@ Page Title="Notifications" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="Notifications.aspx.vb" Inherits="Notifications" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-bell-fill me-2"></i>Notifications</h1>
    <asp:Button ID="btnMarkAll" runat="server" Text="Mark All as Read" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnMarkAll_Click"/>
</div>
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <asp:Repeater ID="rptNotifs" runat="server">
            <ItemTemplate>
                <div class='border-bottom p-4 d-flex gap-3 align-items-start <%# If(Not CBool(Eval("IsRead")), "bg-warning bg-opacity-10","") %>'>
                    <i class='bi bi-bell<%# If(Not CBool(Eval("IsRead")), "-fill text-warning","") %> mt-1 fs-5'></i>
                    <div class="flex-fill">
                        <div class="fw-semibold"><%# System.Web.HttpUtility.HtmlEncode(Eval("Title").ToString()) %></div>
                        <div class="text-muted"><%# System.Web.HttpUtility.HtmlEncode(Eval("Message").ToString()) %></div>
                        <div class="text-muted small mt-1"><%# DirectCast(Eval("CreatedAt"),DateTime).ToString("dd MMMM yyyy, HH:mm") %></div>
                    </div>
                    <% If Not CBool(Eval("IsRead")) Then %>
                    <span class="badge bg-warning text-dark">New</span>
                    <% End If %>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-5 d-block" Visible="false"><i class="bi bi-bell-slash fs-1 d-block mb-2"></i>No notifications.</asp:Label>
    </div>
</div>
</asp:Content>
