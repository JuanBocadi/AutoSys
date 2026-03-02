// ========================================================
// AUTOSYS - SISTEMA DE GESTIÓN DE TALLER
// JavaScript principal con funcionalidades mejoradas
// ========================================================
//
// 📚 COMPONENTES UI DISPONIBLES:
// 
// 1️⃣ Toast Notifications:
//    Toast.success('Mensaje exitoso');
//    Toast.error('Mensaje de error');
//    Toast.warning('Advertencia');
//    Toast.info('Información');
//
// 2️⃣ Loading States:
//    AutoSysUI.setButtonLoading(button, true/false);
//    AutoSysUI.setContainerLoading(container, true/false);
//
// 3️⃣ Skeleton Loaders:
//    AutoSysUI.createTableSkeleton(rows, columns);
//    AutoSysUI.createCardSkeleton();
//    AutoSysUI.createListSkeleton(items);
//
// 4️⃣ Empty States:
//    AutoSysUI.createEmptyState({ icon, title, description, actionText, actionCallback });
//
// 5️⃣ Fetch con Loading:
//    AutoSysUI.fetchWithLoading(url, options, loadingElement);
//
// 6️⃣ Confirmación:
//    AutoSysUI.confirmAction({ title, message, type });
//
// 📖 Ver documentación completa en: GUIA-COMPONENTES-UI.md
// ========================================================

// ========== THEME MANAGEMENT WITH DARKREADER ==========
// DarkReader se carga de forma lazy desde _Layout.cshtml
// La inicialización se maneja allí con requestIdleCallback

