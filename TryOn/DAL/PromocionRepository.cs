using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class PromocionRepository : BaseDatos, IRepository<Promocion>
    {
        public string Guardar(Promocion promocion)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO promociones (nombre, descripcion, codigo_promo, porcentaje_descuento, 
                                               monto_descuento, fecha_inicio, fecha_fin, activo)
                        VALUES (@nombre, @descripcion, @codigoPromo, @porcentajeDescuento, 
                                @montoDescuento, @fechaInicio, @fechaFin, @activo)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = promocion.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = promocion.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@codigoPromo", NpgsqlDbType.Varchar).Value = promocion.CodigoPromo ?? (object)DBNull.Value;
                    comando.Parameters.Add("@porcentajeDescuento", NpgsqlDbType.Double).Value = promocion.PorcentajeDescuento;
                    comando.Parameters.Add("@montoDescuento", NpgsqlDbType.Double).Value = promocion.MontoDescuento.HasValue ? (object)promocion.MontoDescuento.Value : DBNull.Value;
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = promocion.FechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = promocion.FechaFin;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = promocion.Activo;

                    promocion.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Guardar relaciones con categorías
                    if (promocion.CategoriasAplicables != null && promocion.CategoriasAplicables.Count > 0)
                    {
                        foreach (var categoria in promocion.CategoriasAplicables)
                        {
                            AsignarCategoriaAPromocion(promocion.Id, categoria.Id);
                        }
                    }

                    // Guardar relaciones con prendas
                    if (promocion.PrendasAplicables != null && promocion.PrendasAplicables.Count > 0)
                    {
                        foreach (var prenda in promocion.PrendasAplicables)
                        {
                            AsignarPrendaAPromocion(promocion.Id, prenda.Id);
                        }
                    }

                    return $"Promoción guardada con ID: {promocion.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la promoción: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void AsignarCategoriaAPromocion(int promocionId, int categoriaId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "INSERT INTO promociones_categorias (promocion_id, categoria_id) VALUES (@promocionId, @categoriaId) ON CONFLICT DO NOTHING";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;
                comando.Parameters.Add("@categoriaId", NpgsqlDbType.Integer).Value = categoriaId;
                comando.ExecuteNonQuery();
            }
        }

        private void AsignarPrendaAPromocion(int promocionId, int prendaId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "INSERT INTO promociones_prendas (promocion_id, prenda_id) VALUES (@promocionId, @prendaId) ON CONFLICT DO NOTHING";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;
                comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;
                comando.ExecuteNonQuery();
            }
        }

        public List<Promocion> Consultar()
        {
            List<Promocion> promociones = new List<Promocion>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, descripcion, codigo_promo, porcentaje_descuento, 
                               monto_descuento, fecha_inicio, fecha_fin, activo
                        FROM promociones
                        ORDER BY fecha_inicio DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Promocion promocion = new Promocion
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CodigoPromo = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PorcentajeDescuento = reader.GetDouble(4),
                                MontoDescuento = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                FechaInicio = reader.GetDateTime(6),
                                FechaFin = reader.GetDateTime(7),
                                Activo = reader.GetBoolean(8),
                                CategoriasAplicables = new List<Categoria>(),
                                PrendasAplicables = new List<Prenda>()
                            };
                            promociones.Add(promocion);
                        }
                    }

                    // Cargar categorías y prendas para cada promoción
                    foreach (var promocion in promociones)
                    {
                        promocion.CategoriasAplicables = ConsultarCategoriasDePromocion(promocion.Id);
                        promocion.PrendasAplicables = ConsultarPrendasDePromocion(promocion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar promociones: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return promociones;
        }

        private List<Categoria> ConsultarCategoriasDePromocion(int promocionId)
        {
            List<Categoria> categorias = new List<Categoria>();
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    SELECT c.id, c.nombre, c.descripcion
                    FROM categorias c
                    JOIN promociones_categorias pc ON c.id = pc.categoria_id
                    WHERE pc.promocion_id = @promocionId";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;

                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Categoria categoria = new Categoria
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };
                        categorias.Add(categoria);
                    }
                }
            }
            return categorias;
        }

        private List<Prenda> ConsultarPrendasDePromocion(int promocionId)
        {
            List<Prenda> prendas = new List<Prenda>();
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    SELECT p.id, p.nombre, p.tipo, p.talla, p.color, p.precio
                    FROM prendas p
                    JOIN promociones_prendas pp ON p.id = pp.prenda_id
                    WHERE pp.promocion_id = @promocionId";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;

                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Prenda prenda = new Prenda
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(1),
                            Tipo = reader.GetString(2),
                            Talla = reader.GetString(3),
                            Color = reader.GetString(4),
                            Precio = reader.GetDouble(5)
                        };
                        prendas.Add(prenda);
                    }
                }
            }
            return prendas;
        }

        public string Modificar(Promocion promocion)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE promociones 
                        SET nombre = @nombre, descripcion = @descripcion, codigo_promo = @codigoPromo, 
                            porcentaje_descuento = @porcentajeDescuento, monto_descuento = @montoDescuento, 
                            fecha_inicio = @fechaInicio, fecha_fin = @fechaFin, activo = @activo
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = promocion.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = promocion.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@codigoPromo", NpgsqlDbType.Varchar).Value = promocion.CodigoPromo ?? (object)DBNull.Value;
                    comando.Parameters.Add("@porcentajeDescuento", NpgsqlDbType.Double).Value = promocion.PorcentajeDescuento;
                    comando.Parameters.Add("@montoDescuento", NpgsqlDbType.Double).Value = promocion.MontoDescuento.HasValue ? (object)promocion.MontoDescuento.Value : DBNull.Value;
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = promocion.FechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = promocion.FechaFin;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = promocion.Activo;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = promocion.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Eliminar relaciones existentes
                        EliminarRelacionesPromocion(promocion.Id);

                        // Guardar nuevas relaciones con categorías
                        if (promocion.CategoriasAplicables != null && promocion.CategoriasAplicables.Count > 0)
                        {
                            foreach (var categoria in promocion.CategoriasAplicables)
                            {
                                AsignarCategoriaAPromocion(promocion.Id, categoria.Id);
                            }
                        }

                        // Guardar nuevas relaciones con prendas
                        if (promocion.PrendasAplicables != null && promocion.PrendasAplicables.Count > 0)
                        {
                            foreach (var prenda in promocion.PrendasAplicables)
                            {
                                AsignarPrendaAPromocion(promocion.Id, prenda.Id);
                            }
                        }

                        return "Promoción modificada correctamente";
                    }
                    else
                    {
                        return "No se encontró la promoción para modificar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la promoción: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarRelacionesPromocion(int promocionId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;

                // Eliminar relaciones con categorías
                comando.CommandText = "DELETE FROM promociones_categorias WHERE promocion_id = @promocionId";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;
                comando.ExecuteNonQuery();

                // Eliminar relaciones con prendas
                comando.Parameters.Clear();
                comando.CommandText = "DELETE FROM promociones_prendas WHERE promocion_id = @promocionId";
                comando.Parameters.Add("@promocionId", NpgsqlDbType.Integer).Value = promocionId;
                comando.ExecuteNonQuery();
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Eliminar relaciones
                    EliminarRelacionesPromocion(id);

                    // Eliminar la promoción
                    comando.CommandText = "DELETE FROM promociones WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Promoción eliminada correctamente" : "No se encontró la promoción para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la promoción: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Promocion BuscarPorId(int id)
        {
            Promocion promocion = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, descripcion, codigo_promo, porcentaje_descuento, 
                               monto_descuento, fecha_inicio, fecha_fin, activo
                        FROM promociones
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            promocion = new Promocion
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CodigoPromo = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PorcentajeDescuento = reader.GetDouble(4),
                                MontoDescuento = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                FechaInicio = reader.GetDateTime(6),
                                FechaFin = reader.GetDateTime(7),
                                Activo = reader.GetBoolean(8),
                                CategoriasAplicables = new List<Categoria>(),
                                PrendasAplicables = new List<Prenda>()
                            };
                        }
                    }

                    // Cargar categorías y prendas si encontramos la promoción
                    if (promocion != null)
                    {
                        promocion.CategoriasAplicables = ConsultarCategoriasDePromocion(promocion.Id);
                        promocion.PrendasAplicables = ConsultarPrendasDePromocion(promocion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar promoción por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return promocion;
        }

        public Promocion BuscarPorCodigoPromo(string codigoPromo)
        {
            Promocion promocion = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, descripcion, codigo_promo, porcentaje_descuento, 
                               monto_descuento, fecha_inicio, fecha_fin, activo
                        FROM promociones
                        WHERE codigo_promo = @codigoPromo
                        AND activo = true
                        AND fecha_inicio <= CURRENT_TIMESTAMP
                        AND fecha_fin >= CURRENT_TIMESTAMP";
                    comando.Parameters.Add("@codigoPromo", NpgsqlDbType.Varchar).Value = codigoPromo;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            promocion = new Promocion
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CodigoPromo = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PorcentajeDescuento = reader.GetDouble(4),
                                MontoDescuento = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                FechaInicio = reader.GetDateTime(6),
                                FechaFin = reader.GetDateTime(7),
                                Activo = reader.GetBoolean(8),
                                CategoriasAplicables = new List<Categoria>(),
                                PrendasAplicables = new List<Prenda>()
                            };
                        }
                    }

                    // Cargar categorías y prendas si encontramos la promoción
                    if (promocion != null)
                    {
                        promocion.CategoriasAplicables = ConsultarCategoriasDePromocion(promocion.Id);
                        promocion.PrendasAplicables = ConsultarPrendasDePromocion(promocion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar promoción por código: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return promocion;
        }

        public List<Promocion> ConsultarPromocionesActivas()
        {
            List<Promocion> promociones = new List<Promocion>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, descripcion, codigo_promo, porcentaje_descuento, 
                               monto_descuento, fecha_inicio, fecha_fin, activo
                        FROM promociones
                        WHERE activo = true
                        AND fecha_inicio <= CURRENT_TIMESTAMP
                        AND fecha_fin >= CURRENT_TIMESTAMP
                        ORDER BY fecha_inicio DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Promocion promocion = new Promocion
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CodigoPromo = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PorcentajeDescuento = reader.GetDouble(4),
                                MontoDescuento = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                FechaInicio = reader.GetDateTime(6),
                                FechaFin = reader.GetDateTime(7),
                                Activo = reader.GetBoolean(8),
                                CategoriasAplicables = new List<Categoria>(),
                                PrendasAplicables = new List<Prenda>()
                            };
                            promociones.Add(promocion);
                        }
                    }

                    // Cargar categorías y prendas para cada promoción
                    foreach (var promocion in promociones)
                    {
                        promocion.CategoriasAplicables = ConsultarCategoriasDePromocion(promocion.Id);
                        promocion.PrendasAplicables = ConsultarPrendasDePromocion(promocion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar promociones activas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return promociones;
        }
    }
}