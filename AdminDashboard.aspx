<%@ Page Title="Admin Dashboard" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="AdminDashboard.aspx.vb" Inherits="AdminDashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Hero Banner -->
    <div class="iocl-hero-banner d-flex flex-column flex-md-row justify-content-between align-items-start align-items-md-center gap-3">
        <div>
            <h2>COMMUNITY HALL &amp; INVENTORY TERMINAL</h2>
            <p>Monitoring bookings, inventory reservations, and compliance logs for Panipat Refinery Township.</p>
        </div>
        <div class="d-flex flex-wrap gap-2">
            <a href="AdminRequests.aspx" class="iocl-hero-btn iocl-hero-btn-blue">
                <i class="bi bi-file-earmark-text-fill"></i>Rental Requests
            </a>
            <a href="Inventory.aspx" class="iocl-hero-btn iocl-hero-btn-purple">
                <i class="bi bi-boxes"></i>Inventory Items
            </a>
            <a href="Reports.aspx" class="iocl-hero-btn iocl-hero-btn-dark">
                <i class="bi bi-bar-chart-line-fill"></i>Reports
            </a>
        </div>
    </div>

    <!-- KPI Row 1: Requests & Bookings -->
    <div class="row g-3 mb-4">
        <!-- Total Requests -->
        <div class="col-6 col-md-3">
            <div class="card iocl-card iocl-kpi-card p-3">
                <div class="text-muted small text-uppercase fw-bold">Total Requests</div>
                <h3 class="fw-bold mb-0 text-dark"><asp:Label ID="lblTotalRequests" runat="server">0</asp:Label></h3>
                <i class="bi bi-cart-fill iocl-kpi-icon"></i>
            </div>
        </div>
        <!-- Pending Requests -->
        <div class="col-6 col-md-3">
            <div class="card iocl-card iocl-kpi-card kpi-pending p-3">
                <div class="text-muted small text-uppercase fw-bold">Pending Requests</div>
                <h3 class="fw-bold mb-0 text-warning"><asp:Label ID="lblPendingRequests" runat="server">0</asp:Label></h3>
                <i class="bi bi-hourglass-split iocl-kpi-icon"></i>
            </div>
        </div>
        <!-- Approved Requests -->
        <div class="col-6 col-md-3">
            <div class="card iocl-card iocl-kpi-card kpi-approved p-3">
                <div class="text-muted small text-uppercase fw-bold">Approved Requests</div>
                <h3 class="fw-bold mb-0 text-success"><asp:Label ID="lblApprovedRequests" runat="server">0</asp:Label></h3>
                <i class="bi bi-check-circle-fill iocl-kpi-icon"></i>
            </div>
        </div>
        <!-- Rejected Requests -->
        <div class="col-6 col-md-3">
            <div class="card iocl-card iocl-kpi-card kpi-rejected p-3">
                <div class="text-muted small text-uppercase fw-bold">Rejected Requests</div>
                <h3 class="fw-bold mb-0 text-danger"><asp:Label ID="lblRejectedRequests" runat="server">0</asp:Label></h3>
                <i class="bi bi-x-circle-fill iocl-kpi-icon"></i>
            </div>
        </div>
    </div>

    <!-- KPI Row 2: Finance & Inventory -->
    <div class="row g-3 mb-4">
        <!-- Total Revenue -->
        <div class="col-6 col-md-4">
            <div class="card iocl-card iocl-kpi-card kpi-revenue p-3">
                <div class="text-muted small text-uppercase fw-bold">Total Revenue</div>
                <h3 class="fw-bold mb-0 text-primary">₹<asp:Label ID="lblTotalRevenue" runat="server">0</asp:Label></h3>
                <i class="bi bi-currency-rupee iocl-kpi-icon"></i>
            </div>
        </div>
        <!-- Monthly Revenue -->
        <div class="col-6 col-md-4">
            <div class="card iocl-card iocl-kpi-card kpi-revenue p-3">
                <div class="text-muted small text-uppercase fw-bold">Monthly Revenue</div>
                <h3 class="fw-bold mb-0 text-primary">₹<asp:Label ID="lblMonthlyRevenue" runat="server">0</asp:Label></h3>
                <i class="bi bi-cash-stack iocl-kpi-icon"></i>
            </div>
        </div>
        <!-- Low Stock Count -->
        <div class="col-6 col-md-4">
            <div class="card iocl-card iocl-kpi-card kpi-rejected p-3">
                <div class="text-muted small text-uppercase fw-bold">Low Stock Items</div>
                <h3 class="fw-bold mb-0 text-danger"><asp:Label ID="lblLowStock" runat="server">0</asp:Label></h3>
                <i class="bi bi-exclamation-triangle-fill iocl-kpi-icon"></i>
            </div>
        </div>
    </div>

    <!-- Reports Quick Access Card -->
    <div class="row g-3 mb-4">
        <div class="col-12">
            <div class="card shadow-sm border-0" style="background: linear-gradient(135deg, #1a3a6b 0%, #0d2545 100%); border-radius: 12px;">
                <div class="card-body p-4 d-flex flex-column flex-md-row align-items-center justify-content-between gap-3">
                    <div class="d-flex align-items-center gap-3">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" 
                             style="width:56px;height:56px;background:rgba(255,255,255,0.12);">
                            <i class="bi bi-file-earmark-bar-graph-fill fs-3 text-white"></i>
                        </div>
                        <div class="text-white">
                            <div class="fw-bold fs-5">Reports &amp; Analytics</div>
                            <div class="opacity-75 small">Generate monthly rental reports, export Excel files, and view activity analytics</div>
                        </div>
                    </div>
                    <div class="d-flex flex-wrap gap-2">
                        <a href="Reports.aspx" class="btn btn-light fw-semibold px-4">
                            <i class="bi bi-bar-chart-line-fill me-2 text-primary"></i>View Analytics
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Charts Row -->
    <div class="row g-4 mb-4">
        <!-- Revenue Trend -->
        <div class="col-md-6">
            <div class="card iocl-card shadow-sm h-100">
                <div class="card-header iocl-card-header"><i class="bi bi-graph-up-arrow me-2 text-primary"></i>Revenue Trend (Last 6 Months)</div>
                <div class="card-body">
                    <canvas id="revenueChart" style="max-height: 250px;"></canvas>
                </div>
            </div>
        </div>
        <!-- Booking Volume Trend -->
        <div class="col-md-6">
            <div class="card iocl-card shadow-sm h-100">
                <div class="card-header iocl-card-header"><i class="bi bi-bar-chart-fill me-2 text-primary"></i>Booking Volumetric Trend</div>
                <div class="card-body">
                    <canvas id="bookingsTrendChart" style="max-height: 250px;"></canvas>
                </div>
            </div>
        </div>
        <!-- Inventory Usage -->
        <div class="col-md-6">
            <div class="card iocl-card shadow-sm h-100">
                <div class="card-header iocl-card-header"><i class="bi bi-pie-chart-fill me-2 text-primary"></i>Top 5 Most Rented Items</div>
                <div class="card-body d-flex justify-content-center">
                    <div style="max-width: 280px; width: 100%;">
                        <canvas id="topItemsChart"></canvas>
                    </div>
                </div>
            </div>
        </div>
        <!-- Inventory Allocation -->
        <div class="col-md-6">
            <div class="card iocl-card shadow-sm h-100">
                <div class="card-header iocl-card-header"><i class="bi bi-donut-chart-fill me-2 text-primary"></i>Total Stock Allocation (Reserved vs Free)</div>
                <div class="card-body d-flex justify-content-center">
                    <div style="max-width: 280px; width: 100%;">
                        <canvas id="invAllocationChart"></canvas>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Recent Requests -->
    <div class="col-lg-12 mb-4">
        <div class="card iocl-card shadow-sm h-100">
            <div class="card-header iocl-card-header d-flex justify-content-between align-items-center">
                <span><i class="bi bi-clock-history me-2 text-primary"></i>Recent Rental Requests</span>
                <a href="AdminRequests.aspx" class="btn btn-sm btn-outline-primary border-0 fw-bold">Manage All</a>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 iocl-table">
                        <thead class="table-light">
                            <tr>
                                <th class="ps-3">Req No</th>
                                <th>Requester</th>
                                <th>Event Date</th>
                                <th>Status</th>
                                <th class="pe-3"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptRecentRequests" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td class="ps-3 fw-bold text-primary">#<%# Eval("RequestNumber") %></td>
                                        <td>
                                            <div class="fw-bold"><%# Eval("UserFullName") %></div>
                                            <span class="text-muted small" style="font-size: 0.75rem;"><%# Eval("UserDepartment") %></span>
                                        </td>
                                        <td><%# DirectCast(Eval("EventDate"), DateTime).ToString("dd/MM/yyyy") %></td>
                                        <td><%# GetStatusBadge(Container.DataItem) %></td>
                                        <td class="pe-3 text-end">
                                            <a href='AdminRequestDetails.aspx?id=<%# Eval("Id") %>' class="btn btn-xs btn-light border"><i class="bi bi-chevron-right"></i></a>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>

    <!-- Low Stock Alert -->
    <asp:Panel ID="pnlLowStock" runat="server" CssClass="card iocl-card shadow-sm border-start border-warning border-3 mb-4">
        <div class="card-header iocl-card-header bg-warning bg-opacity-10 fw-semibold">
            <i class="bi bi-exclamation-triangle-fill me-2 text-warning"></i>Low Stock Alert
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover mb-0 iocl-table">
                    <thead class="table-light">
                        <tr><th>Item</th><th>Category</th><th>Available</th><th>Total</th><th>Action</th></tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptLowStock" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("Name") %></td>
                                    <td><span class="text-muted small"><%# Eval("CategoryName") %></span></td>
                                    <td><span class="badge bg-danger"><%# Eval("AvailableQuantity") %> <%# Eval("UnitType") %></span></td>
                                    <td><%# Eval("TotalQuantity") %></td>
                                    <td><a href='InventoryEdit.aspx?id=<%# Eval("Id") %>' class="btn btn-sm btn-outline-warning">Update Stock</a></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </asp:Panel>

    <!-- Hidden chart data fields -->
    <asp:HiddenField ID="hfRevenueData" runat="server" />
    <asp:HiddenField ID="hfRevenueLabels" runat="server" />
    <asp:HiddenField ID="hfBookingData" runat="server" />
    <asp:HiddenField ID="hfBookingLabels" runat="server" />
    <asp:HiddenField ID="hfTopItemsData" runat="server" />
    <asp:HiddenField ID="hfTopItemsLabels" runat="server" />
    <asp:HiddenField ID="hfInvStatusData" runat="server" />
    <asp:HiddenField ID="hfInvStatusLabels" runat="server" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        function parseHF(id) { try { return JSON.parse(document.getElementById(id).value || '[]'); } catch(e){ return []; } }
        var monthlyRevenueData = parseHF('<%= hfRevenueData.ClientID %>');
        var monthlyRevenueLabels = parseHF('<%= hfRevenueLabels.ClientID %>');
        var bookingTrendData = parseHF('<%= hfBookingData.ClientID %>');
        var bookingTrendLabels = parseHF('<%= hfBookingLabels.ClientID %>');
        var topItemsData = parseHF('<%= hfTopItemsData.ClientID %>');
        var topItemsLabels = parseHF('<%= hfTopItemsLabels.ClientID %>');
        var invStatusData = parseHF('<%= hfInvStatusData.ClientID %>');
        var invStatusLabels = parseHF('<%= hfInvStatusLabels.ClientID %>');
    </script>
    <script src="Scripts/charts.js"></script>
</asp:Content>