document.addEventListener('DOMContentLoaded', function () {
    // ========== MOBILE MENU ==========
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileCloseBtn = document.getElementById('mobileCloseBtn');
    const mobileOverlay = document.getElementById('mobileOverlay');
    const sidebar = document.getElementById('sidebar');
    
    function openMobileMenu() {
        if (sidebar) sidebar.classList.add('mobile-open');
        if (mobileOverlay) mobileOverlay.classList.add('active');
        document.body.style.overflow = 'hidden';
    }
    
    function closeMobileMenu() {
        if (sidebar) sidebar.classList.remove('mobile-open');
        if (mobileOverlay) mobileOverlay.classList.remove('active');
        document.body.style.overflow = '';
    }
    
    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', openMobileMenu);
    }
    
    if (mobileCloseBtn) {
        mobileCloseBtn.addEventListener('click', closeMobileMenu);
    }
    
    if (mobileOverlay) {
        mobileOverlay.addEventListener('click', closeMobileMenu);
    }
    
    // Cerrar menú al hacer clic en un link (solo en móvil)
    const navItems = document.querySelectorAll('.sidebar-nav .nav-item');
    navItems.forEach(item => {
        item.addEventListener('click', function() {
            if (window.innerWidth <= 768) {
                closeMobileMenu();
            }
        });
    });
    
    // Cerrar menú al cambiar tamaño de ventana
    window.addEventListener('resize', function() {
        if (window.innerWidth > 768) {
            closeMobileMenu();
        }
    });
    
    // ========== THEME TOGGLE - DESKTOP ==========
    const themeToggle = document.getElementById('themeToggle');
    const themeIcon = document.getElementById('themeIcon');
    
    // ========== THEME TOGGLE - SIDEBAR ==========
    const themeToggleSidebar = document.getElementById('themeToggleSidebar');
    const themeIconSidebar = document.getElementById('themeIconSidebar');
    
    function toggleTheme() {
        const currentTheme = localStorage.getItem('theme') || 'light';
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        
        // Cambiar el tema con DarkReader (si ya cargó)
        if (typeof DarkReader !== 'undefined') {
            if (newTheme === 'dark') {
                DarkReader.setFetchMethod(window.fetch);
                DarkReader.enable({
                    brightness: 100,
                    contrast: 90,
                    sepia: 10
                });
            } else {
                DarkReader.disable();
            }
        }
        
        document.documentElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        
        // Actualizar ambos botones
        updateThemeButtons();
        
        // Actualizar el logo del sidebar
        updateSidebarLogo();
        
        // Disparar evento personalizado para actualizar gráficos
        document.dispatchEvent(new Event('themeChanged'));
    }
    
    function updateSidebarLogo() {
        const currentTheme = localStorage.getItem('theme') || 'light';
        
        // Logo del sidebar
        const sidebarLogoLight = document.querySelector('.sidebar-logo-img.logo-light');
        const sidebarLogoDark = document.querySelector('.sidebar-logo-img.logo-dark');
        
        if (sidebarLogoLight && sidebarLogoDark) {
            if (currentTheme === 'dark') {
                sidebarLogoLight.style.display = 'none';
                sidebarLogoDark.style.display = 'block';
            } else {
                sidebarLogoLight.style.display = 'block';
                sidebarLogoDark.style.display = 'none';
            }
        }
        
        // Logo del login (si existe)
        const loginLogoLight = document.querySelector('.login-logo-img.logo-light');
        const loginLogoDark = document.querySelector('.login-logo-img.logo-dark');
        
        if (loginLogoLight && loginLogoDark) {
            if (currentTheme === 'dark') {
                loginLogoLight.style.display = 'none';
                loginLogoDark.style.display = 'block';
            } else {
                loginLogoLight.style.display = 'block';
                loginLogoDark.style.display = 'none';
            }
        }
    }
    
    if (themeToggle) {
        updateThemeButtons();
        themeToggle.addEventListener('click', toggleTheme);
    }
    
    if (themeToggleSidebar) {
        themeToggleSidebar.addEventListener('click', toggleTheme);
    }
    
    // Actualizar el logo al cargar la página
    updateSidebarLogo();
    
    function updateThemeButtons() {
        const currentTheme = localStorage.getItem('theme') || 'light';
        const isDark = currentTheme === 'dark';
        const iconClass = isDark ? 'fas fa-sun' : 'fas fa-moon';
        const title = isDark ? 'Cambiar a modo claro' : 'Cambiar a modo oscuro';
        
        if (themeIcon) {
            themeIcon.className = iconClass;
            themeToggle.title = title;
        }
        
        if (themeIconSidebar) {
            themeIconSidebar.className = iconClass;
            themeToggleSidebar.title = title;
        }
    }
    
    // ========== ACTIVE NAV ITEM ==========
    const currentPath = window.location.pathname;
    navItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href && currentPath.includes(href) && href !== '/') {
            item.classList.add('active');
        } else if (href === '/' && (currentPath === '/' || currentPath === '/Home' || currentPath === '/Home/Index')) {
            item.classList.add('active');
        }
    });
    
    // ========== AUTO-CIERRE DE ALERTAS ==========
    const alerts = document.querySelectorAll('.autosys-alert');
    alerts.forEach(alert => {
        // Animar entrada
        setTimeout(() => {
            alert.style.opacity = '1';
            alert.style.transform = 'translateY(0)';
        }, 100);
        
        // Auto cerrar después de 5 segundos
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-20px)';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });
    
    // ========== CONFIRMACIONES DE ELIMINACIÓN ==========
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            const message = this.getAttribute('data-confirm-delete') || '¿Está seguro de que desea eliminar este elemento?';
            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
        });
    });
    
    // ========== TOOLTIPS DE BOOTSTRAP ==========
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
    
    // ========== VALIDACIÓN DE FORMULARIOS ==========
    const forms = document.querySelectorAll('.needs-validation');
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });
    
    // ========== SCROLL TO TOP ==========
    const scrollTopBtn = document.getElementById('scrollTopBtn');
    if (scrollTopBtn) {
        window.addEventListener('scroll', function() {
            if (window.pageYOffset > 300) {
                scrollTopBtn.classList.add('show');
            } else {
                scrollTopBtn.classList.remove('show');
            }
        });
        
        scrollTopBtn.addEventListener('click', function() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }
    
    // ========== TABLAS RESPONSIVAS ==========
    const tables = document.querySelectorAll('table:not(.table-responsive table)');
    tables.forEach(table => {
        if (!table.parentElement.classList.contains('table-responsive')) {
            const wrapper = document.createElement('div');
            wrapper.classList.add('table-responsive');
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
        }
    });
    
    // ========== LOADING STATES ==========
    const formSubmitButtons = document.querySelectorAll('form button[type="submit"]');
    formSubmitButtons.forEach(button => {
        button.closest('form').addEventListener('submit', function() {
            button.disabled = true;
            const originalText = button.innerHTML;
            button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Procesando...';
            
            // Rehabilitar después de 5 segundos como fallback
            setTimeout(() => {
                button.disabled = false;
                button.innerHTML = originalText;
            }, 5000);
        });
    });
    
    // ========== DRILL DOWN CARDS ==========
    const drillDownCards = document.querySelectorAll('.drill-down-card');
    drillDownCards.forEach(card => {
        card.style.cursor = 'pointer';
        card.addEventListener('click', function() {
            const url = this.getAttribute('data-url');
            if (url) {
                window.location.href = url;
            }
        });
    });
    
    // ========== NÚMEROS ANIMADOS ==========
    const animateNumbers = () => {
        const numbers = document.querySelectorAll('[data-animate-number]');
        numbers.forEach(element => {
            const target = parseInt(element.getAttribute('data-animate-number'));
            const duration = 1000;
            const step = target / (duration / 16);
            let current = 0;
            
            const timer = setInterval(() => {
                current += step;
                if (current >= target) {
                    element.textContent = target;
                    clearInterval(timer);
                } else {
                    element.textContent = Math.floor(current);
                }
            }, 16);
        });
    };
    
    // Ejecutar animación de números si están presentes
    if (document.querySelectorAll('[data-animate-number]').length > 0) {
        animateNumbers();
    }
    
    // ========== COPIAR AL PORTAPAPELES ==========
    const copyButtons = document.querySelectorAll('[data-copy]');
    copyButtons.forEach(button => {
        button.addEventListener('click', function() {
            const text = this.getAttribute('data-copy');
            navigator.clipboard.writeText(text).then(() => {
                const originalHTML = this.innerHTML;
                this.innerHTML = '<i class="fas fa-check"></i> Copiado';
                setTimeout(() => {
                    this.innerHTML = originalHTML;
                }, 2000);
            });
        });
    });
    
    console.log('AutoSys - Sistema cargado correctamente');
});