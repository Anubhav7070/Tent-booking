<%@ Page Title="Price History" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="PriceHistory.aspx.vb" Inherits="PriceHistory" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-clock-history me-2"></i>Price History</h1>
    <div class="d-flex gap-2 align-items-end">
        <div><label class="form-label small fw-semibold mb-1">Filter by Item</label>
            <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlItem_Changed">
                <asp:ListItem Value="">All Items</asp:ListItem>
            </asp:DropDownList>
        </div>
    </div>
</div>
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <table class="table table-hover mb-0 iocl-table">
            <thead class="table-dark"><tr><th>Item</th><th>Previous Price</th><th>Updated Price</th><th>Change</th><th>Effective Date</th><th>Reason</th><th>Updated By</th><th>Updated At</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptHistory" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td class="fw-semibold"><%# Eval("ItemName") %></td>
                            <td>₹<%# String.Format("{0:N2}", Eval("PreviousPrice")) %></td>
                            <td class="fw-bold">₹<%# String.Format("{0:N2}", Eval("UpdatedPrice")) %></td>
                            <td><%# GetDiffBadge(CDec(Eval("PriceDifference"))) %></td>
                            <td><%# DirectCast(Eval("EffectiveDate"),DateTime).ToString("dd MMM yyyy") %></td>
                            <td><small class="text-muted"><%# Eval("Reason") %></small></td>
                            <td><%# Eval("UpdatedBy") %></td>
                            <td><small class="text-muted"><%# DirectCast(Eval("UpdatedAt"),DateTime).ToString("dd MMM yyyy HH:mm") %></small></td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-4 d-block" Visible="false">No price history found.</asp:Label>
    </div>
</div>
</asp:Content>
