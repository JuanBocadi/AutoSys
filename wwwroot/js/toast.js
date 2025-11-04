/* ==========================================
   AUTOSYS - SISTEMA DE TOAST NOTIFICATIONS
   ========================================== */

// Sistema de notificaciones Toast
const Toast = {
    container: null,

    // Inicializar el contenedor de toasts
    init() {
        if (!this.container) {
            this.container = document.createElement('div');
            this.container.className = 'toast-container';
            document.body.appendChild(this.container);
        }
    },

    // Mostrar un toast
    show(options) {
        this.init();

        const {
            title = '',
            message = '',
            type = 'info', // success, error, warning, info
            duration = 5000,
            icon = this.getIcon(type)
        } = options;

        // Crear elemento toast
        const toast = document.createElement('div');
        toast.className = `toast toast-${type}`;
        
        toast.innerHTML = `
            <div class="toast-icon">${icon}</div>
            <div class="toast-content">
                ${title ? `<div class="toast-title">${title}</div>` : ''}
                ${message ? `<div class="toast-message">${message}</div>` : ''}
            </div>
            <button class="toast-close" aria-label="Cerrar">&times;</button>
        `;

        // Añadir al contenedor
        this.container.appendChild(toast);

        // Botón de cerrar
        const closeBtn = toast.querySelector('.toast-close');
        closeBtn.addEventListener('click', () => this.remove(toast));

        // Auto-remover después de la duración especificada
        if (duration > 0) {
            setTimeout(() => this.remove(toast), duration);
        }

        return toast;
    },

    // Remover un toast
    remove(toast) {
        toast.classList.add('removing');
        setTimeout(() => {
            if (toast.parentElement) {
                toast.parentElement.removeChild(toast);
            }
        }, 300); // Duración de la animación
    },

    // Obtener icono según el tipo
    getIcon(type) {
        const icons = {
            success: '<i class="fas fa-check-circle"></i>',
            error: '<i class="fas fa-times-circle"></i>',
            warning: '<i class="fas fa-exclamation-triangle"></i>',
            info: '<i class="fas fa-info-circle"></i>'
        };
        return icons[type] || icons.info;
    },

    // Métodos de conveniencia
    success(message, title = 'Éxito', duration = 5000) {
        return this.show({ type: 'success', title, message, duration });
    },

    error(message, title = 'Error', duration = 7000) {
        return this.show({ type: 'error', title, message, duration });
    },

    warning(message, title = 'Advertencia', duration = 6000) {
        return this.show({ type: 'warning', title, message, duration });
    },

    info(message, title = 'Información', duration = 5000) {
        return this.show({ type: 'info', title, message, duration });
    }
};

// Hacer Toast disponible globalmente
window.Toast = Toast;

// Ejemplo de uso:
// Toast.success('Operación completada exitosamente', 'Éxito');
// Toast.error('No se pudo completar la operación', 'Error');
// Toast.warning('Por favor revisa los datos ingresados', 'Advertencia');
// Toast.info('Esta es una notificación informativa', 'Info');
