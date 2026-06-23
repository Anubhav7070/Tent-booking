<%@ Page Title="User Management" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="Users.aspx.vb" Inherits="Users" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-people me-2"></i>User Accounts</h1>
    <a href="CreateUser.aspx" class="btn btn-primary"><i class="bi bi-person-plus me-1"></i>Create User</a>
</div>
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <table class="table table-hover mb-0 iocl-table">
            <thead class="table-dark"><tr><th>Emp ID</th><th>Name</th><th>Department</th><th>Email</th><th>Role</th><th>Status</th><th>Last Login</th><th>Actions</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptUsers" runat="server">
                    <ItemTemplate>
                        <tr class='<%# If(Not CBool(Eval("IsActive")), "table-secondary opacity-75","") %>'>
                            <td class="fw-semibold font-monospace"><%# Eval("EmployeeId") %></td>
                            <td><%# Eval("FullName") %><br/><small class="text-muted"><%# Eval("UserName") %></small></td>
                            <td><small class="text-muted"><%# Eval("Department") %></small></td>
                            <td><small><%# Eval("Email") %></small></td>
                            <td><%# GetRoleBadge(Eval("Role").ToString()) %></td>
                            <td><%# If(CBool(Eval("IsActive")), "<span class=""badge bg-success"">Active</span>","<span class=""badge bg-secondary"">Inactive</span>") %></td>
                            <td><small class="text-muted"><%# If(Eval("LastLoginAt") Is DBNull.Value OrElse Eval("LastLoginAt") Is Nothing, "Never", DirectCast(Eval("LastLoginAt"), DateTime?).Value.ToString("dd MMM yy HH:mm")) %></small></td>
                            <td>
                                <div class="d-flex gap-1 flex-wrap">
                                    <a href='EditUser.aspx?id=<%# Eval("Id") %>' class="btn btn-xs btn-outline-primary" style="font-size:0.72rem;padding:2px 8px;">Edit</a>
                                    <asp:LinkButton ID="lnkToggle" runat="server" CommandArgument='<%# Eval("Id") %>' OnCommand="lnkToggle_Command"
                                        CssClass='<%# "btn btn-xs " & If(CBool(Eval("IsActive")),"btn-outline-warning","btn-outline-success") %>'
                                        style="font-size:0.72rem;padding:2px 8px;"
                                        OnClientClick="return confirm('Toggle active status?');">
                                        <%# If(CBool(Eval("IsActive")), "Deactivate","Activate") %>
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="lnkReset" runat="server" CommandArgument='<%# Eval("Id") %>' OnCommand="lnkReset_Command"
                                        CssClass="btn btn-xs btn-outline-danger" style="font-size:0.72rem;padding:2px 8px;"
                                        OnClientClick="return confirm('Reset password to Admin@123?');">Reset Pwd</asp:LinkButton>
                                </div>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
    </div>
</div>
</asp:Content>
