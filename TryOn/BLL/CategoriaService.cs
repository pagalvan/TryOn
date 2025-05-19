using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class CategoriaService : IService<Categoria>
    {
        private readonly CategoriaRepository _categoriaRepository;

        public CategoriaService()
        {
            _categoriaRepository = new CategoriaRepository();
        }

        public string Guardar(Categoria categoria)
        {
            try
            {
                // Validaciones
                var validacion = ValidarCategoria(categoria);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                // Guardar la categoría
                return _categoriaRepository.Guardar(categoria);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la categoría: {ex.Message}";
            }
        }

        public List<Categoria> Consultar()
        {
            try
            {
                return _categoriaRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar categorías: {ex.Message}");
            }
        }

        public Categoria BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID de la categoría debe ser mayor que cero");
                }

                return _categoriaRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar categoría: {ex.Message}");
            }
        }

        public string Modificar(Categoria categoria)
        {
            try
            {
                // Validaciones
                var validacion = ValidarCategoria(categoria);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                if (categoria.Id <= 0)
                {
                    return "El ID de la categoría debe ser mayor que cero";
                }

                // Verificar que la categoría exista
                var categoriaExistente = _categoriaRepository.BuscarPorId(categoria.Id);
                if (categoriaExistente == null)
                {
                    return $"No se encontró una categoría con ID {categoria.Id}";
                }

                // Modificar la categoría
                return _categoriaRepository.Modificar(categoria);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la categoría: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID de la categoría debe ser mayor que cero";
                }

                // Verificar que la categoría exista
                var categoriaExistente = _categoriaRepository.BuscarPorId(id);
                if (categoriaExistente == null)
                {
                    return $"No se encontró una categoría con ID {id}";
                }

                // Verificar si hay prendas asociadas a esta categoría
                if (categoriaExistente.Prendas != null && categoriaExistente.Prendas.Count > 0)
                {
                    return $"No se puede eliminar la categoría porque tiene {categoriaExistente.Prendas.Count} prendas asociadas";
                }

                // Eliminar la categoría
                return _categoriaRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la categoría: {ex.Message}";
            }
        }

        public List<Prenda> ConsultarPrendasPorCategoria(int categoriaId)
        {
            try
            {
                if (categoriaId <= 0)
                {
                    throw new ArgumentException("El ID de la categoría debe ser mayor que cero");
                }

                // Verificar que la categoría exista
                var categoriaExistente = _categoriaRepository.BuscarPorId(categoriaId);
                if (categoriaExistente == null)
                {
                    throw new Exception($"No se encontró una categoría con ID {categoriaId}");
                }

                return _categoriaRepository.ConsultarPrendasPorCategoria(categoriaId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas por categoría: {ex.Message}");
            }
        }

        public List<Categoria> ConsultarCategoriasActivas()
        {
            try
            {
                return _categoriaRepository.ConsultarCategoriasActivas();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar categorías activas: {ex.Message}");
            }
        }

        private string ValidarCategoria(Categoria categoria)
        {
            if (categoria == null)
            {
                return "La categoría no puede ser nula";
            }

            if (string.IsNullOrEmpty(categoria.Nombre))
            {
                return "El nombre de la categoría no puede estar vacío";
            }

            if (categoria.Nombre.Length > 100)
            {
                return "El nombre de la categoría no puede exceder los 100 caracteres";
            }

            return null;
        }
    }
}