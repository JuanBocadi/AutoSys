/* ==========================================
   AUTOSYS - UTILIDADES UI
   Loading States, Skeleton Loaders, Empty States
   ========================================== */

// ========== LOADING STATES ==========

// Añadir estado de carga a un botón
function setButtonLoading(button, isLoading) {
    if (isLoading) {
        button.classList.add('btn-loading');
        button.disabled = true;
        button.setAttribute('data-original-text', button.textContent);
    } else {
        button.classList.remove('btn-loading');
        button.disabled = false;
        const originalText = button.getAttribute('data-original-text');
        if (originalText) {
            button.textContent = originalText;
            button.removeAttribute('data-original-text');
        }
    }
}

// Añadir overlay de carga a un contenedor
function setContainerLoading(container, isLoading) {
    if (isLoading) {
        container.classList.add('loading-overlay');
    } else {
        container.classList.remove('loading-overlay');
    }
}

// ========== SKELETON LOADERS ==========

// Crear skeleton loader para una tabla
function createTableSkeleton(rows = 5, columns = 4) {
    const tbody = document.createElement('tbody');
    
    for (let i = 0; i < rows; i++) {
        const tr = document.createElement('tr');
        for (let j = 0; j < columns; j++) {
            const td = document.createElement('td');
            const skeleton = document.createElement('div');
            skeleton.className = 'skeleton skeleton-text';
            td.appendChild(skeleton);
            tr.appendChild(td);
        }
        tbody.appendChild(tr);
    }
    
    return tbody;
}

// Crear skeleton loader para cards
function createCardSkeleton() {
    return `
        <div class="card">
            <div class="card-body">
                <div class="skeleton skeleton-title"></div>
                <div class="skeleton skeleton-text"></div>
                <div class="skeleton skeleton-text"></div>
                <div class="skeleton skeleton-text" style="width: 80%;"></div>
            </div>
        </div>
    `;
}

// Crear skeleton loader para lista
function createListSkeleton(items = 5) {
    let html = '';
    for (let i = 0; i < items; i++) {
        html += `
            <div class="d-flex align-items-center mb-3">
                <div class="skeleton skeleton-avatar me-3"></div>
                <div class="flex-grow-1">
                    <div class="skeleton skeleton-text mb-2"></div>
                    <div class="skeleton skeleton-text" style="width: 60%;"></div>
                </div>
            </div>
        `;
    }
    return html;
}

// ========== EMPTY STATES ==========

// Crear empty state
function createEmptyState(options = {}) {
    const {
        icon = 'fas fa-inbox',
        title = 'No hay datos',
        description = 'No se encontraron elementos para mostrar',
        actionText = null,
        actionCallback = null
    } = options;

    const emptyState = document.createElement('div');
    emptyState.className = 'empty-state';
    
    emptyState.innerHTML = `
        <div class="empty-state-icon">
            <i class="${icon}"></i>
        </div>
        <h3 class="empty-state-title">${title}</h3>
        <p class="empty-state-description">${description}</p>
        ${actionText ? `<button class="btn btn-primary empty-state-action">${actionText}</button>` : ''}
    `;

    if (actionText && actionCallback) {
        const actionBtn = emptyState.querySelector('.empty-state-action');
        actionBtn.addEventListener('click', actionCallback);
    }

    return emptyState;
}

// ========== UTILIDADES DE FETCH ==========

// Fetch con loading automático
async function fetchWithLoading(url, options = {}, loadingElement = null) {
    if (loadingElement) {
        if (loadingElement.tagName === 'BUTTON') {
            setButtonLoading(loadingElement, true);
        } else {
            setContainerLoading(loadingElement, true);
        }
    }

    try {
        const response = await fetch(url, options);
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const data = await response.json();
        return { success: true, data };
    } catch (error) {
        console.error('Fetch error:', error);
        return { success: false, error: error.message };
    } finally {
        if (loadingElement) {
            if (loadingElement.tagName === 'BUTTON') {
                setButtonLoading(loadingElement, false);
            } else {
                setContainerLoading(loadingElement, false);
            }
        }
    }
}

// ========== UTILIDADES DE CONFIRMACIÓN ==========

// Mostrar confirmación antes de una acción
function confirmAction(options = {}) {
    const {
        title = '¿Estás seguro?',
        message = 'Esta acción no se puede deshacer',
        confirmText = 'Confirmar',
        cancelText = 'Cancelar',
        type = 'warning' // success, error, warning, info
    } = options;

    return new Promise((resolve) => {
        // Aquí podrías integrar con Bootstrap Modal o un modal personalizado
        const result = confirm(`${title}\n\n${message}`);
        resolve(result);
    });
}

// ========== EXPORTAR FUNCIONES ==========

// Hacer funciones disponibles globalmente
window.AutoSysUI = {
    // Loading states
    setButtonLoading,
    setContainerLoading,
    
    // Skeleton loaders
    createTableSkeleton,
    createCardSkeleton,
    createListSkeleton,
    
    // Empty states
    createEmptyState,
    
    // Fetch utilities
    fetchWithLoading,
    
    // Confirmación
    confirmAction
};

// ========== EJEMPLOS DE USO ==========

/*
// 1. Loading en botón
const btn = document.getElementById('saveBtn');
AutoSysUI.setButtonLoading(btn, true);
// ... realizar operación
AutoSysUI.setButtonLoading(btn, false);

// 2. Loading en contenedor
const container = document.getElementById('dataContainer');
AutoSysUI.setContainerLoading(container, true);
// ... cargar datos
AutoSysUI.setContainerLoading(container, false);

// 3. Skeleton loader para tabla
const table = document.getElementById('myTable');
const skeleton = AutoSysUI.createTableSkeleton(5, 4);
table.querySelector('tbody').replaceWith(skeleton);
// ... cargar datos reales y reemplazar

// 4. Empty state
const emptyState = AutoSysUI.createEmptyState({
    icon: 'fas fa-users',
    title: 'No hay clientes',
    description: 'Aún no has agregado ningún cliente',
    actionText: 'Agregar Cliente',
    actionCallback: () => window.location.href = '/Clientes/Create'
});
container.appendChild(emptyState);

// 5. Fetch con loading
const result = await AutoSysUI.fetchWithLoading('/api/data', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data)
}, document.getElementById('submitBtn'));

if (result.success) {
    Toast.success('Datos guardados correctamente');
} else {
    Toast.error('Error al guardar: ' + result.error);
}

// 6. Confirmación
const confirmed = await AutoSysUI.confirmAction({
    title: '¿Eliminar cliente?',
    message: 'Esta acción no se puede deshacer',
    type: 'warning'
});

if (confirmed) {
    // Proceder con la eliminación
}
*/
