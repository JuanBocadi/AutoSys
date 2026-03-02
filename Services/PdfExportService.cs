using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using AutoSys.Models;

namespace AutoSys.Services
{
    /// Genera documentos PDF profesionales para cada tipo de reporte usando QuestPDF
    public class PdfExportService : IPdfExportService
    {
        private const string ColorPrimario = "#1e3a5f";
        private const string ColorSecundario = "#3b82f6";
        private const string ColorExito = "#10b981";
        private const string ColorAdvertencia = "#f59e0b";
        private const string ColorPeligro = "#ef4444";

        public PdfExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: INGRESOS POR PERÍODO
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteIngresosPdf(DateTime desde, DateTime hasta, int totalIngresos, int enProceso, int finalizados, IEnumerable<Ingreso> ingresos)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Ingresos por Período",
                        $"Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            // KPIs
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Total Ingresos", totalIngresos.ToString(), ColorSecundario),
                                ("En Proceso", enProceso.ToString(), ColorAdvertencia),
                                ("Finalizados", finalizados.ToString(), ColorExito)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Detalle de Ingresos"));

                            // Tabla
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.2f);  // Fecha
                                    columns.RelativeColumn(2);     // Cliente
                                    columns.RelativeColumn(1.5f);  // Vehículo
                                    columns.RelativeColumn(2.5f);  // Diagnóstico
                                    columns.RelativeColumn(1);     // Estado
                                    columns.RelativeColumn(1.2f);  // Egreso
                                    columns.RelativeColumn(0.7f);  // Días
                                });

                                HeaderTabla(table, "Fecha", "Cliente", "Vehículo", "Diagnóstico", "Estado", "Egreso", "Días");

                                int fila = 0;
                                foreach (var ingreso in ingresos)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, ingreso.FechaIngreso.ToString("dd/MM/yyyy"), bg);
                                    Celda(table, $"{ingreso.Vehiculo?.Cliente?.Nombre} {ingreso.Vehiculo?.Cliente?.Apellido}", bg);
                                    Celda(table, $"{ingreso.Vehiculo?.Patente}", bg);
                                    Celda(table, (ingreso.Diagnostico ?? "").Length > 40 ? ingreso.Diagnostico!.Substring(0, 40) + "..." : ingreso.Diagnostico ?? string.Empty, bg);
                                Celda(table, ingreso.Estado, bg);
                                    Celda(table, ingreso.FechaEgreso?.ToString("dd/MM/yyyy") ?? "-", bg);
                                    CeldaDerecha(table, ingreso.DiasEnTaller.ToString(), bg);
                                }
                            });
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: FACTURACIÓN POR PERÍODO
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteFacturacionPdf(DateTime desde, DateTime hasta, int totalFacturas, int totalPagadas, decimal totalRecaudado, decimal totalPendiente, IEnumerable<Factura> facturas)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Facturación por Período",
                        $"Período: {desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Total Facturas", totalFacturas.ToString(), ColorSecundario),
                                ("Pagadas", totalPagadas.ToString(), ColorExito),
                                ("Recaudado", $"${totalRecaudado:N2}", ColorExito),
                                ("Pendiente", $"${totalPendiente:N2}", ColorAdvertencia)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Detalle de Facturas"));

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.5f);  // Número
                                    columns.RelativeColumn(1.2f);  // Fecha
                                    columns.RelativeColumn(2);     // Cliente
                                    columns.RelativeColumn(1);     // Estado
                                    columns.RelativeColumn(1);     // Método
                                    columns.RelativeColumn(1.2f);  // Subtotal
                                    columns.RelativeColumn(1);     // IVA
                                    columns.RelativeColumn(1.2f);  // Total
                                });

                                HeaderTabla(table, "Número", "Fecha", "Cliente", "Estado", "M. Pago", "Subtotal", "IVA", "Total");

                                int fila = 0;
                                foreach (var factura in facturas)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, factura.NumeroFactura, bg);
                                    Celda(table, factura.FechaEmision.ToString("dd/MM/yyyy"), bg);
                                    Celda(table, $"{factura.Cliente?.Nombre} {factura.Cliente?.Apellido}", bg);
                                    Celda(table, factura.Estado, bg);
                                    Celda(table, factura.MetodoPago ?? "-", bg);
                                    CeldaDerecha(table, $"${factura.Subtotal:N2}", bg);
                                    CeldaDerecha(table, $"${factura.IVA:N2}", bg);
                                    CeldaDerecha(table, $"${factura.Total:N2}", bg);
                                }

                                // Fila de totales
                                table.Cell().ColumnSpan(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).AlignRight().Text("TOTALES:").Bold().FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).AlignRight().Text($"${facturas.Sum(f => f.Subtotal):N2}").Bold().FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).AlignRight().Text($"${facturas.Sum(f => f.IVA):N2}").Bold().FontSize(9);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                    .Padding(6).AlignRight().Text($"${facturas.Sum(f => f.Total):N2}").Bold().FontSize(9);
                            });
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: STOCK BAJO
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteStockBajoPdf(int totalItems, int itemsCriticos, int itemsBajo, IEnumerable<Stock> items)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Stock Bajo y Crítico",
                        $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Items Críticos", itemsCriticos.ToString(), ColorPeligro),
                                ("Items Bajo", itemsBajo.ToString(), ColorAdvertencia),
                                ("Total Items", totalItems.ToString(), ColorSecundario)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Items con Stock Bajo"));

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.5f);  // Nombre
                                    columns.RelativeColumn(2.5f);  // Descripción
                                    columns.RelativeColumn(1);     // Cantidad
                                    columns.RelativeColumn(1);     // Mínimo
                                    columns.RelativeColumn(1);     // Nivel
                                    columns.RelativeColumn(1.2f);  // Precio
                                });

                                HeaderTabla(table, "Producto", "Descripción", "Cant.", "Mínimo", "Nivel", "Precio");

                                int fila = 0;
                                foreach (var item in items)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, item.Nombre, bg);
                                    Celda(table, item.Descripcion ?? "-", bg);
                                    CeldaDerecha(table, item.Cantidad.ToString(), bg);
                                    CeldaDerecha(table, item.StockMinimo.ToString(), bg);
                                    CeldaDerecha(table, item.NivelStock, bg);
                                    CeldaDerecha(table, $"${item.PrecioUnitario:N2}", bg);
                                }
                            });

                            // Recomendaciones de reorden
                            var criticos = items.Where(i => i.Cantidad < i.StockMinimo).ToList();
                            if (criticos.Any())
                            {
                                col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Recomendaciones de Reorden"));

                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2.5f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    HeaderTabla(table, "Producto", "Cantidad a Reponer", "Costo Estimado");

                                    int fila2 = 0;
                                    foreach (var item in criticos)
                                    {
                                        var bg = fila2++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                        var cantReorden = (item.StockMinimo * 3) - item.Cantidad;

                                        Celda(table, item.Nombre, bg);
                                        CeldaDerecha(table, $"{cantReorden} {item.Unidad}", bg);
                                        CeldaDerecha(table, $"${(cantReorden * item.PrecioUnitario):N2}", bg);
                                    }
                                });
                            }
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: TIEMPOS DE REPARACIÓN
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteTiemposReparacionPdf(int totalReparaciones, double tiempoPromedio, double tiempoMinimo, double tiempoMaximo, IEnumerable<Ingreso> ingresos)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Tiempos de Reparación",
                        $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Reparaciones", totalReparaciones.ToString(), ColorSecundario),
                                ("Promedio", $"{tiempoPromedio:F1} días", ColorAdvertencia),
                                ("Mínimo", $"{tiempoMinimo:F1} días", ColorExito),
                                ("Máximo", $"{tiempoMaximo:F1} días", ColorPeligro)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Detalle de Reparaciones Finalizadas"));

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1.2f);  // Ingreso
                                    columns.RelativeColumn(2);     // Cliente
                                    columns.RelativeColumn(1.5f);  // Vehículo
                                    columns.RelativeColumn(2.5f);  // Diagnóstico
                                    columns.RelativeColumn(1.2f);  // Egreso
                                    columns.RelativeColumn(0.8f);  // Días
                                });

                                HeaderTabla(table, "Ingreso", "Cliente", "Vehículo", "Diagnóstico", "Egreso", "Días");

                                int fila = 0;
                                foreach (var ingreso in ingresos)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, ingreso.FechaIngreso.ToString("dd/MM/yyyy"), bg);
                                    Celda(table, $"{ingreso.Vehiculo?.Cliente?.Nombre} {ingreso.Vehiculo?.Cliente?.Apellido}", bg);
                                    Celda(table, $"{ingreso.Vehiculo?.Patente}", bg);
                                    Celda(table, (ingreso.Diagnostico ?? "").Length > 35 ? ingreso.Diagnostico!.Substring(0, 35) + "..." : ingreso.Diagnostico ?? string.Empty, bg);
                                    Celda(table, ingreso.FechaEgreso?.ToString("dd/MM/yyyy") ?? "-", bg);
                                    CeldaDerecha(table, ingreso.DiasEnTaller.ToString(), bg);
                                }
                            });
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: RENTABILIDAD POR CLIENTES
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteRentabilidadPdf(int totalClientes, int clientesConFactura, decimal totalRecaudado, decimal ticketPromedio, dynamic rentabilidad)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Rentabilidad por Clientes",
                        $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Total Clientes", totalClientes.ToString(), ColorSecundario),
                                ("Con Factura", clientesConFactura.ToString(), ColorExito),
                                ("Recaudado", $"${totalRecaudado:N2}", ColorExito),
                                ("Ticket Prom.", $"${ticketPromedio:N2}", ColorAdvertencia)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Rentabilidad por Cliente"));

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2.5f);  // Cliente
                                    columns.RelativeColumn(1);     // Vehículos
                                    columns.RelativeColumn(1);     // Ingresos
                                    columns.RelativeColumn(1);     // Facturas
                                    columns.RelativeColumn(1.3f);  // Facturado
                                    columns.RelativeColumn(1.3f);  // Servicios
                                    columns.RelativeColumn(1.3f);  // Repuestos
                                });

                                HeaderTabla(table, "Cliente", "Vehic.", "Ingr.", "Fact.", "Facturado", "Servicios", "Repuestos");

                                int fila = 0;
                                foreach (var r in rentabilidad)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, $"{r.Cliente.Nombre} {r.Cliente.Apellido}", bg);
                                    CeldaDerecha(table, r.TotalVehiculos.ToString(), bg);
                                    CeldaDerecha(table, r.TotalIngresos.ToString(), bg);
                                    CeldaDerecha(table, r.CantidadFacturas.ToString(), bg);
                                    CeldaDerecha(table, $"${r.TotalFacturado:N2}", bg);
                                    CeldaDerecha(table, $"${r.TotalServicios:N2}", bg);
                                    CeldaDerecha(table, $"${r.TotalRepuestos:N2}", bg);
                                }
                            });
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  REPORTE: PRODUCTIVIDAD DEL TALLER
        // ═══════════════════════════════════════════════════════
        public byte[] GenerarReporteProductividadPdf(int totalIngresos, decimal totalFacturado, double promedioIngresosMes, decimal promedioFacturacionMes, double tasaFinalizacion, dynamic mesesData)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurarPagina(page);

                    page.Header().Element(c => EncabezadoReporte(c,
                        "Reporte de Productividad del Taller",
                        $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

                    page.Content().Element(content =>
                    {
                        content.PaddingVertical(10).Column(col =>
                        {
                            col.Item().Element(c => FilaKpis(c, new[]
                            {
                                ("Ingresos", totalIngresos.ToString(), ColorSecundario),
                                ("Facturado", $"${totalFacturado:N2}", ColorExito),
                                ("Prom./Mes", $"{promedioIngresosMes:F1}", ColorAdvertencia),
                                ("Tasa Final.", $"{tasaFinalizacion}%", ColorExito)
                            }));

                            col.Item().PaddingTop(15).Element(c => TituloSeccion(c, "Detalle Mensual"));

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);     // Mes
                                    columns.RelativeColumn(1);     // Ingresos
                                    columns.RelativeColumn(1);     // Finalizados
                                    columns.RelativeColumn(1);     // Facturas
                                    columns.RelativeColumn(1.5f);  // Facturado
                                    columns.RelativeColumn(1.5f);  // Ticket Prom.
                                });

                                HeaderTabla(table, "Mes", "Ingresos", "Finaliz.", "Facturas", "Facturado", "Ticket Prom.");

                                int fila = 0;
                                foreach (var mes in mesesData)
                                {
                                    var bg = fila++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                    Celda(table, (string)mes.Mes, bg);
                                    CeldaDerecha(table, mes.Ingresos.ToString(), bg);
                                    CeldaDerecha(table, mes.Finalizados.ToString(), bg);
                                    CeldaDerecha(table, mes.CantFacturas.ToString(), bg);
                                    CeldaDerecha(table, $"${mes.Facturado:N2}", bg);
                                    CeldaDerecha(table, $"${mes.TicketPromedio:N2}", bg);
                                }
                            });
                        });
                    });

                    page.Footer().Element(PiePagina);
                });
            }).GeneratePdf();
        }

        // ═══════════════════════════════════════════════════════
        //  COMPONENTES REUTILIZABLES DEL PDF
        // ═══════════════════════════════════════════════════════

        private void ConfigurarPagina(PageDescriptor page)
        {
            page.Size(PageSizes.A4.Landscape());
            page.MarginHorizontal(30);
            page.MarginVertical(25);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));
        }

        private void EncabezadoReporte(IContainer container, string titulo, string subtitulo)
        {
            container.Column(col =>
            {
                col.Item().BorderBottom(2).BorderColor(ColorPrimario).PaddingBottom(8).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("AutoSys").Bold().FontSize(18).FontColor(ColorPrimario);
                        c.Item().Text("Sistema de Gestión para Talleres Mecánicos").FontSize(8).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(titulo).Bold().FontSize(13).FontColor(ColorPrimario);
                        c.Item().AlignRight().Text(subtitulo).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }

        private void TituloSeccion(IContainer container, string titulo)
        {
            container.PaddingBottom(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Text(titulo).Bold().FontSize(11).FontColor(ColorPrimario);
        }

        private void FilaKpis(IContainer container, (string titulo, string valor, string color)[] kpis)
        {
            container.Row(row =>
            {
                foreach (var kpi in kpis)
                {
                    row.RelativeItem().Padding(4).Border(1).BorderColor(Colors.Grey.Lighten2)
                        .Background(Colors.Grey.Lighten5).Padding(8).Column(col =>
                        {
                            col.Item().AlignCenter().Text(kpi.titulo).FontSize(8).FontColor(Colors.Grey.Darken1);
                            col.Item().AlignCenter().Text(kpi.valor).Bold().FontSize(14).FontColor(kpi.color);
                        });
                }
            });
        }

        /// Genera celdas de encabezado en el TableDescriptor
        private static void HeaderTabla(TableDescriptor table, params string[] columnas)
        {
            foreach (var col in columnas)
            {
                table.Cell().Background(ColorPrimario).Padding(6)
                    .Text(col).FontColor(Colors.White).Bold().FontSize(8);
            }
        }

        private static void Celda(TableDescriptor table, string texto, string bgColor)
        {
            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                .Padding(5).Text(texto ?? "-").FontSize(8);
        }

        private static void CeldaDerecha(TableDescriptor table, string texto, string bgColor)
        {
            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3)
                .Padding(5).AlignRight().Text(texto ?? "-").FontSize(8);
        }

        private void PiePagina(IContainer container)
        {
            container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.Span("AutoSys - ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Medium);
                });

                row.RelativeItem().AlignRight().Text(text =>
                {
                    text.Span("Página ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                    text.Span(" de ").FontSize(7).FontColor(Colors.Grey.Medium);
                    text.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                });
            });
        }
    }
}
