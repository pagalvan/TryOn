using DAL;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class CarritoCompraService : IService<CarritoCompra>
    {
        private readonly CarritoRepository _carritoRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly PrendaRepository _prendaRepository;

        public CarritoCompraService()
        {
            _carritoRepository = new CarritoRepository();
            _clienteRepository = new ClienteRepository();
            _prendaRepository = new PrendaRepository();
        }

        public string Guardar(CarritoCompra carrito)
        {
            try
            {
                if (carrito == null)
                    return "El carrito no puede ser nulo";

                if (carrito.Cliente == null || carrito.Cliente.Id <= 0)
                    return "El cliente del carrito no es válido";

                var clienteExistente = _clienteRepository.BuscarPorId(carrito.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {carrito.Cliente.Id}";

                if (carrito.Items != null)
                {
                    foreach (var item in carrito.Items)
                    {
                        if (item.Prenda == null || item.Prenda.Id <= 0)
                            return "Una de las prendas del carrito no es válida";

                        if (item.Cantidad <= 0)
                            return "La cantidad de una prenda debe ser mayor que cero";

                        var prendaExistente = _prendaRepository.BuscarPorId(item.Prenda.Id);
                        if (prendaExistente == null)
                            return $"No se encontró una prenda con ID {item.Prenda.Id}";
                    }
                }

                return _carritoRepository.Guardar(carrito);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el carrito: {ex.Message}";
            }
        }

        public List<CarritoCompra> Consultar()
        {
            try
            {
                return _carritoRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar carritos: {ex.Message}");
            }
        }

        public CarritoCompra BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID del carrito debe ser mayor que cero");

                return _carritoRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar carrito: {ex.Message}");
            }
        }

        public string Modificar(CarritoCompra carrito)
        {
            try
            {
                if (carrito == null)
                    return "El carrito no puede ser nulo";

                if (carrito.Id <= 0)
                    return "El ID del carrito debe ser mayor que cero";

                if (carrito.Cliente == null || carrito.Cliente.Id <= 0)
                    return "El cliente del carrito no es válido";

                var carritoExistente = _carritoRepository.BuscarPorId(carrito.Id);
                if (carritoExistente == null)
                    return $"No se encontró un carrito con ID {carrito.Id}";

                var clienteExistente = _clienteRepository.BuscarPorId(carrito.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {carrito.Cliente.Id}";

                if (carrito.Items != null)
                {
                    foreach (var item in carrito.Items)
                    {
                        if (item.Prenda == null || item.Prenda.Id <= 0)
                            return "Una de las prendas del carrito no es válida";

                        if (item.Cantidad <= 0)
                            return "La cantidad de una prenda debe ser mayor que cero";

                        var prendaExistente = _prendaRepository.BuscarPorId(item.Prenda.Id);
                        if (prendaExistente == null)
                            return $"No se encontró una prenda con ID {item.Prenda.Id}";
                    }
                }

                return _carritoRepository.Modificar(carrito);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el carrito: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID del carrito debe ser mayor que cero";

                var carritoExistente = _carritoRepository.BuscarPorId(id);
                if (carritoExistente == null)
                    return $"No se encontró un carrito con ID {id}";

                return _carritoRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el carrito: {ex.Message}";
            }
        }

        public CarritoCompra BuscarCarritoActivoCliente(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    throw new ArgumentException("El ID del cliente debe ser mayor que cero");

                var clienteExistente = _clienteRepository.BuscarPorId(clienteId);
                if (clienteExistente == null)
                    throw new Exception($"No se encontró un cliente con ID {clienteId}");

                return _carritoRepository.BuscarCarritoActivoCliente(clienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar carrito activo: {ex.Message}");
            }
        }

        public string AgregarItemCarrito(int carritoId, int prendaId, int cantidad)
        {
            try
            {
                if (carritoId <= 0)
                    return "El ID del carrito debe ser mayor que cero";

                if (prendaId <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                if (cantidad <= 0)
                    return "La cantidad debe ser mayor que cero";

                var carritoExistente = _carritoRepository.BuscarPorId(carritoId);
                if (carritoExistente == null)
                    return $"No se encontró un carrito con ID {carritoId}";

                var prendaExistente = _prendaRepository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {prendaId}";

                return _carritoRepository.AgregarItemCarrito(carritoId, prendaId, cantidad);
            }
            catch (Exception ex)
            {
                return $"Error al agregar item al carrito: {ex.Message}";
            }
        }

        public string EliminarItemCarrito(int itemId)
        {
            try
            {
                if (itemId <= 0)
                    return "El ID del item debe ser mayor que cero";

                return _carritoRepository.EliminarItemCarrito(itemId);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar item del carrito: {ex.Message}";
            }
        }

        public string VaciarCarrito(int carritoId)
        {
            try
            {
                if (carritoId <= 0)
                    return "El ID del carrito debe ser mayor que cero";

                var carritoExistente = _carritoRepository.BuscarPorId(carritoId);
                if (carritoExistente == null)
                    return $"No se encontró un carrito con ID {carritoId}";

                return _carritoRepository.VaciarCarrito(carritoId);
            }
            catch (Exception ex)
            {
                return $"Error al vaciar el carrito: {ex.Message}";
            }
        }
    }
}