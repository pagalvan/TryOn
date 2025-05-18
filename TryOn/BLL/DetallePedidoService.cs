using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class DetallePedidoService : IService<DetallePedido>
    {
        private readonly DetallePedidoRepository _repository;
        private readonly PedidoRepository _pedidoRepository;
        private readonly PrendaRepository _prendaRepository;

        public DetallePedidoService()
        {
            _repository = new DetallePedidoRepository();
            _pedidoRepository = new PedidoRepository();
            _prendaRepository = new PrendaRepository();
        }

        public string Guardar(DetallePedido detallePedido)
        {
            try
            {
                if (detallePedido == null)
                    return "El detalle de pedido no puede ser nulo";

                if (detallePedido.Pedido == null || detallePedido.Pedido.Id <= 0)
                    return "El pedido asociado no es válido";

                if (detallePedido.Prenda == null || detallePedido.Prenda.Id <= 0)
                    return "La prenda asociada no es válida";

                if (detallePedido.Cantidad <= 0)
                    return "La cantidad debe ser mayor que cero";

                var pedidoExistente = _pedidoRepository.BuscarPorId(detallePedido.Pedido.Id);
                if (pedidoExistente == null)
                    return $"No se encontró un pedido con ID {detallePedido.Pedido.Id}";

                var prendaExistente = _prendaRepository.BuscarPorId(detallePedido.Prenda.Id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {detallePedido.Prenda.Id}";

                return _repository.Guardar(detallePedido);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el detalle de pedido: {ex.Message}";
            }
        }

        public List<DetallePedido> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar detalles de pedido: {ex.Message}");
            }
        }

        public DetallePedido BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID del detalle debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar detalle de pedido: {ex.Message}");
            }
        }

        public string Modificar(DetallePedido detallePedido)
        {
            try
            {
                if (detallePedido == null)
                    return "El detalle de pedido no puede ser nulo";

                if (detallePedido.Id <= 0)
                    return "El ID del detalle debe ser mayor que cero";

                if (detallePedido.Pedido == null || detallePedido.Pedido.Id <= 0)
                    return "El pedido asociado no es válido";

                if (detallePedido.Prenda == null || detallePedido.Prenda.Id <= 0)
                    return "La prenda asociada no es válida";

                if (detallePedido.Cantidad <= 0)
                    return "La cantidad debe ser mayor que cero";

                var detalleExistente = _repository.BuscarPorId(detallePedido.Id);
                if (detalleExistente == null)
                    return $"No se encontró un detalle de pedido con ID {detallePedido.Id}";

                var pedidoExistente = _pedidoRepository.BuscarPorId(detallePedido.Pedido.Id);
                if (pedidoExistente == null)
                    return $"No se encontró un pedido con ID {detallePedido.Pedido.Id}";

                var prendaExistente = _prendaRepository.BuscarPorId(detallePedido.Prenda.Id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {detallePedido.Prenda.Id}";

                return _repository.Modificar(detallePedido);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el detalle de pedido: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID del detalle debe ser mayor que cero";

                var detalleExistente = _repository.BuscarPorId(id);
                if (detalleExistente == null)
                    return $"No se encontró un detalle de pedido con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el detalle de pedido: {ex.Message}";
            }
        }

        public List<DetallePedido> ConsultarPorPedido(int pedidoId)
        {
            try
            {
                if (pedidoId <= 0)
                    throw new ArgumentException("El ID del pedido debe ser mayor que cero");

                var pedidoExistente = _pedidoRepository.BuscarPorId(pedidoId);
                if (pedidoExistente == null)
                    throw new Exception($"No se encontró un pedido con ID {pedidoId}");

                return _repository.ConsultarPorPedido(pedidoId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar detalles por pedido: {ex.Message}");
            }
        }
    }
}