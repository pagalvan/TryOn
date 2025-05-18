using DAL;
using Entities;
using System;
using System.Collections.Generic;

namespace BLL
{
    public class PromocionService : IService<Promocion>
    {
        private readonly PromocionRepository _repository;
        private readonly CategoriaRepository _categoriaRepository;
        private readonly PrendaRepository _prendaRepository;

        public PromocionService()
        {
            _repository = new PromocionRepository();
            _categoriaRepository = new CategoriaRepository();
            _prendaRepository = new PrendaRepository();
        }

        public string Guardar(Promocion promocion)
        {
            try
            {
                if (promocion == null)
                    return "La promoción no puede ser nula";

                if (string.IsNullOrEmpty(promocion.Nombre))
                    return "El nombre de la promoción no puede estar vacío";

                if (promocion.FechaInicio >= promocion.FechaFin)
                    return "La fecha de inicio debe ser anterior a la fecha de fin";

                if (promocion.PorcentajeDescuento < 0 || promocion.PorcentajeDescuento > 100)
                    return "El porcentaje de descuento debe estar entre 0 y 100";

                if (promocion.MontoDescuento.HasValue && promocion.MontoDescuento.Value < 0)
                    return "El monto de descuento no puede ser negativo";

                // Validar categorías
                if (promocion.CategoriasAplicables != null)
                {
                    foreach (var categoria in promocion.CategoriasAplicables)
                    {
                        if (categoria.Id <= 0)
                            return $"La categoría {categoria.Nombre} no es válida";

                        var categoriaExistente = _categoriaRepository.BuscarPorId(categoria.Id);
                        if (categoriaExistente == null)
                            return $"No se encontró una categoría con ID {categoria.Id}";
                    }
                }

                // Validar prendas
                if (promocion.PrendasAplicables != null)
                {
                    foreach (var prenda in promocion.PrendasAplicables)
                    {
                        if (prenda.Id <= 0)
                            return $"La prenda {prenda.Nombre} no es válida";

                        var prendaExistente = _prendaRepository.BuscarPorId(prenda.Id);
                        if (prendaExistente == null)
                            return $"No se encontró una prenda con ID {prenda.Id}";
                    }
                }

                return _repository.Guardar(promocion);
            }
            catch (Exception ex)
            {
                return $"Error al guardar la promoción: {ex.Message}";
            }
        }

        public List<Promocion> Consultar()
        {
            try
            {
                return _repository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar promociones: {ex.Message}");
            }
        }

        public Promocion BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("El ID de la promoción debe ser mayor que cero");

                return _repository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar promoción: {ex.Message}");
            }
        }

        public string Modificar(Promocion promocion)
        {
            try
            {
                if (promocion == null)
                    return "La promoción no puede ser nula";

                if (promocion.Id <= 0)
                    return "El ID de la promoción debe ser mayor que cero";

                if (string.IsNullOrEmpty(promocion.Nombre))
                    return "El nombre de la promoción no puede estar vacío";

                if (promocion.FechaInicio >= promocion.FechaFin)
                    return "La fecha de inicio debe ser anterior a la fecha de fin";

                if (promocion.PorcentajeDescuento < 0 || promocion.PorcentajeDescuento > 100)
                    return "El porcentaje de descuento debe estar entre 0 y 100";

                if (promocion.MontoDescuento.HasValue && promocion.MontoDescuento.Value < 0)
                    return "El monto de descuento no puede ser negativo";

                var promocionExistente = _repository.BuscarPorId(promocion.Id);
                if (promocionExistente == null)
                    return $"No se encontró una promoción con ID {promocion.Id}";

                // Validar categorías
                if (promocion.CategoriasAplicables != null)
                {
                    foreach (var categoria in promocion.CategoriasAplicables)
                    {
                        if (categoria.Id <= 0)
                            return $"La categoría {categoria.Nombre} no es válida";

                        var categoriaExistente = _categoriaRepository.BuscarPorId(categoria.Id);
                        if (categoriaExistente == null)
                            return $"No se encontró una categoría con ID {categoria.Id}";
                    }
                }

                // Validar prendas
                if (promocion.PrendasAplicables != null)
                {
                    foreach (var prenda in promocion.PrendasAplicables)
                    {
                        if (prenda.Id <= 0)
                            return $"La prenda {prenda.Nombre} no es válida";

                        var prendaExistente = _prendaRepository.BuscarPorId(prenda.Id);
                        if (prendaExistente == null)
                            return $"No se encontró una prenda con ID {prenda.Id}";
                    }
                }

                return _repository.Modificar(promocion);
            }
            catch (Exception ex)
            {
                return $"Error al modificar la promoción: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                    return "El ID de la promoción debe ser mayor que cero";

                var promocionExistente = _repository.BuscarPorId(id);
                if (promocionExistente == null)
                    return $"No se encontró una promoción con ID {id}";

                return _repository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la promoción: {ex.Message}";
            }
        }

        public Promocion BuscarPorCodigoPromo(string codigoPromo)
        {
            try
            {
                if (string.IsNullOrEmpty(codigoPromo))
                    throw new ArgumentException("El código de promoción no puede estar vacío");

                return _repository.BuscarPorCodigoPromo(codigoPromo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar promoción por código: {ex.Message}");
            }
        }

        public List<Promocion> ConsultarPromocionesActivas()
        {
            try
            {
                return _repository.ConsultarPromocionesActivas();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar promociones activas: {ex.Message}");
            }
        }

        public double CalcularDescuento(Promocion promocion, double montoOriginal)
        {
            if (promocion == null)
                return 0;

            if (promocion.MontoDescuento.HasValue)
                return Math.Min(promocion.MontoDescuento.Value, montoOriginal);

            return montoOriginal * (promocion.PorcentajeDescuento / 100);
        }
    }
}