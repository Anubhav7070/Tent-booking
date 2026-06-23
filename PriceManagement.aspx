<%@ Page Title="Price Management" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="PriceManagement.aspx.vb" Inherits="PriceManagement" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-currency-rupee me-2"></i>Price Management</h1></div>
<div class="row g-4">
    <div class="col-lg-5">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header py-3 bg-white border-bottom fw-semibold">Update Item Price</div>
            <div class="card-body p-4">
                <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mb-3" Visible="false"/>
                <div class="mb-3"><label class="form-label fw-semibold">Select Item *</label>
                    <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlItem_Changed"></asp:DropDownList>
                </div>
                <div class="mb-3"><label class="form-label fw-semibold">Current Price</label>
                    <div class="input-group"><span class="input-group-text">₹</span><asp:TextBox ID="txtCurrentPrice" runat="server" CssClass="form-control" ReadOnly="true"/></div>
                </div>
                <div class="mb-3"><label class="form-label fw-semibold">New Price *</label>
                    <div class="input-group"><span class="input-group-text">₹</span><asp:TextBox ID="txtNewPrice" runat="server" CssClass="form-control" placeholder="0.00" TextMode="Number"/></div>
                </div>
                <div class="mb-3"><label class="form-label fw-semibold">Effective Date *</label><asp:TextBox ID="txtEffDate" runat="server" CssClass="form-control" TextMode="Date"/></div>
                <div class="mb-4"><label class="form-label fw-semibold">Reason</label><asp:TextBox ID="txtReason" runat="server" CssClass="form-control" placeholder="Reason for price update..."/></div>
                <asp:Button ID="btnUpdate" runat="server" Text="Update Price" CssClass="btn btn-primary w-100" OnClick="btnUpdate_Click"/>
            </div>
        </div>
    </div>
    <div class="col-lg-7">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-header py-3 bg-white border-bottom fw-semibold d-flex justify-content-between">
                <span>Recent Price Changes</span>
                <a href="PriceHistory.aspx" class="btn btn-sm btn-outline-secondary">Full History</a>
            </div>
            <div class="card-body p-0">
                <table class="table table-hover mb-0 iocl-table">
                    <thead class="table-light"><tr><th>Item</th><th>Old Price</th><th>New Price</th><th>Change</th><th>By</th><th>Date</th></tr></thead>
                    <tbody>
                        <asp:Repeater ID="rptHistory" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("ItemName") %></td>
                                    <td>₹<%# String.Format("{0:N2}", Eval("PreviousPrice")) %></td>
                                    <td class="fw-bold text-success">₹<%# String.Format("{0:N2}", Eval("UpdatedPrice")) %></td>
                                    <td><%# GetDiffBadge(CDec(Eval("PriceDifference"))) %></td>
                                    <td><small class="text-muted"><%# Eval("UpdatedBy") %></small></td>
                                    <td><small class="text-muted"><%# DirectCast(Eval("UpdatedAt"),DateTime).ToString("dd MMM yy") %></small></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
</asp:Content>
