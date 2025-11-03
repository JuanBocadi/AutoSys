// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ========== THEME MANAGEMENT ==========
// Inicializar el tema al cargar la página
(function() {
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
})();

document.addEventListener('DOMContentLoaded', function () {
    // ========== THEME TOGGLE ==========
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    
    if (themeToggle) {
        // Establecer el estado inicial del botón
        updateThemeButton();
        
        themeToggle.addEventListener('click', function() {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
            
            // Cambiar el tema
            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            
            // Actualizar el botón
            updateThemeButton();
        });
    }
    
    function updateThemeButton() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        
        if (currentTheme === 'dark') {
            themeIcon.className = 'fas fa-sun';
            themeToggle.title = 'Cambiar a modo claro';
        } else {
            themeIcon.className = 'fas fa-moon';
            themeToggle.title = 'Cambiar a modo oscuro';
        }
    }
    
    // ========== AUTO-CIERRE DE ALERTAS ==========
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