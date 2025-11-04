// Script para manejar la funcionalidad de Stock
document.addEventListener('DOMContentLoaded', function() {
    console.log('✓ Stock.js cargado correctamente');

    // Verificar Bootstrap
    if (typeof bootstrap === 'undefined') {
        console.error('✗ Bootstrap no está cargado');
        return;
    }
    console.log('✓ Bootstrap cargado correctamente');

    // ========== MODAL AJUSTAR STOCK ==========
    const modalAjustar = document.getElementById('ajustarStockModal');
    const formAjustar = document.getElementById('formAjustarStock');
    const botonesAjustar = document.querySelectorAll('.btn-ajustar-stock');

    console.log('Botones de ajustar encontrados:', botonesAjustar.length);

    if (modalAjustar && formAjustar) {
        const bsModalAjustar = new bootstrap.Modal(modalAjustar);

        botonesAjustar.forEach(boton => {
            boton.addEventListener('click', function(e) {
                e.preventDefault();
                console.log('Click en botón ajustar');

                const stockId = this.getAttribute('data-stock-id');
                const stockNombre = this.getAttribute('data-stock-nombre');
                const stockCantidad = this.getAttribute('data-stock-cantidad');
                const stockUnidad = this.getAttribute('data-stock-unidad') || 'unid.';

                document.getElementById('stockId').value = stockId;
                document.getElementById('modalStockNombre').textContent = stockNombre;
                document.getElementById('modalStockCantidad').textContent = stockCantidad;
                document.getElementById('modalStockUnidad').textContent = stockUnidad;
                document.getElementById('stockCantidad').value = '1';
                document.getElementById('stockTipo').value = 'agregar';

                bsModalAjustar.show();
                console.log('Modal ajustar mostrado');
            });
        });

        formAjustar.addEventListener('submit', function(e) {
            e.preventDefault();
            console.log('=== FORMULARIO AJUSTAR ENVIADO ===');

            const formData = new FormData(this);
            const id = formData.get('id');
            const cantidad = formData.get('cantidad');
            const tipo = formData.get('tipo');

            console.log('Datos:', { id, cantidad, tipo });

            if (!id || !cantidad || !tipo || cantidad <= 0) {
                alert('Por favor, complete todos los campos correctamente');
                return;
            }

            console.log('Validación OK. Enviando...');
            this.submit();
        });
    }

    // ========== MODAL EDITAR STOCK ==========
    const modalEditar = document.getElementById('editarStockModal');
    const formEditar = document.getElementById('formEditarStock');
    const botonesEditar = document.querySelectorAll('.btn-editar-stock');

    console.log('Botones de editar encontrados:', botonesEditar.length);

    if (modalEditar && formEditar) {
        const bsModalEditar = new bootstrap.Modal(modalEditar);

        botonesEditar.forEach(boton => {
            boton.addEventListener('click', function(e) {
                e.preventDefault();
                console.log('Click en botón editar');

                const id = this.getAttribute('data-stock-id');
                const nombre = this.getAttribute('data-stock-nombre');
                const descripcion = this.getAttribute('data-stock-descripcion') || '';
                const cantidad = this.getAttribute('data-stock-cantidad');
                const minimo = this.getAttribute('data-stock-minimo');
                const unidad = this.getAttribute('data-stock-unidad') || '';
                const precio = this.getAttribute('data-stock-precio');

                console.log('Datos del item a editar:', { id, nombre, cantidad, minimo, precio });

                document.getElementById('editId').value = id;
                document.getElementById('editNombre').textContent = nombre;
                document.getElementById('editNombreInput').value = nombre;
                document.getElementById('editDescripcion').value = descripcion;
                document.getElementById('editCantidad').value = cantidad;
                document.getElementById('editStockMinimo').value = minimo;
                document.getElementById('editUnidad').value = unidad;
                document.getElementById('editPrecio').value = precio;

                bsModalEditar.show();
                console.log('Modal editar mostrado');
            });
        });

        formEditar.addEventListener('submit', function(e) {
            e.preventDefault();
            console.log('=== FORMULARIO EDITAR ENVIADO ===');

            const formData = new FormData(this);
            console.log('Datos del formulario:');
            for (let pair of formData.entries()) {
                console.log('  ' + pair[0] + ':', pair[1]);
            }

            console.log('Enviando formulario de edición...');
            this.submit();
        });
    }

    // ========== MODAL ELIMINAR STOCK ==========
    const modalEliminar = document.getElementById('eliminarStockModal');
    const formEliminar = document.getElementById('formEliminarStock');
    const botonesEliminar = document.querySelectorAll('.btn-eliminar-stock');

    console.log('Botones de eliminar encontrados:', botonesEliminar.length);

    if (modalEliminar && formEliminar) {
        const bsModalEliminar = new bootstrap.Modal(modalEliminar);

        botonesEliminar.forEach(boton => {
            boton.addEventListener('click', function(e) {
                e.preventDefault();
                console.log('Click en botón eliminar');

                const id = this.getAttribute('data-stock-id');
                const nombre = this.getAttribute('data-stock-nombre');
                const cantidad = this.getAttribute('data-stock-cantidad');
                const unidad = this.getAttribute('data-stock-unidad') || 'unid.';

                console.log('Datos del item a eliminar:', { id, nombre, cantidad, unidad });

                document.getElementById('deleteId').value = id;
                document.getElementById('deleteNombre').textContent = nombre;
                document.getElementById('deleteCantidad').textContent = cantidad;
                document.getElementById('deleteUnidad').textContent = unidad;

                bsModalEliminar.show();
                console.log('Modal eliminar mostrado');
            });
        });

        formEliminar.addEventListener('submit', function(e) {
            e.preventDefault();
            console.log('=== FORMULARIO ELIMINAR ENVIADO ===');

            const id = document.getElementById('deleteId').value;
            console.log('ID a eliminar:', id);

            if (!id) {
                alert('Error: No se pudo obtener el ID del item');
                return;
            }

            console.log('Enviando formulario de eliminación...');
            this.submit();
        });
    }

    console.log('✓ Event listeners registrados correctamente');
});
