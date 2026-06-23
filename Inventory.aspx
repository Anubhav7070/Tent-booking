<%@ Page Title="Inventory" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="Inventory.aspx.vb" Inherits="Inventory" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-boxes me-2"></i>Inventory Items</h1>
    <a href="InventoryCreate.aspx" class="btn btn-primary"><i class="bi bi-plus-circle me-1"></i>Add Item</a>
</div>
<!-- Filters -->
<div class="card border-0 shadow-sm mb-4"><div class="card-body d-flex flex-wrap gap-3 align-items-end">
    <div><label class="form-label small fw-semibold">Search</label><asp:TextBox ID="txtSearch" runat="server" CssClass="form-control form-control-sm" placeholder="Item name..."/></div>
    <div><label class="form-label small fw-semibold">Category</label>
        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm"><asp:ListItem Value="">All Categories</asp:ListItem></asp:DropDownList>
    </div>
    <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click"/>
    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClear_Click"/>
</div></div>
<!-- Items Grid -->
<div class="row g-3">
    <asp:Repeater ID="rptItems" runat="server">
        <ItemTemplate>
            <div class="col-md-6 col-lg-4">
                <div class="card border-0 shadow-sm h-100">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div>
                                <h5 class="card-title mb-1 fw-bold"><%# Eval("Name") %></h5>
                                <span class="badge bg-light text-dark border small"><%# Eval("CategoryName") %></span>
                            </div>
                            <asp:LinkButton ID="lnkDelete" runat="server" CommandArgument='<%# Eval("Id") %>' OnCommand="lnkDelete_Command" CssClass="btn btn-sm btn-outline-danger" OnClientClick="return confirm('Deactivate this item?');"><i class="bi bi-trash"></i></asp:LinkButton>
                        </div>
                        <p class="text-muted small mb-2"><%# Eval("Description") %></p>
                        <div class="row g-2 text-center">
                            <div class="col-4"><div class="bg-light rounded p-2"><div class="fw-bold"><%# Eval("TotalQuantity") %></div><div class="text-muted" style="font-size:0.7rem;">Total</div></div></div>
                            <div class="col-4"><div class="bg-warning bg-opacity-10 rounded p-2"><div class="fw-bold text-warning"><%# Eval("ReservedQuantity") %></div><div class="text-muted" style="font-size:0.7rem;">Reserved</div></div></div>
                            <div class="col-4"><div class="bg-success bg-opacity-10 rounded p-2"><div class="fw-bold text-success"><%# Eval("AvailableQuantity") %></div><div class="text-muted" style="font-size:0.7rem;">Available</div></div></div>
                        </div>
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <span class="fw-bold text-success fs-6">₹<%# String.Format("{0:N2}", Eval("CurrentPrice")) %>/<%# Eval("UnitType") %></span>
                            <a href='InventoryEdit.aspx?id=<%# Eval("Id") %>' class="btn btn-sm btn-outline-primary"><i class="bi bi-pencil me-1"></i>Edit</a>
                        </div>
                    </div>
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>
</div>
<asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-5 d-block" Visible="false"><i class="bi bi-inbox fs-1 d-block mb-2"></i>No inventory items found.</asp:Label>
</asp:Content>
