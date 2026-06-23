<%@ Page Title="Rental Requests" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="AdminRequests.aspx.vb" Inherits="AdminRequests" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-file-earmark-text me-2"></i>Rental Requests</h1>
</div>
<!-- Filters -->
<div class="card border-0 shadow-sm mb-4">
    <div class="card-body d-flex flex-wrap gap-3 align-items-end">
        <div>
            <label class="form-label small fw-semibold">Status Filter</label>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                <asp:ListItem Value="">All Statuses</asp:ListItem>
                <asp:ListItem Value="0">Pending</asp:ListItem>
                <asp:ListItem Value="1">Approved</asp:ListItem>
                <asp:ListItem Value="2">Rejected</asp:ListItem>
                <asp:ListItem Value="3">Cancelled</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div>
            <label class="form-label small fw-semibold">Search</label>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control form-control-sm" placeholder="Request # or employee name" />
        </div>
        <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn btn-primary btn-sm" OnClick="btnFilter_Click" />
        <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClear_Click" />
    </div>
</div>
<!-- Table -->
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <div class="table-responsive">
            <table class="table table-hover mb-0 iocl-table">
                <thead class="table-dark">
                    <tr><th>Request #</th><th>Employee</th><th>Dept</th><th>Event Date</th><th>Grand Total</th><th>Submitted By</th><th>Stage</th><th>Status</th><th>Date</th><th>Action</th></tr>
                </thead>
                <tbody>
                    <asp:Repeater ID="rptRequests" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td class="fw-semibold text-primary"><%# Eval("RequestNumber") %></td>
                                <td><%# Eval("UserFullName") %><br/><small class="text-muted"><%# Eval("UserEmployeeId") %></small></td>
                                <td><small class="text-muted"><%# Eval("UserDepartment") %></small></td>
                                <td><%# DirectCast(Eval("EventDate"),DateTime).ToString("dd MMM yyyy") %></td>
                                <td class="fw-semibold">₹<%# String.Format("{0:N2}", Eval("GrandTotal")) %></td>
                                <td><span class="badge bg-light text-dark border"><%# Eval("SubmittedByRole") %></span></td>
                                <td><%# GetStageBadge(CInt(Eval("ApprovalStage"))) %></td>
                                <td><%# GetStatusBadge(CInt(Eval("Status"))) %></td>
                                <td><small class="text-muted"><%# DirectCast(Eval("CreatedAt"),DateTime).ToString("dd MMM yy") %></small></td>
                                <td><a href='AdminRequestDetails.aspx?id=<%# Eval("Id") %>' class="btn btn-sm btn-outline-primary">Details</a></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-4 d-block" Visible="false">No requests found.</asp:Label>
    </div>
</div>
</asp:Content>
