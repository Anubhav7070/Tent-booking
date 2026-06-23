<%@ Page Title="Audit Log" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="AuditLog.aspx.vb" Inherits="AuditLog" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-journal-text me-2"></i>Audit Log</h1></div>
<div class="card border-0 shadow-sm mb-3 p-3 d-flex flex-row gap-3 align-items-end">
    <div><label class="form-label small fw-semibold">Page</label><asp:TextBox ID="txtPage" runat="server" CssClass="form-control form-control-sm" Text="1" style="width:80px;"/></div>
    <asp:Button ID="btnGo" runat="server" Text="Go" CssClass="btn btn-primary btn-sm" OnClick="btnGo_Click"/>
    <asp:Button ID="btnPrev" runat="server" Text="‹ Prev" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnPrev_Click"/>
    <asp:Button ID="btnNext" runat="server" Text="Next ›" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnNext_Click"/>
    <small class="text-muted ms-2">Page <asp:Label ID="lblCurrentPage" runat="server"/> of <asp:Label ID="lblTotalPages" runat="server"/> (<asp:Label ID="lblTotal" runat="server"/> records)</small>
</div>
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <table class="table table-hover mb-0 iocl-table" style="font-size:0.82rem;">
            <thead class="table-dark"><tr><th>#</th><th>User</th><th>Action</th><th>Entity</th><th>Entity ID</th><th>Description</th><th>Old Value</th><th>New Value</th><th>IP</th><th>Timestamp</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptLogs" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("Id") %></td>
                            <td><%# Eval("UserName") %></td>
                            <td><span class="badge bg-light text-dark border"><%# Eval("Action") %></span></td>
                            <td class="text-muted"><%# Eval("EntityName") %></td>
                            <td class="text-muted"><%# Eval("EntityId") %></td>
                            <td><%# System.Web.HttpUtility.HtmlEncode(Eval("Description").ToString()) %></td>
                            <td><small class="text-muted"><%# Eval("OldValue") %></small></td>
                            <td><small class="text-success"><%# Eval("NewValue") %></small></td>
                            <td><small class="text-muted"><%# Eval("IpAddress") %></small></td>
                            <td><small><%# DirectCast(Eval("Timestamp"),DateTime).ToString("dd MMM yyyy HH:mm") %></small></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-4 d-block" Visible="false">No audit records found.</asp:Label>
    </div>
</div>
</asp:Content>
