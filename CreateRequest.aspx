<%@ Page Title="New Rental Request" Language="VB" MasterPageFile="~/Site.Master" AutoEventWireup="false" CodeFile="CreateRequest.aspx.vb" Inherits="CreateRequest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row">
        <div class="col-12">
            <div class="card iocl-card shadow-sm">
                <div class="card-header iocl-card-header text-white" style="background-color: var(--iocl-blue);">
                    <h4 class="mb-0"><i class="bi bi-cart-plus me-2"></i>Create New Rental Request</h4>
                </div>
                <div class="card-body p-4">
                    <asp:Label ID="lblError" runat="server" CssClass="alert alert-danger d-block mb-3" Visible="false"/>

                    <!-- Event Information Section -->
                    <h5 class="fw-bold border-bottom pb-2 mb-3 text-dark"><i class="bi bi-info-circle-fill me-2 text-primary"></i>Event Details</h5>

                    <div class="row mb-3">
                        <div class="col-md-4">
                            <div class="mb-3">
                                <label class="form-label iocl-form-label">Event Date <span class="text-danger">*</span></label>
                                <asp:TextBox ID="EventDate" ClientIDMode="Static" runat="server" CssClass="form-control iocl-form-control" TextMode="Date" required="required" />
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="mb-3">
                                <label class="form-label iocl-form-label">Item Required From <span class="text-danger">*</span></label>
                                <asp:TextBox ID="StartDate" ClientIDMode="Static" runat="server" CssClass="form-control iocl-form-control" TextMode="Date" required="required" />
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="mb-3">
                                <label class="form-label iocl-form-label">Item Required Until <span class="text-danger">*</span></label>
                                <asp:TextBox ID="EndDate" ClientIDMode="Static" runat="server" CssClass="form-control iocl-form-control" TextMode="Date" required="required" />
                            </div>
                        </div>
                    </div>
                    
                    <div class="row mb-4">
                        <div class="col-md-6">
                            <div class="mb-3">
                                <label class="form-label iocl-form-label">In-Principal Approval Document <span class="text-danger">*</span></label>
                                <asp:FileUpload ID="fileDocument" runat="server" CssClass="form-control iocl-form-control" accept=".pdf,.png,.jpg,.jpeg" required="required" />
                                <div class="form-text text-muted small">Accepted formats: PDF, PNG, JPG, JPEG (Max 5MB)</div>
                            </div>
                        </div>
                    </div>

                    <!-- Items Selection Section -->
                    <div class="d-flex justify-content-between align-items-center border-bottom pb-2 mb-3">
                        <h5 class="fw-bold text-dark mb-0"><i class="bi bi-box-seam-fill me-2 text-primary"></i>Required Inventory Items</h5>
                        <button type="button" class="btn btn-sm btn-outline-primary" id="btn-add-item">
                            <i class="bi bi-plus-circle-fill me-1"></i>Add Item Row
                        </button>
                    </div>

                    <div class="table-responsive mb-4">
                        <table class="table table-bordered align-middle" id="itemsTable">
                            <thead class="table-light">
                                <tr>
                                    <th style="width: 40%">Inventory Item</th>
                                    <th style="width: 15%">Price per Unit</th>
                                    <th style="width: 15%">Stock Available</th>
                                    <th style="width: 15%">Required Qty</th>
                                    <th style="width: 15%">Line Total</th>
                                    <th style="width: 50px"></th>
                                </tr>
                            </thead>
                            <tbody id="itemsContainer">
                                <!-- Dynamic item rows will be added here via rental-calculator.js -->
                            </tbody>
                        </table>
                    </div>

                    <!-- Calculator Summary Bar -->
                    <div class="card bg-light border-0 p-3 mb-4">
                        <div class="row align-items-center">
                            <div class="col-md-6 text-muted small">
                                *All prices listed are per day rental charges. Overdue charges may apply.
                            </div>
                            <div class="col-md-6 text-md-end">
                                <h4 class="mb-0 fw-bold">Grand Total: <span style="color: var(--iocl-orange);" id="grandTotalText">₹0.00</span></h4>
                                <input type="hidden" name="GrandTotal" id="inputGrandTotal" value="0.00" />
                            </div>
                        </div>
                    </div>

                    <div class="d-flex justify-content-between">
                        <a href="MyRequests.aspx" class="btn btn-light border">Cancel &amp; Go Back</a>
                        <asp:Button ID="btnSubmit" runat="server" Text="Submit Rental Request" CssClass="btn btn-primary px-4 py-2" OnClick="btnSubmit_Click" />
                    </div>

                    <asp:HiddenField ID="hfSelectedItems" runat="server"/>
                    <asp:HiddenField ID="hfSelectedQtys" runat="server"/>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="c3" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        // Inject the available items list into JavaScript
        var availableItemsList = <%= GetAvailableItemsJson() %>;
    </script>
    <script src="Scripts/rental-calculator.js"></script>
    <script>
        // Populate hidden fields from dynamic rows before submission
        document.getElementById('form1').addEventListener('submit', function(e) {
            const ids = [], qtys = [];
            document.querySelectorAll('.item-row').forEach(row => {
                const select = row.querySelector('.item-select');
                const qtyInput = row.querySelector('.qty-input');
                if (select && select.value) {
                    ids.push(select.value);
                    qtys.push(qtyInput.value || 0);
                }
            });

            const hfItems = document.getElementById('<%= hfSelectedItems.ClientID %>');
            const hfQtys = document.getElementById('<%= hfSelectedQtys.ClientID %>');
            
            if (hfItems && hfQtys) {
                hfItems.value = ids.join(',');
                hfQtys.value = qtys.join(',');
            }

            // Client side validation
            if (ids.length === 0) {
                e.preventDefault();
                alert('Please select at least one inventory item.');
            }
        });
    </script>
</asp:Content>
