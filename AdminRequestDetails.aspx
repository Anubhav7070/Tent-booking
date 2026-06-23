<%@ Page Title="Request Details" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="AdminRequestDetails.aspx.vb" Inherits="AdminRequestDetails" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <div><h1 class="iocl-page-title"><i class="bi bi-file-earmark-check me-2"></i>Request Details</h1></div>
    <a href="AdminRequests.aspx" class="btn btn-outline-secondary btn-sm"><i class="bi bi-arrow-left me-1"></i>Back</a>
</div>

<asp:Panel ID="pnlNotFound" runat="server" CssClass="alert alert-danger" Visible="false">Request not found.</asp:Panel>

<asp:Panel ID="pnlMain" runat="server" Visible="false">
    <!-- Request header card -->
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-header d-flex justify-content-between align-items-center py-3 bg-white border-bottom">
            <div>
                <span class="fw-bold fs-5"><asp:Label ID="lblRequestNumber" runat="server"/></span>
                <asp:Label ID="lblStageBadge" runat="server" CssClass="ms-2"/>
                <asp:Label ID="lblStatusBadge" runat="server" CssClass="ms-1"/>
            </div>
            <small class="text-muted">Submitted: <asp:Label ID="lblCreatedAt" runat="server"/></small>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-3"><div class="text-muted small fw-semibold">Employee</div><div><asp:Label ID="lblEmployee" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Employee ID</div><div><asp:Label ID="lblEmpId" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Department</div><div><asp:Label ID="lblDept" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Submitted As</div><div><asp:Label ID="lblRole" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Event Date</div><div><asp:Label ID="lblEventDate" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Start Date</div><div><asp:Label ID="lblStartDate" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">End Date</div><div><asp:Label ID="lblEndDate" runat="server"/></div></div>
                <div class="col-md-3"><div class="text-muted small fw-semibold">Grand Total</div><div class="fw-bold fs-5 text-success">₹<asp:Label ID="lblGrandTotal" runat="server"/></div></div>
            </div>
            <asp:Panel ID="pnlDocument" runat="server" CssClass="mt-3 pt-3 border-top" Visible="false">
                <div class="text-muted small fw-semibold mb-1"><i class="bi bi-file-earmark-pdf me-1 text-primary"></i>In-Principal Approval Document</div>
                <asp:HyperLink ID="hlDocument" runat="server" Target="_blank" CssClass="btn btn-sm btn-outline-primary">
                    <i class="bi bi-file-earmark-pdf-fill me-1"></i>View Uploaded Document
                </asp:HyperLink>
            </asp:Panel>
        </div>
    </div>

    <!-- Items table -->
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-header py-3 bg-white border-bottom fw-semibold"><i class="bi bi-list-check me-2"></i>Requested Items</div>
        <div class="card-body p-0">
            <table class="table mb-0 iocl-table">
                <thead class="table-light"><tr><th>#</th><th>Item</th><th>Unit</th><th>Qty</th><th>Unit Price</th><th>Line Total</th></tr></thead>
                <tbody>
                    <asp:Repeater ID="rptItems" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td><%# Container.ItemIndex + 1 %></td>
                                <td class="fw-semibold"><%# Eval("ItemName") %></td>
                                <td><%# Eval("UnitType") %></td>
                                <td><%# Eval("RequestedQuantity") %></td>
                                <td>₹<%# String.Format("{0:N2}", Eval("UnitPriceAtRequest")) %></td>
                                <td>₹<%# String.Format("{0:N2}", Eval("LineTotal")) %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>

    <!-- Approval Timeline -->
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-header py-3 bg-white border-bottom fw-semibold"><i class="bi bi-diagram-3 me-2"></i>Approval Timeline</div>
        <div class="card-body"><asp:Literal ID="litTimeline" runat="server"/></div>
    </div>

    <!-- Rejection reason -->
    <asp:Panel ID="pnlRejection" runat="server" CssClass="alert alert-danger mb-4" Visible="false">
        <i class="bi bi-x-circle me-2"></i><strong>Rejection Reason:</strong> <asp:Label ID="lblRejectionReason" runat="server"/>
    </asp:Panel>

    <!-- Self-approval warning -->
    <asp:Panel ID="pnlSelfWarning" runat="server" CssClass="alert alert-warning mb-4" Visible="false">
        <i class="bi bi-exclamation-triangle me-2"></i>You cannot approve your own request.
    </asp:Panel>

    <!-- Action Buttons -->
    <asp:Panel ID="pnlActions" runat="server" Visible="false" CssClass="card border-0 shadow-sm mb-4">
        <div class="card-body d-flex gap-3 align-items-end flex-wrap">
            <asp:Button ID="btnApprove" runat="server" Text="✓ Approve Request" CssClass="btn btn-success px-4" OnClick="btnApprove_Click" OnClientClick="return confirm('Approve this request?');" />
            <div class="flex-fill">
                <label class="form-label small fw-semibold">Rejection Reason (required to reject)</label>
                <asp:TextBox ID="txtRejectReason" runat="server" CssClass="form-control" placeholder="Enter reason for rejection..." />
            </div>
            <asp:Button ID="btnReject" runat="server" Text="✗ Reject" CssClass="btn btn-danger px-4" OnClick="btnReject_Click" OnClientClick="return confirm('Reject this request?');" />
        </div>
        <asp:Label ID="lblActionError" runat="server" CssClass="text-danger px-3 pb-2 d-block" Visible="false"/>
    </asp:Panel>
</asp:Panel>
<asp:HiddenField ID="hfRequestId" runat="server"/>
</asp:Content>
