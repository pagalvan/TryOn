using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class MedidaService : IService<Medida>
    {
        private readonly MedidaRepository _repository;
        private readonly ClienteRepository _clienteRepository;

        public MedidaService()
        {
            _repository = new MedidaRepository();
            _clienteRepository = new ClienteRepository();
        }

        public string Guardar(Medida medida)
        {
            try
            {
                if (medida == null)
                    return "La medida no puede ser nula";

                if (string.IsNullOrEmpty(medida.Tipo))
                    return "El tipo de medida no puede estar vacío";

                if (string.IsNullOrEmpty(medida.Unidad))
                    return "La unidad de medida no puede estar vacía";

                if (medida.Cliente == null || medida.Cliente.Id <= 0)
                    return "El cliente asociado a la medida no es válido";

                var clienteExistente = _clienteRepository.BuscarPorId(medida.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {medida.Cliente.Id}";

                return _repository.Guardar(medida);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la medida: {ex.Message}";
            }
        }

        public List<Medida> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar medidas: {ex.Message}");
            }
        }

        public Medida BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID de la medida debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar medida: {ex.Message}");
            }
        }

        public string Modificar(Medida medida)
        {
            try
            {
                if (medida == null)
                    return "La medida no puede ser nula";

                if (medida.Id <= 0)
                    return "El ID de la medida debe ser mayor que cero";

                if (string.IsNullOrEmpty(medida.Tipo))
                    return "El tipo de medida no puede estar vacío";

                if (string.IsNullOrEmpty(medida.Unidad))
                    return "La unidad de medida no puede estar vacía";

                if (medida.Cliente == null || medida.Cliente.Id <= 0)
                    return "El cliente asociado a la medida no es válido";

                var medidaExistente = _repository.BuscarPorId(medida.Id);
                if (medidaExistente == null)
                    return $"No se encontró una medida con ID {medida.Id}";

                var clienteExistente = _clienteRepository.BuscarPorId(medida.Cliente.Id);
                if (clienteExistente == null)
                    return $"No se encontró un cliente con ID {medida.Cliente.Id}";

                return _repository.Modificar(medida);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la medida: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID de la medida debe ser mayor que cero";

                var medidaExistente = _repository.BuscarPorId(id);
                if (medidaExistente == null)
                    return $"No se encontró una medida con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la medida: {ex.Message}";
            }
        }

        public List<Medida> ConsultarPorCliente(int clienteId)
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
                throw new Exception($"Error al consultar medidas por cliente: {ex.Message}");
            }
        }
    }
}