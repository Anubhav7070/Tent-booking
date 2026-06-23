<%@ Page Title="Edit User" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="EditUser.aspx.vb" Inherits="EditUser" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-person-gear me-2"></i>Edit User Account</h1></div>
<div class="card border-0 shadow-sm" style="max-width:720px">
    <div class="card-body p-4">
        <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mb-3" Visible="false"/>
        <div class="row g-3">
            <div class="col-md-6"><label class="form-label fw-semibold">Employee ID</label><asp:TextBox ID="txtEmpId" runat="server" CssClass="form-control" ReadOnly="true"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Full Name *</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Department</label><asp:TextBox ID="txtDept" runat="server" CssClass="form-control"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Designation</label><asp:TextBox ID="txtDesig" runat="server" CssClass="form-control"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Email</label><asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Phone Number</label><asp:TextBox ID="txtPhone" runat="server" CssClass="form-control"/></div>
            <div class="col-12"><label class="form-label fw-semibold">Quarter Address</label><asp:TextBox ID="txtAddress" runat="server" CssClass="form-control"/></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Role</label>
                <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                    <asp:ListItem Value="User">Employee (User)</asp:ListItem>
                    <asp:ListItem Value="HOD">Head of Department (HOD)</asp:ListItem>
                    <asp:ListItem Value="GM">General Manager (GM)</asp:ListItem>
                    <asp:ListItem Value="SuperAdmin">Super Admin</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="d-flex gap-3 mt-4">
            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary px-4" OnClick="btnSave_Click"/>
            <a href="Users.aspx" class="btn btn-outline-secondary px-4">Cancel</a>
        </div>
    </div>
</div>
<asp:HiddenField ID="hfUserId" runat="server"/>
</asp:Content>
