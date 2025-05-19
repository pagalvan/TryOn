using DAL;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

public class ReporteService
{
    private readonly VentaRepository _ventaRepository;
    private readonly PedidoRepository _pedidoRepository;
    private readonly InventarioRepository _inventarioRepository;
    private readonly PrendaRepository _prendaRepository;
    private readonly ClienteRepository _clienteRepository;

    public ReporteService()
    {
        _ventaRepository = new VentaRepository();
        _pedidoRepository = new PedidoRepository();
        _inventarioRepository = new InventarioRepository();
        _prendaRepository = new PrendaRepository();
        _clienteRepository = new ClienteRepository();
    }

    public class ReportePrendaVendida
    {
        public Prenda Prenda { get; set; }
        public int CantidadVendida { get; set; }
        public double MontoTotal { get; set; }
    }

    public List<ReportePrendaVendida> GenerarReportePrendasMasVendidas(DateTime fechaInicio, DateTime fechaFin, int topN = 10)
    {
        try
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            if (topN <= 0)
            {
                throw new ArgumentException("El número de prendas a mostrar debe ser mayor que cero");
            }

            // Obtener todas las ventas en el rango de fechas
            var ventas = _ventaRepository.ConsultarPorFecha(fechaInicio, fechaFin);

            if (ventas.Count == 0)
            {
                throw new Exception($"No se encontraron ventas entre {fechaInicio.ToShortDateString()} y {fechaFin.ToShortDateString()}");
            }

            // Agrupar por prenda y sumar cantidades
            var prendasVendidas = new Dictionary<int, ReportePrendaVendida>();

            foreach (var venta in ventas)
            {
                foreach (var detalle in venta.Pedido.Detalles)
                {
                    if (prendasVendidas.ContainsKey(detalle.Prenda.Id))
                    {
                        prendasVendidas[detalle.Prenda.Id].CantidadVendida += detalle.Cantidad;
                        prendasVendidas[detalle.Prenda.Id].MontoTotal += detalle.Subtotal;
                    }
                    else
                    {
                        prendasVendidas[detalle.Prenda.Id] = new ReportePrendaVendida
                        {
                            Prenda = detalle.Prenda,
                            CantidadVendida = detalle.Cantidad,
                            MontoTotal = detalle.Subtotal
                        };
                    }
                }
            }

            // Ordenar por cantidad vendida y tomar los top N
            return prendasVendidas.Values
                .OrderByDescending(p => p.CantidadVendida)
                .Take(topN)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte de prendas más vendidas: {ex.Message}");
        }
    }

    public class ReporteVentaPorPeriodo
    {
        public string Periodo { get; set; }
        public int CantidadVentas { get; set; }
        public double MontoTotal { get; set; }
    }

    public List<ReporteVentaPorPeriodo> GenerarReporteVentasPorPeriodo(DateTime fechaInicio, DateTime fechaFin, string agrupacion = "Mes")
    {
        try
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            // Validar tipo de agrupación
            if (agrupacion != "Dia" && agrupacion != "Semana" && agrupacion != "Mes" && agrupacion != "Año")
            {
                throw new ArgumentException("El tipo de agrupación debe ser 'Dia', 'Semana', 'Mes' o 'Año'");
            }

            // Obtener todas las ventas en el rango de fechas
            var ventas = _ventaRepository.ConsultarPorFecha(fechaInicio, fechaFin);

            if (ventas.Count == 0)
            {
                throw new Exception($"No se encontraron ventas entre {fechaInicio.ToShortDateString()} y {fechaFin.ToShortDateString()}");
            }

            // Agrupar por periodo según la agrupación seleccionada
            var ventasPorPeriodo = new Dictionary<string, ReporteVentaPorPeriodo>();

            foreach (var venta in ventas)
            {
                string periodo;

                switch (agrupacion)
                {
                    case "Dia":
                        periodo = venta.FechaVenta.ToString("yyyy-MM-dd");
                        break;
                    case "Semana":
                        // Obtener el primer día de la semana (lunes)
                        DateTime primerDiaSemana = venta.FechaVenta.AddDays(-(int)venta.FechaVenta.DayOfWeek + 1);
                        if (primerDiaSemana.DayOfWeek == DayOfWeek.Sunday) primerDiaSemana = primerDiaSemana.AddDays(-6);
                        periodo = $"Semana {primerDiaSemana.ToString("yyyy-MM-dd")}";
                        break;
                    case "Mes":
                        periodo = venta.FechaVenta.ToString("yyyy-MM");
                        break;
                    case "Año":
                        periodo = venta.FechaVenta.ToString("yyyy");
                        break;
                    default:
                        periodo = venta.FechaVenta.ToString("yyyy-MM");
                        break;
                }

                if (ventasPorPeriodo.ContainsKey(periodo))
                {
                    ventasPorPeriodo[periodo].CantidadVentas++;
                    ventasPorPeriodo[periodo].MontoTotal += venta.MontoTotal;
                }
                else
                {
                    ventasPorPeriodo[periodo] = new ReporteVentaPorPeriodo
                    {
                        Periodo = periodo,
                        CantidadVentas = 1,
                        MontoTotal = venta.MontoTotal
                    };
                }
            }

            // Ordenar por periodo
            return ventasPorPeriodo.Values
                .OrderBy(v => v.Periodo)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte de ventas por periodo: {ex.Message}");
        }
    }

    public class ReporteInventario
    {
        public Prenda Prenda { get; set; }
        public int Cantidad { get; set; }
        public double ValorTotal { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }

    public List<ReporteInventario> GenerarReporteInventarioActual()
    {
        try
        {
            // Obtener el inventario más reciente
            var inventarios = _inventarioRepository.Consultar();

            if (inventarios.Count == 0)
            {
                throw new Exception("No hay inventarios registrados");
            }

            var inventarioReciente = inventarios.OrderByDescending(i => i.FechaActualizacion).First();

            // Crear el reporte
            var reporte = new List<ReporteInventario>();

            foreach (var prenda in inventarioReciente.Prendas)
            {
                reporte.Add(new ReporteInventario
                {
                    Prenda = prenda,
                    Cantidad = prenda.Cantidad,
                    ValorTotal = prenda.Cantidad * prenda.Precio,
                    FechaActualizacion = inventarioReciente.FechaActualizacion
                });
            }

            // Ordenar por cantidad (de menor a mayor)
            return reporte.OrderBy(r => r.Cantidad).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte de inventario: {ex.Message}");
        }
    }

    public class ReporteClientesFrecuentes
    {
        public Cliente Cliente { get; set; }
        public int CantidadPedidos { get; set; }
        public double MontoTotal { get; set; }
    }

    public List<ReporteClientesFrecuentes> GenerarReporteClientesFrecuentes(DateTime fechaInicio, DateTime fechaFin, int topN = 10)
    {
        try
        {
            if (fechaInicio > fechaFin)
            {
                throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
            }

            if (topN <= 0)
            {
                throw new ArgumentException("El número de clientes a mostrar debe ser mayor que cero");
            }

            // Obtener todos los pedidos en el rango de fechas
            var pedidos = _pedidoRepository.Consultar()
                .Where(p => p.Fecha >= fechaInicio && p.Fecha <= fechaFin && p.Estado != "Cancelado")
                .ToList();

            if (pedidos.Count == 0)
            {
                throw new Exception($"No se encontraron pedidos entre {fechaInicio.ToShortDateString()} y {fechaFin.ToShortDateString()}");
            }

            // Agrupar por cliente y contar pedidos
            var clientesFrecuentes = new Dictionary<int, ReporteClientesFrecuentes>();

            foreach (var pedido in pedidos)
            {
                if (clientesFrecuentes.ContainsKey(pedido.Cliente.Id))
                {
                    clientesFrecuentes[pedido.Cliente.Id].CantidadPedidos++;
                    clientesFrecuentes[pedido.Cliente.Id].MontoTotal += pedido.Total;
                }
                else
                {
                    clientesFrecuentes[pedido.Cliente.Id] = new ReporteClientesFrecuentes
                    {
                        Cliente = pedido.Cliente,
                        CantidadPedidos = 1,
                        MontoTotal = pedido.Total
                    };
                }
            }

            // Ordenar por cantidad de pedidos y tomar los top N
            return clientesFrecuentes.Values
                .OrderByDescending(c => c.CantidadPedidos)
                .ThenByDescending(c => c.MontoTotal)
                .Take(topN)
                .ToList();
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte de clientes frecuentes: {ex.Message}");
        }
    }

    public List<Pedido> GenerarReportePedidosPorEstado(string estado)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(estado))
            {
                throw new ArgumentException("El estado del pedido no puede estar vacío");
            }

            var pedidos = _pedidoRepository.Consultar();

            var pedidosFiltrados = pedidos
                .Where(p => p.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (pedidosFiltrados.Count == 0)
            {
                throw new Exception($"No se encontraron pedidos con estado '{estado}'");
            }

            return pedidosFiltrados;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al generar reporte de pedidos por estado: {ex.Message}");
        }
    }
}
