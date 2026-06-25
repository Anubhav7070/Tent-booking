<%@ Page Title="My Requests" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="MyRequests.aspx.vb" Inherits="MyRequests" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header d-flex justify-content-between align-items-center mb-4">
    <h1 class="iocl-page-title"><i class="bi bi-card-list me-2"></i>My Requests</h1>
    <a href="CreateRequest.aspx" class="btn btn-primary"><i class="bi bi-plus-circle me-1"></i>New Request</a>
</div>
<div class="card border-0 shadow-sm">
    <div class="card-body p-0">
        <table class="table table-hover mb-0 iocl-table">
            <thead class="table-dark"><tr><th>Request #</th><th>Event Date</th><th>Dates</th><th>Grand Total</th><th>Stage</th><th>Status</th><th>Submitted</th><th>Action</th></tr></thead>
            <tbody>
                <asp:Repeater ID="rptRequests" runat="server">
                    <ItemTemplate>
                        <tr>
                            <td class="fw-semibold text-primary"><%# Eval("RequestNumber") %></td>
                            <td><%# DirectCast(Eval("EventDate"),DateTime).ToString("dd MMM yyyy") %></td>
                            <td><small class="text-muted"><%# DirectCast(Eval("StartDate"),DateTime).ToString("dd MMM") %> – <%# DirectCast(Eval("EndDate"),DateTime).ToString("dd MMM yyyy") %></small></td>
                            <td class="fw-semibold">₹<%# String.Format("{0:N2}", Eval("GrandTotal")) %></td>
                            <td><%# GetStageBadge(Container.DataItem) %></td>
                            <td><%# GetStatusBadge(Container.DataItem) %></td>
                            <td><small class="text-muted"><%# DirectCast(Eval("CreatedAt"),DateTime).ToString("dd MMM yy") %></small></td>
                            <td>
                                <a href='AdminRequestDetails.aspx?id=<%# Eval("Id") %>' class="btn btn-sm btn-outline-primary me-1">Details</a>
                                <asp:HyperLink ID="hlDoc" runat="server" NavigateUrl='<%# Eval("InPrincipalDocumentPath") %>' Target="_blank" CssClass="btn btn-sm btn-outline-info me-1" Visible='<%# Not String.IsNullOrEmpty(Eval("InPrincipalDocumentPath").ToString()) %>' ToolTip="View Document">
                                    <i class="bi bi-file-earmark-pdf"></i>
                                </asp:HyperLink>
                                <asp:LinkButton ID="lnkCancel" runat="server" 
                                    CommandName="CancelRequest" 
                                    CommandArgument='<%# Eval("Id") %>' 
                                    CssClass="btn btn-sm btn-outline-danger" 
                                    OnClientClick="return confirm('Are you sure you want to cancel this request? This will release any allocated stock.');"
                                    Visible='<%# CInt(Eval("Status")) = 0 OrElse CInt(Eval("Status")) = 1 %>'>
                                    <i class="bi bi-x-circle me-1"></i>Cancel
                                </asp:LinkButton>
                            </td>
                        </tr>
                    </ItemTemplate>
                </asp:Repeater>
            </tbody>
        </table>
        <asp:Label ID="lblNoData" runat="server" CssClass="text-center text-muted py-5 d-block" Visible="false"><i class="bi bi-inbox fs-1 d-block mb-2"></i>No requests found. <a href="CreateRequest.aspx">Submit your first request!</a></asp:Label>
    </div>
</div>
</asp:Content>
