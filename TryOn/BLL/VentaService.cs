using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class VentaService : IService<Venta>
    {
        private readonly VentaRepository _ventaRepository;
        private readonly PedidoRepository _pedidoRepository;

        public VentaService()
        {
            _ventaRepository = new VentaRepository();
            _pedidoRepository = new PedidoRepository();
        }

        public string Guardar(Venta venta)
        {
            try
            {
                // Validaciones
                if (venta == null)
                {
                    return "La venta no puede ser nula";
                }

                if (venta.Pedido == null || venta.Pedido.Id <= 0)
                {
                    return "El pedido de la venta no es válido";
                }

                if (string.IsNullOrEmpty(venta.MetodoPago))
                {
                    return "El método de pago no puede estar vacío";
                }

                // Verificar que el pedido exista
                var pedido = _pedidoRepository.BuscarPorId(venta.Pedido.Id);
                if (pedido == null)
                {
                    return $"No se encontró un pedido con ID {venta.Pedido.Id}";
                }

                // Verificar que el pedido no esté cancelado
                if (pedido.Estado == "Cancelado")
                {
                    return "No se puede realizar una venta de un pedido cancelado";
                }

                // Verificar que el pedido no esté ya vendido
                var ventaExistente = _ventaRepository.BuscarPorPedido(venta.Pedido.Id);
                if (ventaExistente != null)
                {
                    return $"El pedido con ID {venta.Pedido.Id} ya tiene una venta asociada";
                }

                // Establecer fecha actual y monto total
                venta.FechaVenta = DateTime.Now;
                venta.MontoTotal = pedido.Total;

                // Actualizar el estado del pedido a "Completado"
                _pedidoRepository.ActualizarEstado(pedido.Id, "Completado");

                // Guardar la venta
                return _ventaRepository.Guardar(venta);
            }
            catch (Exception ex)
            {
                return $"Error al realizar la venta: {ex.Message}";
            }
        }

        public List<Venta> Consultar()
        {
            try
            {
                return _ventaRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar ventas: {ex.Message}");
            }
        }

        public Venta BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID de la venta debe ser mayor que cero");
                }

                return _ventaRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar venta: {ex.Message}");
            }
        }

        public Venta BuscarPorPedido(int pedidoId)
        {
            try
            {
                if (pedidoId <= 0)
                {
                    throw new ArgumentException("El ID del pedido debe ser mayor que cero");
                }

                return _ventaRepository.BuscarPorPedido(pedidoId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar venta por pedido: {ex.Message}");
            }
        }

        public List<Venta> ConsultarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                if (fechaInicio > fechaFin)
                {
                    throw new ArgumentException("La fecha de inicio no puede ser posterior a la fecha de fin");
                }

                return _ventaRepository.ConsultarPorFecha(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar ventas por fecha: {ex.Message}");
            }
        }

        public string Modificar(Venta venta)
        {
            try
            {
                // Validaciones
                if (venta == null)
                {
                    return "La venta no puede ser nula";
                }

                if (venta.Id <= 0)
                {
                    return "El ID de la venta debe ser mayor que cero";
                }

                if (venta.Pedido == null || venta.Pedido.Id <= 0)
                {
                    return "El pedido de la venta no es válido";
                }

                if (string.IsNullOrEmpty(venta.MetodoPago))
                {
                    return "El método de pago no puede estar vacío";
                }

                // Verificar que la venta exista
                var ventaExistente = _ventaRepository.BuscarPorId(venta.Id);
                if (ventaExistente == null)
                {
                    return $"No se encontró una venta con ID {venta.Id}";
                }

                // Modificar la venta
                return _ventaRepository.Modificar(venta);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la venta: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID de la venta debe ser mayor que cero";
                }

                // Verificar que la venta exista
                var venta = _ventaRepository.BuscarPorId(id);
                if (venta == null)
                {
                    return $"No se encontró una venta con ID {id}";
                }

                // Actualizar el estado del pedido a "Pendiente"
                _pedidoRepository.ActualizarEstado(venta.Pedido.Id, "Pendiente");

                // Eliminar la venta
                return _ventaRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al anular la venta: {ex.Message}";
            }
        }
    }
}