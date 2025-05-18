using System;
using System.Collections.Generic;
using System.Linq;
using DAL;
using Entities;

namespace BLL
{
    public class InventarioService : IService<Inventario>
    {
        private readonly InventarioRepository _inventarioRepository;
        private readonly PrendaRepository _prendaRepository;

        public InventarioService()
        {
            _inventarioRepository = new InventarioRepository();
            _prendaRepository = new PrendaRepository();
        }

        public string Guardar(Inventario inventario)
        {
            try
            {
                // Validaciones
                if (inventario == null)
                {
                    return "El inventario no puede ser nulo";
                }

                if (inventario.Prendas == null || inventario.Prendas.Count == 0)
                {
                    return "El inventario debe contener al menos una prenda";
                }

                // Verificar que todas las prendas existan
                foreach (var prenda in inventario.Prendas)
                {
                    if (prenda.Id <= 0)
                    {
                        return $"La prenda {prenda.Nombre} no está registrada en el sistema";
                    }

                    var prendaExistente = _prendaRepository.BuscarPorId(prenda.Id);
                    if (prendaExistente == null)
                    {
                        return $"La prenda con ID {prenda.Id} no existe en el sistema";
                    }
                }

                // Guardar el inventario
                return _inventarioRepository.Guardar(inventario);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el inventario: {ex.Message}";
            }
        }

        public List<Inventario> Consultar()
        {
            try
            {
                return _inventarioRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar inventarios: {ex.Message}");
            }
        }

        public Inventario BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del inventario debe ser mayor que cero");
                }

                return _inventarioRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar inventario: {ex.Message}");
            }
        }

        public string Modificar(Inventario inventario)
        {
            try
            {
                // Validaciones
                if (inventario == null)
                {
                    return "El inventario no puede ser nulo";
                }

                if (inventario.Id <= 0)
                {
                    return "El ID del inventario debe ser mayor que cero";
                }

                if (inventario.Prendas == null || inventario.Prendas.Count == 0)
                {
                    return "El inventario debe contener al menos una prenda";
                }

                // Verificar que el inventario exista
                var inventarioExistente = _inventarioRepository.BuscarPorId(inventario.Id);
                if (inventarioExistente == null)
                {
                    return $"No se encontró un inventario con ID {inventario.Id}";
                }

                // Verificar que todas las prendas existan
                foreach (var prenda in inventario.Prendas)
                {
                    if (prenda.Id <= 0)
                    {
                        return $"La prenda {prenda.Nombre} no está registrada en el sistema";
                    }

                    var prendaExistente = _prendaRepository.BuscarPorId(prenda.Id);
                    if (prendaExistente == null)
                    {
                        return $"La prenda con ID {prenda.Id} no existe en el sistema";
                    }
                }

                // Modificar el inventario
                return _inventarioRepository.Modificar(inventario);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el inventario: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID del inventario debe ser mayor que cero";
                }

                // Verificar que el inventario exista
                var inventarioExistente = _inventarioRepository.BuscarPorId(id);
                if (inventarioExistente == null)
                {
                    return $"No se encontró un inventario con ID {id}";
                }

                // Eliminar el inventario
                return _inventarioRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el inventario: {ex.Message}";
            }
        }

        public string ActualizarCantidadPrenda(int inventarioId, int prendaId, int cantidad)
        {
            try
            {
                if (inventarioId <= 0)
                {
                    return "El ID del inventario debe ser mayor que cero";
                }

                if (prendaId <= 0)
                {
                    return "El ID de la prenda debe ser mayor que cero";
                }

                // Verificar que el inventario exista
                var inventarioExistente = _inventarioRepository.BuscarPorId(inventarioId);
                if (inventarioExistente == null)
                {
                    return $"No se encontró un inventario con ID {inventarioId}";
                }

                // Verificar que la prenda exista
                var prendaExistente = _prendaRepository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                {
                    return $"No se encontró una prenda con ID {prendaId}";
                }

                // Actualizar la cantidad
                return _inventarioRepository.ActualizarCantidadPrenda(inventarioId, prendaId, cantidad);
            }
            catch (Exception ex)
            {
                return $"Error al actualizar cantidad de prenda: {ex.Message}";
            }
        }

        public string EliminarPrendaDeInventario(int inventarioId, int prendaId)
        {
            try
            {
                if (inventarioId <= 0)
                {
                    return "El ID del inventario debe ser mayor que cero";
                }

                if (prendaId <= 0)
                {
                    return "El ID de la prenda debe ser mayor que cero";
                }

                // Verificar que el inventario exista
                var inventarioExistente = _inventarioRepository.BuscarPorId(inventarioId);
                if (inventarioExistente == null)
                {
                    return $"No se encontró un inventario con ID {inventarioId}";
                }

                // Verificar que la prenda exista en el inventario
                bool prendaEnInventario = inventarioExistente.Prendas.Any(p => p.Id == prendaId);
                if (!prendaEnInventario)
                {
                    return $"La prenda con ID {prendaId} no está en el inventario";
                }

                // Eliminar la prenda del inventario
                return _inventarioRepository.EliminarPrendaDeInventario(inventarioId, prendaId);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar prenda del inventario: {ex.Message}";
            }
        }

        public int ObtenerCantidadPrenda(int inventarioId, int prendaId)
        {
            try
            {
                if (inventarioId <= 0)
                {
                    throw new ArgumentException("El ID del inventario debe ser mayor que cero");
                }

                if (prendaId <= 0)
                {
                    throw new ArgumentException("El ID de la prenda debe ser mayor que cero");
                }

                // Verificar que el inventario exista
                var inventarioExistente = _inventarioRepository.BuscarPorId(inventarioId);
                if (inventarioExistente == null)
                {
                    throw new Exception($"No se encontró un inventario con ID {inventarioId}");
                }

                // Verificar que la prenda exista
                var prendaExistente = _prendaRepository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                {
                    throw new Exception($"No se encontró una prenda con ID {prendaId}");
                }

                // Obtener la cantidad
                return _inventarioRepository.ObtenerCantidadPrenda(inventarioId, prendaId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cantidad de prenda: {ex.Message}");
            }
        }

        public bool VerificarDisponibilidadPrenda(int prendaId, int cantidadRequerida)
        {
            try
            {
                if (prendaId <= 0)
                {
                    throw new ArgumentException("El ID de la prenda debe ser mayor que cero");
                }

                if (cantidadRequerida <= 0)
                {
                    throw new ArgumentException("La cantidad requerida debe ser mayor que cero");
                }

                // Verificar que la prenda exista
                var prendaExistente = _prendaRepository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                {
                    throw new Exception($"No se encontró una prenda con ID {prendaId}");
                }

                // Obtener el inventario más reciente
                var inventarios = _inventarioRepository.Consultar();
                if (inventarios.Count == 0)
                {
                    throw new Exception("No hay inventarios registrados");
                }

                var inventarioReciente = inventarios.OrderByDescending(i => i.FechaActualizacion).First();

                // Verificar si la prenda está en el inventario
                var prendaEnInventario = inventarioReciente.Prendas.FirstOrDefault(p => p.Id == prendaId);
                if (prendaEnInventario == null)
                {
                    throw new Exception($"La prenda con ID {prendaId} no está en el inventario");
                }

                // Verificar si hay suficiente cantidad
                return prendaEnInventario.Cantidad >= cantidadRequerida;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar disponibilidad de prenda: {ex.Message}");
            }
        }
    }
}