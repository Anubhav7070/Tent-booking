<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Login.aspx.vb" Inherits="Login" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Employee Login — IOCL Community Hall</title>
    <meta name="description" content="IOCL Panipat Refinery Township Community Hall Portal — Secure Employee Login" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800&display=swap" rel="stylesheet" />
    <style>
        :root { --iocl-blue:#0054A6; --iocl-orange:#FF6B00; --iocl-dark-orange:#DC4A1A; --bg-dark:#0a0f1e; --card-bg:rgba(15,23,42,0.85); --text-primary:#F1F5F9; --text-secondary:#94A3B8; }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { min-height:100vh; background:var(--bg-dark); display:flex; align-items:center; justify-content:center; font-family:'Inter',system-ui,sans-serif; overflow-x:hidden; position:relative; }
        body::before { content:''; position:absolute; inset:0; background: radial-gradient(ellipse 80% 60% at 20% 20%, rgba(220,74,26,0.12) 0%, transparent 60%), radial-gradient(ellipse 60% 50% at 80% 80%, rgba(0,84,166,0.12) 0%, transparent 60%); pointer-events:none; z-index:1; }
        body::after { content:''; position:absolute; inset:0; background-image:linear-gradient(rgba(255,255,255,0.015) 1px, transparent 1px),linear-gradient(90deg, rgba(255,255,255,0.015) 1px, transparent 1px); background-size:40px 40px; pointer-events:none; z-index:2; }
        .login-wrap { position:relative; z-index:10; width:100%; max-width:440px; padding:1rem; }
        .login-card { background:var(--card-bg); border:1px solid rgba(255,255,255,0.08); border-radius:20px; padding:2.5rem 2.25rem 2rem; backdrop-filter:blur(24px); box-shadow:0 30px 70px rgba(0,0,0,0.5),0 0 0 1px rgba(255,255,255,0.04),inset 0 1px 0 rgba(255,255,255,0.06); }
        .login-logo { text-align:center; margin-bottom:1.25rem; }
        .logo-wrapper { background:#fff; padding:8px 16px; border-radius:8px; display:inline-flex; align-items:center; justify-content:center; box-shadow:0 4px 12px rgba(0,0,0,0.15); }
        .logo-wrapper img { height:48px; object-fit:contain; }
        .login-org { text-align:center; font-size:0.72rem; font-weight:700; color:var(--iocl-orange); letter-spacing:1.2px; text-transform:uppercase; margin-bottom:0.2rem; }
        .login-title { font-size:1.35rem; font-weight:700; letter-spacing:-0.02em; background:linear-gradient(135deg,#FFF 60%,#FF9E59 100%); -webkit-background-clip:text; -webkit-text-fill-color:transparent; text-align:center; margin-bottom:0.2rem; }
        .login-sub { font-size:0.8rem; color:var(--text-secondary); text-align:center; margin-bottom:1.8rem; line-height:1.5; }
        .field-label { display:block; font-size:0.72rem; font-weight:700; color:var(--text-secondary); margin-bottom:0.4rem; letter-spacing:0.1em; text-transform:uppercase; }
        .field-hint { font-size:0.68rem; color:#64748b; font-weight:400; margin-left:0.4rem; text-transform:none; letter-spacing:0; }
        .input-wrap { position:relative; margin-bottom:1.1rem; }
        .input-icon { position:absolute; left:14px; top:50%; transform:translateY(-50%); color:#64748b; font-size:0.95rem; pointer-events:none; }
        .field-input { width:100%; height:44px; background:rgba(2,6,23,0.6); border:1.5px solid rgba(255,255,255,0.08); border-radius:10px; padding:0 0.9rem 0 2.6rem; font-size:0.92rem; color:var(--text-primary); outline:none; transition:border-color 0.2s,box-shadow 0.2s; }
        .field-input:focus { border-color:var(--iocl-orange); box-shadow:0 0 0 3px rgba(255,107,0,0.15); }
        .form-row { display:flex; justify-content:space-between; align-items:center; margin-bottom:1.4rem; font-size:0.82rem; }
        .form-row label { display:flex; align-items:center; gap:0.35rem; color:var(--text-secondary); cursor:pointer; }
        .form-row a { color:var(--iocl-orange); text-decoration:none; font-weight:600; }
        .btn-login { width:100%; height:46px; background:linear-gradient(135deg,var(--iocl-dark-orange) 0%,var(--iocl-orange) 100%); border:none; border-radius:10px; color:#fff; font-size:0.95rem; font-weight:700; font-family:inherit; cursor:pointer; transition:all 0.2s; box-shadow:0 6px 20px rgba(220,74,26,0.35); }
        .btn-login:hover { transform:translateY(-1px); box-shadow:0 10px 25px rgba(220,74,26,0.45); }
        .error-box { background:rgba(220,38,38,0.15); border:1px solid rgba(220,38,38,0.35); border-left:4px solid #EF4444; border-radius:8px; padding:0.7rem 0.9rem; margin-bottom:1.1rem; display:flex; align-items:flex-start; gap:0.6rem; }
        .error-box span { color:#FCA5A5; font-size:0.83rem; font-weight:600; }
        .footer-note { text-align:center; margin-top:1.5rem; font-size:0.71rem; color:#64748b; line-height:1.6; }
        .footer-note span { color:var(--iocl-orange); font-weight:600; }
        .invalid-msg { font-size:0.75rem; color:#fca5a5; margin-top:0.25rem; display:none; }
    </style>
</head>
<body>
<form id="form1" runat="server">
    <div class="login-wrap">
        <div class="login-card">
            <div class="login-logo">
                <div class="logo-wrapper">
                    <img src="https://iocl.com/assets/images/logo.gif" alt="IOCL Logo" onerror="this.src='Content/iocl-logo.png'" />
                </div>
            </div>
            <div class="login-org">Indian Oil Corporation Limited</div>
            <div class="login-title">Community Hall Portal</div>
            <div class="login-sub">Panipat Refinery Township — Secure Employee Login</div>

            <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-box">
                <i class="bi bi-exclamation-circle-fill" style="color:#FCA5A5;font-size:1.1rem;flex-shrink:0;"></i>
                <span><asp:Label ID="lblError" runat="server" /></span>
            </asp:Panel>

            <label class="field-label" for="txtEmployeeId">Employee ID <span class="field-hint">(8-digit numeric)</span></label>
            <div class="input-wrap">
                <i class="bi bi-person-badge input-icon"></i>
                <asp:TextBox ID="txtEmployeeId" runat="server" CssClass="field-input" placeholder="e.g. 12345678" MaxLength="8" autocomplete="username" />
            </div>
            <div class="invalid-msg" id="empIdError"><i class="bi bi-x-circle me-1"></i>Employee ID must be exactly 8 numeric digits.</div>

            <label class="field-label" for="txtPassword" style="margin-top:0.8rem;">Password</label>
            <div class="input-wrap">
                <i class="bi bi-lock-fill input-icon"></i>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="field-input" placeholder="••••••••" autocomplete="current-password" />
            </div>

            <div class="form-row" style="margin-top:0.5rem;">
                <label>
                    <asp:CheckBox ID="chkRemember" runat="server" style="accent-color:#003399;" />
                    Keep me signed in
                </label>
                <a href="#">Forgot Password?</a>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn-login" OnClick="btnLogin_Click" />
        </div>
        <div class="footer-note">
            © <%= DateTime.Now.Year %> <span>Indian Oil Corporation Ltd.</span><br />
            Panipat Refinery Township — Internal Portal
        </div>
    </div>
</form>
<script>
    const empInput = document.getElementById('<%= txtEmployeeId.ClientID %>');
    const empError = document.getElementById('empIdError');
    if (empInput) {
        empInput.addEventListener('input', function () { this.value = this.value.replace(/\D/g, '').slice(0, 8); });
        empInput.addEventListener('blur', function () {
            if (this.value.length > 0 && this.value.length !== 8) { this.style.borderColor='#DC2626'; empError.style.display='block'; }
            else { this.style.borderColor=''; empError.style.display='none'; }
        });
    }

    document.getElementById('form1').addEventListener('submit', function (e) {
        if (empInput) {
            const val = empInput.value;
            if (val.length !== 8 || !/^\d{8}$/.test(val)) {
                e.preventDefault();
                empInput.style.borderColor = '#DC2626';
                empError.style.display = 'block';
                empInput.focus();
            }
        }
    });
</script>
</body>
</html>
