<%@ Page Title="Error" Language="VB" AutoEventWireup="false" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8"/><meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>Error — IOCL</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet"/>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet"/>
</head>
<body class="bg-light d-flex align-items-center justify-content-center" style="min-height:100vh;">
    <div class="text-center p-5">
        <i class="bi bi-exclamation-triangle-fill text-warning" style="font-size:5rem;"></i>
        <h1 class="mt-3 fw-bold">Something went wrong</h1>
        <p class="text-muted">An unexpected error occurred. Please try again or contact the administrator.</p>
        <a href="javascript:history.back()" class="btn btn-outline-secondary me-2">Go Back</a>
        <a href="Login.aspx" class="btn btn-primary">Go to Login</a>
    </div>
</body>
</html>
