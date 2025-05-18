using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class PruebaPrendaService : IService<PruebaPrenda>
    {
        private readonly PruebaPrendaRepository _repository;
        private readonly ClienteRepository _clienteRepository;
        private readonly PrendaRepository _prendaRepository;

        public PruebaPrendaService()
        {
            _repository = new PruebaPrendaRepository();
            _clienteRepository = new ClienteRepository();
            _prendaRepository = new PrendaRepository();
        }

        public string Guardar(PruebaPrenda pruebaPrenda)
        {
            try
            {
                if (pruebaPrenda == null)
                    return "La prueba de prenda no puede ser nula";

                if (pruebaPrenda.Cliente == null || pruebaPrenda.Cliente.Id <= 0)
                    return "El cliente de la prueba no es válido";

                if (pruebaPrenda.Prenda == null || pruebaPrenda.Prenda.Id <= 0)
                    return "La prenda de la prueba no es válida";

                var clienteExistente = _clienteRepository.BuscarPorId(pruebaPrenda.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {pruebaPrenda.Cliente.Id}";

                var prendaExistente = _prendaRepository.BuscarPorId(pruebaPrenda.Prenda.Id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {pruebaPrenda.Prenda.Id}";

                pruebaPrenda.Fecha = DateTime.Now;

                return _repository.Guardar(pruebaPrenda);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la prueba de prenda: {ex.Message}";
            }
        }

        public List<PruebaPrenda> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pruebas de prendas: {ex.Message}");
            }
        }

        public PruebaPrenda BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID de la prueba debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar prueba de prenda: {ex.Message}");
            }
        }

        public string Modificar(PruebaPrenda pruebaPrenda)
        {
            try
            {
                if (pruebaPrenda == null)
                    return "La prueba de prenda no puede ser nula";

                if (pruebaPrenda.Id <= 0)
                    return "El ID de la prueba debe ser mayor que cero";

                if (pruebaPrenda.Cliente == null || pruebaPrenda.Cliente.Id <= 0)
                    return "El cliente de la prueba no es válido";

                if (pruebaPrenda.Prenda == null || pruebaPrenda.Prenda.Id <= 0)
                    return "La prenda de la prueba no es válida";

                var pruebaExistente = _repository.BuscarPorId(pruebaPrenda.Id);
                if (pruebaExistente == null)
                    return $"No se encontró una prueba con ID {pruebaPrenda.Id}";

                var clienteExistente = _clienteRepository.BuscarPorId(pruebaPrenda.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {pruebaPrenda.Cliente.Id}";

                var prendaExistente = _prendaRepository.BuscarPorId(pruebaPrenda.Prenda.Id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {pruebaPrenda.Prenda.Id}";

                return _repository.Modificar(pruebaPrenda);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la prueba de prenda: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID de la prueba debe ser mayor que cero";

                var pruebaExistente = _repository.BuscarPorId(id);
                if (pruebaExistente == null)
                    return $"No se encontró una prueba con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la prueba de prenda: {ex.Message}";
            }
        }

        public List<PruebaPrenda> ConsultarPorCliente(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    throw new ArgumentException("El ID del cliente debe ser mayor que cero");

                var clienteExistente = _clienteRepository.BuscarPorId(clienteId);
                if (clienteExistente == null)
                    throw new Exception($"No se encontró un cliente con ID {clienteId}");

                return _repository.ConsultarPorCliente(clienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pruebas por cliente: {ex.Message}");
            }
        }

        public List<PruebaPrenda> ConsultarPorPrenda(int prendaId)
        {
            try
            {
                if (prendaId <= 0)
                    throw new ArgumentException("El ID de la prenda debe ser mayor que cero");

                var prendaExistente = _prendaRepository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                    throw new Exception($"No se encontró una prenda con ID {prendaId}");

                return _repository.ConsultarPorPrenda(prendaId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pruebas por prenda: {ex.Message}");
            }
        }
    }
}