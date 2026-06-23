<%@ Page Title="Reports" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="Reports.aspx.vb" Inherits="Reports" %>
<asp:Content ID="c2" ContentPlaceHolderID="MainContent" runat="server">
<div class="iocl-page-header mb-4"><h1 class="iocl-page-title"><i class="bi bi-graph-up-arrow me-2"></i>Reports</h1></div>
<!-- Report Filters -->
<div class="card border-0 shadow-sm mb-4">
    <div class="card-body d-flex flex-wrap gap-3 align-items-end">
        <div><label class="form-label small fw-semibold">Report Type</label>
            <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlType_Changed">
                <asp:ListItem Value="Monthly">Monthly</asp:ListItem>
                <asp:ListItem Value="Annual">Annual</asp:ListItem>
                <asp:ListItem Value="Custom">Custom Date Range</asp:ListItem>
            </asp:DropDownList>
        </div>
        <asp:Panel ID="pnlMonthly" runat="server">
            <div class="d-flex gap-2">
                <div><label class="form-label small fw-semibold">Year</label>
                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select form-select-sm"></asp:DropDownList>
                </div>
                <div><label class="form-label small fw-semibold">Month</label>
                    <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-select form-select-sm">
                        <asp:ListItem Value="1">Jan</asp:ListItem><asp:ListItem Value="2">Feb</asp:ListItem><asp:ListItem Value="3">Mar</asp:ListItem>
                        <asp:ListItem Value="4">Apr</asp:ListItem><asp:ListItem Value="5">May</asp:ListItem><asp:ListItem Value="6">Jun</asp:ListItem>
                        <asp:ListItem Value="7">Jul</asp:ListItem><asp:ListItem Value="8">Aug</asp:ListItem><asp:ListItem Value="9">Sep</asp:ListItem>
                        <asp:ListItem Value="10">Oct</asp:ListItem><asp:ListItem Value="11">Nov</asp:ListItem><asp:ListItem Value="12">Dec</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlCustom" runat="server" Visible="false">
            <div class="d-flex gap-2">
                <div><label class="form-label small fw-semibold">From</label><asp:TextBox ID="txtFrom" runat="server" CssClass="form-control form-control-sm" TextMode="Date"/></div>
                <div><label class="form-label small fw-semibold">To</label><asp:TextBox ID="txtTo" runat="server" CssClass="form-control form-control-sm" TextMode="Date"/></div>
            </div>
        </asp:Panel>
        <asp:Button ID="btnGenerate" runat="server" Text="Generate Report" CssClass="btn btn-primary btn-sm" OnClick="btnGenerate_Click"/>
    </div>
</div>
<!-- Summary Cards -->
<asp:Panel ID="pnlResults" runat="server" Visible="false">
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-header py-3 bg-white fw-semibold border-bottom"><asp:Label ID="lblReportTitle" runat="server"/></div>
        <div class="card-body">
            <div class="row g-3 mb-4">
                <div class="col-md-3 text-center"><div class="fw-bold fs-3 text-primary"><asp:Label ID="lblTotal" runat="server"/></div><div class="text-muted small">Total Requests</div></div>
                <div class="col-md-3 text-center"><div class="fw-bold fs-3 text-success">₹<asp:Label ID="lblRevenue" runat="server"/></div><div class="text-muted small">Approved Revenue</div></div>
                <div class="col-md-3 text-center"><div class="fw-bold fs-3 text-success"><asp:Label ID="lblApproved" runat="server"/></div><div class="text-muted small">Approved</div></div>
                <div class="col-md-3 text-center"><div class="fw-bold fs-3 text-danger"><asp:Label ID="lblRejected" runat="server"/></div><div class="text-muted small">Rejected/Pending</div></div>
            </div>
        </div>
    </div>
    <div class="card border-0 shadow-sm">
        <div class="card-body p-0">
            <table class="table table-hover mb-0 iocl-table">
                <thead class="table-dark"><tr><th>Request #</th><th>Employee</th><th>Dept</th><th>Event Date</th><th>Grand Total</th><th>Status</th></tr></thead>
                <tbody>
                    <asp:Repeater ID="rptReport" runat="server">
                        <ItemTemplate>
                            <tr>
                                <td class="fw-semibold"><%# Eval("RequestNumber") %></td>
                                <td><%# Eval("UserFullName") %><br/><small class="text-muted"><%# Eval("UserEmployeeId") %></small></td>
                                <td><small class="text-muted"><%# Eval("UserDepartment") %></small></td>
                                <td><%# DirectCast(Eval("EventDate"),DateTime).ToString("dd MMM yyyy") %></td>
                                <td>₹<%# String.Format("{0:N2}", Eval("GrandTotal")) %></td>
                                <td><%# GetStatusBadge(CInt(Eval("Status"))) %></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </div>
    </div>
</asp:Panel>
</asp:Content>
