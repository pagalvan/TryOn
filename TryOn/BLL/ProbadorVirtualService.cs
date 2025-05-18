using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class ProbadorVirtualService
    {
        private readonly PruebaPrendaRepository _pruebaPrendaRepository;
        private readonly ClienteRepository _clienteRepository;
        private readonly PrendaRepository _prendaRepository;
        private readonly MedidaRepository _medidaRepository;

        public ProbadorVirtualService()
        {
            _pruebaPrendaRepository = new PruebaPrendaRepository();
            _clienteRepository = new ClienteRepository();
            _prendaRepository = new PrendaRepository();
            _medidaRepository = new MedidaRepository();
        }

        public string IniciarSesionProbador(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    return "El ID del cliente debe ser mayor que cero";

                var cliente = _clienteRepository.BuscarPorId(clienteId);
                if (cliente == null)
                    return $"No se encontró un cliente con ID {clienteId}";

                // Verificar si el cliente tiene medidas registradas
                var medidas = _medidaRepository.ConsultarPorCliente(clienteId);
                if (medidas.Count == 0)
                    return "El cliente no tiene medidas registradas. Se recomienda registrar medidas para una mejor experiencia.";

                return "Sesión iniciada correctamente en el probador virtual";
            }
            catch (Exception ex)
            {
                return $"Error al iniciar sesión en el probador virtual: {ex.Message}";
            }
        }

        public string ProbarPrenda(int clienteId, int prendaId, string comentarios = null)
        {
            try
            {
                if (clienteId <= 0)
                    return "El ID del cliente debe ser mayor que cero";

                if (prendaId <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                var cliente = _clienteRepository.BuscarPorId(clienteId);
                if (cliente == null)
                    return $"No se encontró un cliente con ID {clienteId}";

                var prenda = _prendaRepository.BuscarPorId(prendaId);
                if (prenda == null)
                    return $"No se encontró una prenda con ID {prendaId}";

                // Crear una nueva prueba de prenda
                var prueba = new PruebaPrenda
                {
                    Cliente = cliente,
                    Prenda = prenda,
                    Fecha = DateTime.Now,
                    Comentarios = comentarios
                };

                string resultado = _pruebaPrendaRepository.Guardar(prueba);
                if (resultado.StartsWith("Prueba guardada"))
                    return "Prenda probada correctamente en el probador virtual";
                else
                    return resultado;
            }
            catch (Exception ex)
            {
                return $"Error al probar la prenda: {ex.Message}";
            }
        }

        public List<PruebaPrenda> ObtenerHistorialPruebas(int clienteId)
        {
            try
            {
                if (clienteId <= 0)
                    throw new ArgumentException("El ID del cliente debe ser mayor que cero");

                var cliente = _clienteRepository.BuscarPorId(clienteId);
                if (cliente == null)
                    throw new Exception($"No se encontró un cliente con ID {clienteId}");

                return _pruebaPrendaRepository.ConsultarPorCliente(clienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial de pruebas: {ex.Message}");
            }
        }

        public string VerificarCompatibilidadTalla(int clienteId, int prendaId)
        {
            try
            {
                if (clienteId <= 0)
                    return "El ID del cliente debe ser mayor que cero";

                if (prendaId <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                var cliente = _clienteRepository.BuscarPorId(clienteId);
                if (cliente == null)
                    return $"No se encontró un cliente con ID {clienteId}";

                var prenda = _prendaRepository.BuscarPorId(prendaId);
                if (prenda == null)
                    return $"No se encontró una prenda con ID {prendaId}";

                // Obtener medidas del cliente
                var medidas = _medidaRepository.ConsultarPorCliente(clienteId);
                if (medidas.Count == 0)
                    return "El cliente no tiene medidas registradas para verificar compatibilidad";

                // Lógica simplificada para verificar compatibilidad
                // En un sistema real, esto sería más complejo y dependería de las medidas específicas
                // y de la tabla de tallas de cada tipo de prenda
                return $"La prenda {prenda.Nombre} en talla {prenda.Talla} parece ser compatible con tus medidas";
            }
            catch (Exception ex)
            {
                return $"Error al verificar compatibilidad de talla: {ex.Message}";
            }
        }
    }
}