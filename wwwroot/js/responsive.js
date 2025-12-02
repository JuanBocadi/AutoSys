// ========================================
// AUTOSYS - RESPONSIVE ENHANCEMENTS
// ========================================

(function() {
    'use strict';

    // Detectar tamaño de pantalla
    function getScreenSize() {
        const width = window.innerWidth;
        if (width <= 480) return 'xs';
        if (width <= 768) return 'sm';
        if (width <= 1024) return 'md';
        return 'lg';
    }

    // Hacer tablas responsivas automáticamente
    function makeTablesResponsive() {
        const tables = document.querySelectorAll('.table:not(.table-card-mobile)');
        
        tables.forEach(table => {
            // Si la tabla no está en un .table-responsive, envuélvela
            if (!table.closest('.table-responsive')) {
                const wrapper = document.createElement('div');
                wrapper.className = 'table-responsive';
                table.parentNode.insertBefore(wrapper, table);
                wrapper.appendChild(table);
            }

            // En móviles, convertir a modo card si es muy ancha
            if (getScreenSize() === 'xs' || getScreenSize() === 'sm') {
                const headers = Array.from(table.querySelectorAll('thead th')).map(th => th.textContent.trim());
                
                table.querySelectorAll('tbody td').forEach((td, index) => {
                    const headerIndex = index % headers.length;
                    if (headers[headerIndex]) {
                        td.setAttribute('data-label', headers[headerIndex]);
                    }
                });
            }
        });
    }

    // Mejorar inputs numéricos en móviles
    function enhanceMobileInputs() {
        if (getScreenSize() === 'xs' || getScreenSize() === 'sm') {
            // Inputs numéricos: activar teclado numérico
            document.querySelectorAll('input[type="number"]').forEach(input => {
                input.setAttribute('inputmode', 'numeric');
            });

            // Inputs de teléfono
            document.querySelectorAll('input[type="tel"]').forEach(input => {
                input.setAttribute('inputmode', 'tel');
            });

            // Inputs de email
            document.querySelectorAll('input[type="email"]').forEach(input => {
                input.setAttribute('inputmode', 'email');
            });
        }
    }

    // Mejorar modales en móviles
    function enhanceMobileModals() {
        if ((getScreenSize() === 'xs' || getScreenSize() === 'sm') && typeof bootstrap !== 'undefined') {
            document.querySelectorAll('.modal').forEach(modal => {
                try {
                    modal.addEventListener('shown.bs.modal', function() {
                        // Prevenir scroll del body
                        document.body.style.overflow = 'hidden';
                        document.body.style.position = 'fixed';
                        document.body.style.width = '100%';
                    });
    
                    modal.addEventListener('hidden.bs.modal', function() {
                        // Restaurar scroll
                        document.body.style.overflow = '';
                        document.body.style.position = '';
                        document.body.style.width = '';
                    });
                } catch (e) {
                    console.warn('Error configurando modal:', e);
                }
            });
        }
    }

    // Touch gestures para el sidebar en móvil
    function addMobileGestures() {
        const sidebar = document.querySelector('.autosys-sidebar');
        const overlay = document.querySelector('.mobile-overlay');
        
        if (!sidebar || getScreenSize() === 'lg') return;

        let touchStartX = 0;
        let touchEndX = 0;

        function handleSwipe() {
            const swipeDistance = touchEndX - touchStartX;
            
            if (Math.abs(swipeDistance) > 50) { // Mínimo 50px de swipe
                if (swipeDistance < 0 && sidebar.classList.contains('mobile-open')) {
                    // Swipe izquierda: cerrar sidebar
                    sidebar.classList.remove('mobile-open');
                    overlay.classList.remove('active');
                }
            }
        }

        sidebar.addEventListener('touchstart', e => {
            touchStartX = e.changedTouches[0].screenX;
        }, { passive: true });

        sidebar.addEventListener('touchend', e => {
            touchEndX = e.changedTouches[0].screenX;
            handleSwipe();
        }, { passive: true });
    }

    // Ajustar viewport height para móviles (problema de barra de direcciones)
    function adjustViewportHeight() {
        const vh = window.innerHeight * 0.01;
        document.documentElement.style.setProperty('--vh', `${vh}px`);
    }

    // Lazy loading de imágenes
    function lazyLoadImages() {
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        if (img.dataset.src) {
                            img.src = img.dataset.src;
                            img.removeAttribute('data-src');
                            imageObserver.unobserve(img);
                        }
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }
    }

    // Prevenir zoom en inputs en iOS
    function preventIOSZoom() {
        if (/iPhone|iPad|iPod/.test(navigator.userAgent)) {
            const meta = document.querySelector('meta[name="viewport"]');
            if (meta) {
                const content = meta.getAttribute('content');
                if (!content.includes('maximum-scale')) {
                    meta.setAttribute('content', content + ', maximum-scale=1.0, user-scalable=no');
                }
            }
        }
    }

    // Mejorar performance de scroll
    let ticking = false;
    function optimizeScroll(callback) {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                callback();
                ticking = false;
            });
            ticking = true;
        }
    }

    // Inicializar todas las mejoras
    function init() {
        try {
            makeTablesResponsive();
            enhanceMobileInputs();
            enhanceMobileModals();
            addMobileGestures();
            adjustViewportHeight();
            lazyLoadImages();
            // preventIOSZoom(); // Descomenta si es necesario
        } catch (e) {
            console.error('Error inicializando responsive.js:', e);
        }
        
        // Ajustar viewport height en resize/orientación
        window.addEventListener('resize', () => {
            optimizeScroll(adjustViewportHeight);
        });

        window.addEventListener('orientationchange', () => {
            setTimeout(adjustViewportHeight, 100);
        });

        // Re-aplicar mejoras después de actualizaciones dinámicas
        try {
            const observer = new MutationObserver(() => {
                try {
                    makeTablesResponsive();
                    enhanceMobileInputs();
                    lazyLoadImages();
                } catch (e) {
                    console.warn('Error en MutationObserver:', e);
                }
            });
    
            observer.observe(document.body, {
                childList: true,
                subtree: true
            });
        } catch (e) {
            console.warn('MutationObserver no disponible:', e);
        }
    }

    // Ejecutar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();
