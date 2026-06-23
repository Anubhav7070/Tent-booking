<%@ Page Title="Edit Inventory Item" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="InventoryEdit.aspx.vb" Inherits="InventoryEdit" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-pencil me-2"></i>Edit Inventory Item</h1></div>
<div class="card border-0 shadow-sm" style="max-width:700px">
    <div class="card-body p-4">
        <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mb-3" Visible="false"/>
        <div class="mb-3"><label class="form-label fw-semibold">Item Name *</label><asp:TextBox ID="txtName" runat="server" CssClass="form-control"/></div>
        <div class="mb-3"><label class="form-label fw-semibold">Description</label><asp:TextBox ID="txtDesc" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"/></div>
        <div class="row g-3 mb-3">
            <div class="col-md-6"><label class="form-label fw-semibold">Category</label><asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select"></asp:DropDownList></div>
            <div class="col-md-6"><label class="form-label fw-semibold">Unit Type</label>
                <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select">
                    <asp:ListItem>Nos</asp:ListItem><asp:ListItem>Kg</asp:ListItem><asp:ListItem>Litre</asp:ListItem><asp:ListItem>Meter</asp:ListItem><asp:ListItem>Set</asp:ListItem><asp:ListItem>Box</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="row g-3 mb-3">
            <div class="col-md-4"><label class="form-label fw-semibold">Total Quantity</label><asp:TextBox ID="txtQty" runat="server" CssClass="form-control" TextMode="Number"/></div>
            <div class="col-md-4"><label class="form-label fw-semibold">Price per Unit (₹)</label><asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" TextMode="Number"/></div>
            <div class="col-md-4 d-flex align-items-end pb-1">
                <div class="form-check"><asp:CheckBox ID="chkActive" runat="server" CssClass="form-check-input"/><label class="form-check-label ms-2 fw-semibold">Active</label></div>
            </div>
        </div>
        <div class="d-flex gap-3">
            <asp:Button ID="btnSave" runat="server" Text="Save Changes" CssClass="btn btn-primary px-4" OnClick="btnSave_Click"/>
            <a href="Inventory.aspx" class="btn btn-outline-secondary px-4">Cancel</a>
        </div>
    </div>
</div>
<asp:HiddenField ID="hfId" runat="server"/>
</asp:Content>
