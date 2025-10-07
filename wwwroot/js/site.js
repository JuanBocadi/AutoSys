// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-cierre de alertas globales (2 segundos)
document.addEventListener('DOMContentLoaded', function () {
    function closeAlertById(id) {
        var el = document.getElementById(id);
        if (!el) return;
        if (window.bootstrap && bootstrap.Alert) {
            var alert = new bootstrap.Alert(el);
            alert.close();
        } else {
            // Fallback simple si Bootstrap aún no está disponible
            el.classList.remove('show');
        }
    }

    setTimeout(function () { closeAlertById('autosys-alert-success'); }, 2000);
    setTimeout(function () { closeAlertById('autosys-alert-error'); }, 2000);
});