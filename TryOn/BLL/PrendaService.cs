using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class PrendaService : IService<Prenda>
    {
        private readonly PrendaRepository _repository;
        private readonly CategoriaRepository _categoriaRepository;

        public PrendaService()
        {
            _repository = new PrendaRepository();
            _categoriaRepository = new CategoriaRepository();
        }

        public string Guardar(Prenda prenda)
        {
            try
            {
                if (prenda == null)
                    return "La prenda no puede ser nula";

                if (string.IsNullOrEmpty(prenda.Nombre))
                    return "El nombre de la prenda no puede estar vacío";

                if (string.IsNullOrEmpty(prenda.Tipo))
                    return "El tipo de prenda no puede estar vacío";

                if (string.IsNullOrEmpty(prenda.Talla))
                    return "La talla de la prenda no puede estar vacía";

                if (string.IsNullOrEmpty(prenda.Color))
                    return "El color de la prenda no puede estar vacío";

                if (prenda.Precio <= 0)
                    return "El precio de la prenda debe ser mayor que cero";

                if (prenda.Stock < 0)
                    return "El stock de la prenda no puede ser negativo";

                if (prenda.Categoria != null && prenda.Categoria.Id > 0)
                {
                    var categoriaExistente = _categoriaRepository.BuscarPorId(prenda.Categoria.Id);
                    if (categoriaExistente == null)
                        return $"No se encontró una categoría con ID {prenda.Categoria.Id}";
                }

                return _repository.Guardar(prenda);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la prenda: {ex.Message}";
            }
        }

        public List<Prenda> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas: {ex.Message}");
            }
        }

        public Prenda BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID de la prenda debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar prenda: {ex.Message}");
            }
        }

        public string Modificar(Prenda prenda)
        {
            try
            {
                if (prenda == null)
                    return "La prenda no puede ser nula";

                if (prenda.Id <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                if (string.IsNullOrEmpty(prenda.Nombre))
                    return "El nombre de la prenda no puede estar vacío";

                if (string.IsNullOrEmpty(prenda.Tipo))
                    return "El tipo de prenda no puede estar vacío";

                if (string.IsNullOrEmpty(prenda.Talla))
                    return "La talla de la prenda no puede estar vacía";

                if (string.IsNullOrEmpty(prenda.Color))
                    return "El color de la prenda no puede estar vacío";

                if (prenda.Precio <= 0)
                    return "El precio de la prenda debe ser mayor que cero";

                if (prenda.Stock < 0)
                    return "El stock de la prenda no puede ser negativo";

                var prendaExistente = _repository.BuscarPorId(prenda.Id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {prenda.Id}";

                if (prenda.Categoria != null && prenda.Categoria.Id > 0)
                {
                    var categoriaExistente = _categoriaRepository.BuscarPorId(prenda.Categoria.Id);
                    if (categoriaExistente == null)
                        return $"No se encontró una categoría con ID {prenda.Categoria.Id}";
                }

                return _repository.Modificar(prenda);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la prenda: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                var prendaExistente = _repository.BuscarPorId(id);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la prenda: {ex.Message}";
            }
        }

        public List<Prenda> ConsultarPrendasDestacadas()
        {
            try
            {
                return _repository.ConsultarPrendasDestacadas();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas destacadas: {ex.Message}");
            }
        }

        public List<Prenda> BuscarPrendas(string termino)
        {
            try
            {
                if (string.IsNullOrEmpty(termino))
                    throw new ArgumentException("El término de búsqueda no puede estar vacío");

                return _repository.BuscarPrendas(termino);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar prendas: {ex.Message}");
            }
        }

        public string ActualizarStock(int prendaId, int cantidad)
        {
            try
            {
                if (prendaId <= 0)
                    return "El ID de la prenda debe ser mayor que cero";

                var prendaExistente = _repository.BuscarPorId(prendaId);
                if (prendaExistente == null)
                    return $"No se encontró una prenda con ID {prendaId}";

                if (prendaExistente.Stock + cantidad < 0)
                    return "No hay suficiente stock disponible para realizar esta operación";

                return _repository.ActualizarStock(prendaId, cantidad);
            }
            catch (Exception ex)
            {
                return $"Error al actualizar stock: {ex.Message}";
            }
        }
    }
}