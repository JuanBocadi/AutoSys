/**
 * AutoSys - Mobile Menu Handler
 * Maneja la apertura/cierre del menú lateral en dispositivos móviles
 * Toggle completo con botón hamburguesa
 */

(function() {
    'use strict';
    
    // Elementos DOM
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileOverlay = document.getElementById('mobileOverlay');
    const sidebar = document.getElementById('sidebar');
    
    // Verificar que los elementos existen
    if (!mobileMenuToggle || !mobileOverlay || !sidebar) {
        return;
    }
    
    /**
     * Abre el menú móvil
     */
    function openMobileMenu() {
        sidebar.classList.add('mobile-open');
        mobileOverlay.classList.add('active');
        document.body.style.overflow = 'hidden'; // Prevenir scroll del body
    }
    
    /**
     * Cierra el menú móvil
     */
    function closeMobileMenu() {
        sidebar.classList.remove('mobile-open');
        mobileOverlay.classList.remove('active');
        document.body.style.overflow = ''; // Restaurar scroll
    }
    
    /**
     * Toggle del menú móvil
     */
    function toggleMobileMenu(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('Toggle clicked, sidebar open:', sidebar.classList.contains('mobile-open'));
        
        if (sidebar.classList.contains('mobile-open')) {
            console.log('Closing menu');
            closeMobileMenu();
        } else {
            console.log('Opening menu');
            openMobileMenu();
        }
    }
    
    // Event Listeners
    mobileMenuToggle.addEventListener('click', toggleMobileMenu, true);
    mobileOverlay.addEventListener('click', closeMobileMenu);
    
    // Cerrar menú al hacer clic en un enlace de navegación (solo en móvil)
    const navLinks = sidebar.querySelectorAll('.nav-item');
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            // Solo cerrar si estamos en vista móvil
            if (window.innerWidth <= 768) {
                closeMobileMenu();
            }
        });
    });
    
    // Cerrar menú al presionar ESC
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && sidebar.classList.contains('mobile-open')) {
            closeMobileMenu();
        }
    });
    
    // Cerrar menú al cambiar de orientación o resize (si se pasa a desktop)
    let resizeTimer;
    window.addEventListener('resize', function() {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function() {
            if (window.innerWidth > 768 && sidebar.classList.contains('mobile-open')) {
                closeMobileMenu();
            }
        }, 250);
    });
    
})();
