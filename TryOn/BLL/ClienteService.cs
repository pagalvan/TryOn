using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class ClienteService : IService<Cliente>
    {
        private readonly ClienteRepository _clienteRepository;
        private readonly MedidaRepository _medidaRepository;
        private readonly PreferenciaRepository _preferenciaRepository;

        public ClienteService()
        {
            _clienteRepository = new ClienteRepository();
            _medidaRepository = new MedidaRepository();
            _preferenciaRepository = new PreferenciaRepository();
        }

        public string Guardar(Cliente cliente)
        {
            try
            {
                // Validaciones
                var validacion = ValidarCliente(cliente);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                // Guardar el cliente
                return _clienteRepository.Guardar(cliente);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el cliente: {ex.Message}";
            }
        }

        public List<Cliente> Consultar()
        {
            try
            {
                return _clienteRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar clientes: {ex.Message}");
            }
        }

        public Cliente BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del cliente debe ser mayor que cero");
                }

                return _clienteRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar cliente: {ex.Message}");
            }
        }

        public string Modificar(Cliente cliente)
        {
            try
            {
                // Validaciones
                var validacion = ValidarCliente(cliente);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                if (cliente.Id <= 0)
                {
                    return "El ID del cliente debe ser mayor que cero";
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(cliente.Id);
                if (clienteExistente == null)
                {
                    return $"No se encontró un cliente con ID {cliente.Id}";
                }

                // Modificar el cliente
                return _clienteRepository.Modificar(cliente);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el cliente: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID del cliente debe ser mayor que cero";
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(id);
                if (clienteExistente == null)
                {
                    return $"No se encontró un cliente con ID {id}";
                }

                // Eliminar el cliente
                return _clienteRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el cliente: {ex.Message}";
            }
        }

        public string AgregarMedidaACliente(int clienteId, Medida medida)
        {
            try
            {
                if (clienteId <= 0)
                {
                    return "El ID del cliente debe ser mayor que cero";
                }

                // Validar la medida
                if (medida == null)
                {
                    return "La medida no puede ser nula";
                }

                if (string.IsNullOrEmpty(medida.Tipo))
                {
                    return "El tipo de medida no puede estar vacío";
                }

                if (string.IsNullOrEmpty(medida.Unidad))
                {
                    return "La unidad de medida no puede estar vacía";
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(clienteId);
                if (clienteExistente == null)
                {
                    return $"No se encontró un cliente con ID {clienteId}";
                }

                // Guardar la medida
                string mensaje = _medidaRepository.Guardar(medida);

                if (mensaje.StartsWith("Medida guardada"))
                {
                    // Asociar la medida al cliente
                    clienteExistente.Medidas.Add(medida);
                    _clienteRepository.Modificar(clienteExistente);

                    return "Medida agregada al cliente correctamente";
                }
                else
                {
                    return mensaje;
                }
            }
            catch (Exception ex)
            {
                return $"Error al agregar medida al cliente: {ex.Message}";
            }
        }

        public string AgregarPreferenciaACliente(int clienteId, Preferencia preferencia)
        {
            try
            {
                if (clienteId <= 0)
                {
                    return "El ID del cliente debe ser mayor que cero";
                }

                // Validar la preferencia
                if (preferencia == null)
                {
                    return "La preferencia no puede ser nula";
                }

                if (string.IsNullOrEmpty(preferencia.Categoria))
                {
                    return "La categoría de preferencia no puede estar vacía";
                }

                if (string.IsNullOrEmpty(preferencia.Valor))
                {
                    return "El valor de preferencia no puede estar vacío";
                }

                // Verificar que el cliente exista
                var clienteExistente = _clienteRepository.BuscarPorId(clienteId);
                if (clienteExistente == null)
                {
                    return $"No se encontró un cliente con ID {clienteId}";
                }

                // Guardar la preferencia
                string mensaje = _preferenciaRepository.Guardar(preferencia);

                if (mensaje.StartsWith("Preferencia guardada"))
                {
                    // Asociar la preferencia al cliente
                    clienteExistente.Preferencias.Add(preferencia);
                    _clienteRepository.Modificar(clienteExistente);

                    return "Preferencia agregada al cliente correctamente";
                }
                else
                {
                    return mensaje;
                }
            }
            catch (Exception ex)
            {
                return $"Error al agregar preferencia al cliente: {ex.Message}";
            }
        }

        private string ValidarCliente(Cliente cliente)
        {
            if (cliente == null)
            {
                return "El cliente no puede ser nulo";
            }

            if (string.IsNullOrEmpty(cliente.Nombre))
            {
                return "El nombre no puede estar vacío";
            }

            if (string.IsNullOrEmpty(cliente.Apellido))
            {
                return "El apellido no puede estar vacío";
            }

            return null;
        }
    }
}