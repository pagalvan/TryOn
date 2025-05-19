using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class PreferenciaService : IService<Preferencia>
    {
        private readonly PreferenciaRepository _repository;
        private readonly ClienteRepository _clienteRepository;

        public PreferenciaService()
        {
            _repository = new PreferenciaRepository();
            _clienteRepository = new ClienteRepository();
        }

        public string Guardar(Preferencia preferencia)
        {
            try
            {
                if (preferencia == null)
                    return "La preferencia no puede ser nula";

                if (string.IsNullOrEmpty(preferencia.Categoria))
                    return "La categoría de preferencia no puede estar vacía";

                if (string.IsNullOrEmpty(preferencia.Valor))
                    return "El valor de preferencia no puede estar vacío";

                if (preferencia.Cliente == null || preferencia.Cliente.Id <= 0)
                    return "El cliente asociado a la preferencia no es válido";

                var clienteExistente = _clienteRepository.BuscarPorId(preferencia.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {preferencia.Cliente.Id}";

                return _repository.Guardar(preferencia);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la preferencia: {ex.Message}";
            }
        }

        public List<Preferencia> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar preferencias: {ex.Message}");
            }
        }

        public Preferencia BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID de la preferencia debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar preferencia: {ex.Message}");
            }
        }

        public string Modificar(Preferencia preferencia)
        {
            try
            {
                if (preferencia == null)
                    return "La preferencia no puede ser nula";

                if (preferencia.Id <= 0)
                    return "El ID de la preferencia debe ser mayor que cero";

                if (string.IsNullOrEmpty(preferencia.Categoria))
                    return "La categoría de preferencia no puede estar vacía";

                if (string.IsNullOrEmpty(preferencia.Valor))
                    return "El valor de preferencia no puede estar vacío";

                if (preferencia.Cliente == null || preferencia.Cliente.Id <= 0)
                    return "El cliente asociado a la preferencia no es válido";

                var preferenciaExistente = _repository.BuscarPorId(preferencia.Id);
                if (preferenciaExistente == null)
                    return $"No se encontró una preferencia con ID {preferencia.Id}";

                var clienteExistente = _clienteRepository.BuscarPorId(preferencia.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {preferencia.Cliente.Id}";

                return _repository.Modificar(preferencia);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la preferencia: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID de la preferencia debe ser mayor que cero";

                var preferenciaExistente = _repository.BuscarPorId(id);
                if (preferenciaExistente == null)
                    return $"No se encontró una preferencia con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la preferencia: {ex.Message}";
            }
        }

        public List<Preferencia> ConsultarPorCliente(int clienteId)
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
                throw new Exception($"Error al consultar preferencias por cliente: {ex.Message}");
            }
        }

        public List<Preferencia> ConsultarPorCategoria(string categoria)
        {
            try
            {
                if (string.IsNullOrEmpty(categoria))
                    throw new ArgumentException("La categoría no puede estar vacía");

                return _repository.ConsultarPorCategoria(categoria);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar preferencias por categoría: {ex.Message}");
            }
        }
    }
}