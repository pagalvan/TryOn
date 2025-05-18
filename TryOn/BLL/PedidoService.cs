using DAL;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class PedidoService : IService<Pedido>
    {
        private readonly PedidoRepository _pedidoRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly InventarioRepository _inventarioRepository;
        private readonly DetallePedidoRepository _detallePedidoRepository;

        public PedidoService()
        {
            _pedidoRepository = new PedidoRepository();
            _clienteRepository = new ClienteRepository();
            _inventarioRepository = new InventarioRepository();
            _detallePedidoRepository = new DetallePedidoRepository();
        }

        public string Guardar(Pedido pedido)
        {
            try
            {
                // Validaciones
                var validacion = ValidarPedido(pedido);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(pedido.Cliente.Id);
                if (clienteExistente == null)
                {
                    return $"No se encontró un cliente con ID {pedido.Cliente.Id}";
                }

                // Verificar disponibilidad de prendas en el inventario
                var inventarios = _inventarioRepository.Consultar();
                if (inventarios.Count == 0)
                {
                    return "No hay inventarios registrados";
                }

                var inventarioReciente = inventarios.OrderByDescending(i => i.FechaActualizacion).First();

                foreach (var detalle in pedido.Detalles)
                {
                    var prendaEnInventario = inventarioReciente.Prendas.FirstOrDefault(p => p.Id == detalle.Prenda.Id);
                    if (prendaEnInventario == null)
                    {
                        return $"La prenda con ID {detalle.Prenda.Id} no está en el inventario";
                    }

                    if (prendaEnInventario.Cantidad < detalle.Cantidad)
                    {
                        return $"No hay suficiente cantidad de la prenda {detalle.Prenda.Nombre} (Disponible: {prendaEnInventario.Cantidad}, Requerida: {detalle.Cantidad})";
                    }
                }

                // Establecer fecha actual y estado inicial
                pedido.Fecha = DateTime.Now;
                pedido.Estado = "Pendiente";

                // Guardar el pedido
                string mensaje = _pedidoRepository.Guardar(pedido);

                if (mensaje.StartsWith("Pedido guardado"))
                {
                    // Actualizar el inventario
                    foreach (var detalle in pedido.Detalles)
                    {
                        _inventarioRepository.ActualizarCantidadPrenda(inventarioReciente.Id, detalle.Prenda.Id, -detalle.Cantidad);
                    }

                    return mensaje;
                }
                else
                {
                    return mensaje;
                }
            }
            catch (Exception ex)
            {
                return $"Error al crear el pedido: {ex.Message}";
            }
        }

        public List<Pedido> Consultar()
        {
            try
            {
                return _pedidoRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos: {ex.Message}");
            }
        }

        public Pedido BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del pedido debe ser mayor que cero");
                }

                return _pedidoRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar pedido: {ex.Message}");
            }
        }

        public List<Pedido> ConsultarPorCliente(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                {
                    throw new ArgumentException("El ID del cliente debe ser mayor que cero");
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(clienteId);
                if (clienteExistente == null)
                {
                    throw new Exception($"No se encontró un cliente con ID {clienteId}");
                }

                return _pedidoRepository.ConsultarPorCliente(clienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos por cliente: {ex.Message}");
            }
        }

        public List<Pedido> ConsultarPorEstado(string estado)
        {
            try
            {
                if (string.IsNullOrEmpty(estado))
                {
                    throw new ArgumentException("El estado no puede estar vacío");
                }

                return _pedidoRepository.ConsultarPorEstado(estado);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos por estado: {ex.Message}");
            }
        }

        public string Modificar(Pedido pedido)
        {
            try
            {
                // Validaciones
                var validacion = ValidarPedido(pedido);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                if (pedido.Id <= 0)
                {
                    return "El ID del pedido debe ser mayor que cero";
                }

                // Verificar que el pedido exista
                var pedidoExistente = _pedidoRepository.BuscarPorId(pedido.Id);
                if (pedidoExistente == null)
                {
                    return $"No se encontró un pedido con ID {pedido.Id}";
                }

                // Modificar el pedido
                return _pedidoRepository.Modificar(pedido);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el pedido: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID del pedido debe ser mayor que cero";
                }

                // Verificar que el pedido exista
                var pedidoExistente = _pedidoRepository.BuscarPorId(id);
                if (pedidoExistente == null)
                {
                    return $"No se encontró un pedido con ID {id}";
                }

                // Eliminar el pedido
                return _pedidoRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el pedido: {ex.Message}";
            }
        }

        public string ActualizarEstado(int pedidoId, string nuevoEstado)
        {
            try
            {
                if (pedidoId <= 0)
                {
                    return "El ID del pedido debe ser mayor que cero";
                }

                if (string.IsNullOrEmpty(nuevoEstado))
                {
                    return "El nuevo estado no puede estar vacío";
                }

                // Verificar que el pedido exista
                var pedidoExistente = _pedidoRepository.BuscarPorId(pedidoId);
                if (pedidoExistente == null)
                {
                    return $"No se encontró un pedido con ID {pedidoId}";
                }

                // Actualizar el estado
                return _pedidoRepository.ActualizarEstado(pedidoId, nuevoEstado);
            }
            catch (Exception ex)
            {
                return $"Error al actualizar estado del pedido: {ex.Message}";
            }
        }

        public string CancelarPedido(int pedidoId)
        {
            try
            {
                if (pedidoId <= 0)
                {
                    return "El ID del pedido debe ser mayor que cero";
                }

                // Verificar que el pedido exista
                var pedidoExistente = _pedidoRepository.BuscarPorId(pedidoId);
                if (pedidoExistente == null)
                {
                    return $"No se encontró un pedido con ID {pedidoId}";
                }

                // Verificar que el pedido no esté completado o ya cancelado
                if (pedidoExistente.Estado == "Completado")
                {
                    return "No se puede cancelar un pedido completado";
                }

                if (pedidoExistente.Estado == "Cancelado")
                {
                    return "El pedido ya está cancelado";
                }

                // Actualizar el estado a "Cancelado"
                string mensaje = _pedidoRepository.ActualizarEstado(pedidoId, "Cancelado");

                if (mensaje == "Estado del pedido actualizado correctamente")
                {
                    // Devolver las prendas al inventario
                    var inventarios = _inventarioRepository.Consultar();
                    if (inventarios.Count > 0)
                    {
                        var inventarioReciente = inventarios.OrderByDescending(i => i.FechaActualizacion).First();

                        foreach (var detalle in pedidoExistente.Detalles)
                        {
                            _inventarioRepository.ActualizarCantidadPrenda(inventarioReciente.Id, detalle.Prenda.Id, detalle.Cantidad);
                        }
                    }

                    return "Pedido cancelado correctamente";
                }
                else
                {
                    return mensaje;
                }
            }
            catch (Exception ex)
            {
                return $"Error al cancelar el pedido: {ex.Message}";
            }
        }

        private string ValidarPedido(Pedido pedido)
        {
            if (pedido == null)
            {
                return "El pedido no puede ser nulo";
            }

            if (pedido.Cliente == null || pedido.Cliente.Id <= 0)
            {
                return "El cliente del pedido no es válido";
            }

            if (pedido.Detalles == null || pedido.Detalles.Count == 0)
            {
                return "El pedido debe tener al menos un detalle";
            }

            foreach (var detalle in pedido.Detalles)
            {
                if (detalle.Prenda == null || detalle.Prenda.Id <= 0)
                {
                    return "Una de las prendas del pedido no es válida";
                }

                if (detalle.Cantidad <= 0)
                {
                    return $"La cantidad de la prenda {detalle.Prenda.Nombre} debe ser mayor que cero";
                }
            }

            return null;
        }
    }
}