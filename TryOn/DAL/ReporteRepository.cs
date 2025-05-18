using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class ReporteRepository : BaseDatos, IRepository<Reporte>
    {
        public string Guardar(Reporte reporte)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO reportes (nombre, descripcion, fecha_inicio, fecha_fin, tipo, formato, datos_json, creado_por, fecha_creacion)
                        VALUES (@nombre, @descripcion, @fechaInicio, @fechaFin, @tipo, @formato, @datosJSON, @creadoPor, CURRENT_TIMESTAMP)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = reporte.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = reporte.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = reporte.FechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = reporte.FechaFin;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = reporte.Tipo;
                    comando.Parameters.Add("@formato", NpgsqlDbType.Varchar).Value = reporte.Formato;
                    comando.Parameters.Add("@datosJSON", NpgsqlDbType.Text).Value = reporte.DatosJSON ?? (object)DBNull.Value;
                    comando.Parameters.Add("@creadoPor", NpgsqlDbType.Integer).Value = reporte.CreadoPor.Id;

                    reporte.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Reporte guardado con ID: {reporte.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el reporte: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Reporte> Consultar()
        {
            List<Reporte> reportes = new List<Reporte>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT r.id, r.nombre, r.descripcion, r.fecha_inicio, r.fecha_fin, r.tipo, r.formato, 
                               r.datos_json, r.creado_por, r.fecha_creacion,
                               a.id, u.id, p.id, p.nombre, p.apellido, a.cargo
                        FROM reportes r
                        JOIN administradores a ON r.creado_por = a.id
                        JOIN usuarios u ON a.usuario_id = u.id
                        JOIN personas p ON u.persona_id = p.id
                        ORDER BY r.fecha_creacion DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Administrador admin = new Administrador
                            {
                                Id = reader.GetInt32(10),
                                Nombre = reader.GetString(13),
                                Apellido = reader.GetString(14),
                                Cargo = reader.GetString(15)
                            };

                            Reporte reporte = new Reporte
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                FechaInicio = reader.GetDateTime(3),
                                FechaFin = reader.GetDateTime(4),
                                Tipo = reader.GetString(5),
                                Formato = reader.GetString(6),
                                DatosJSON = reader.IsDBNull(7) ? null : reader.GetString(7),
                                CreadoPor = admin,
                                FechaCreacion = reader.GetDateTime(9)
                            };
                            reportes.Add(reporte);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar reportes: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return reportes;
        }

        public string Modificar(Reporte reporte)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE reportes 
                        SET nombre = @nombre, descripcion = @descripcion, fecha_inicio = @fechaInicio, 
                            fecha_fin = @fechaFin, tipo = @tipo, formato = @formato, datos_json = @datosJSON
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = reporte.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = reporte.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = reporte.FechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = reporte.FechaFin;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = reporte.Tipo;
                    comando.Parameters.Add("@formato", NpgsqlDbType.Varchar).Value = reporte.Formato;
                    comando.Parameters.Add("@datosJSON", NpgsqlDbType.Text).Value = reporte.DatosJSON ?? (object)DBNull.Value;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = reporte.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Reporte modificado correctamente" : "No se encontró el reporte para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el reporte: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
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
                    comando.CommandText = "DELETE FROM reportes WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Reporte eliminado correctamente" : "No se encontró el reporte para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el reporte: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Reporte BuscarPorId(int id)
        {
            Reporte reporte = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT r.id, r.nombre, r.descripcion, r.fecha_inicio, r.fecha_fin, r.tipo, r.formato, 
                               r.datos_json, r.creado_por, r.fecha_creacion,
                               a.id, u.id, p.id, p.nombre, p.apellido, a.cargo
                        FROM reportes r
                        JOIN administradores a ON r.creado_por = a.id
                        JOIN usuarios u ON a.usuario_id = u.id
                        JOIN personas p ON u.persona_id = p.id
                        WHERE r.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Administrador admin = new Administrador
                            {
                                Id = reader.GetInt32(10),
                                Nombre = reader.GetString(13),
                                Apellido = reader.GetString(14),
                                Cargo = reader.GetString(15)
                            };

                            reporte = new Reporte
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                FechaInicio = reader.GetDateTime(3),
                                FechaFin = reader.GetDateTime(4),
                                Tipo = reader.GetString(5),
                                Formato = reader.GetString(6),
                                DatosJSON = reader.IsDBNull(7) ? null : reader.GetString(7),
                                CreadoPor = admin,
                                FechaCreacion = reader.GetDateTime(9)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar reporte por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return reporte;
        }

        public List<Reporte> ConsultarPorTipo(string tipo)
        {
            List<Reporte> reportes = new List<Reporte>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT r.id, r.nombre, r.descripcion, r.fecha_inicio, r.fecha_fin, r.tipo, r.formato, 
                               r.datos_json, r.creado_por, r.fecha_creacion,
                               a.id, u.id, p.id, p.nombre, p.apellido, a.cargo
                        FROM reportes r
                        JOIN administradores a ON r.creado_por = a.id
                        JOIN usuarios u ON a.usuario_id = u.id
                        JOIN personas p ON u.persona_id = p.id
                        WHERE r.tipo = @tipo
                        ORDER BY r.fecha_creacion DESC";
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = tipo;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Administrador admin = new Administrador
                            {
                                Id = reader.GetInt32(10),
                                Nombre = reader.GetString(13),
                                Apellido = reader.GetString(14),
                                Cargo = reader.GetString(15)
                            };

                            Reporte reporte = new Reporte
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                FechaInicio = reader.GetDateTime(3),
                                FechaFin = reader.GetDateTime(4),
                                Tipo = reader.GetString(5),
                                Formato = reader.GetString(6),
                                DatosJSON = reader.IsDBNull(7) ? null : reader.GetString(7),
                                CreadoPor = admin,
                                FechaCreacion = reader.GetDateTime(9)
                            };
                            reportes.Add(reporte);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar reportes por tipo: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return reportes;
        }

        public string GenerarReporteVentasPorCategoria(DateTime fechaInicio, DateTime fechaFin, int administradorId)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Obtener datos para el reporte
                    comando.CommandText = @"
                        SELECT c.nombre as categoria, SUM(dp.subtotal) as total_ventas, COUNT(dp.id) as cantidad_items
                        FROM detalles_pedido dp
                        JOIN prendas p ON dp.prenda_id = p.id
                        JOIN categorias c ON p.categoria_id = c.id
                        JOIN pedidos ped ON dp.pedido_id = ped.id
                        JOIN ventas v ON v.pedido_id = ped.id
                        WHERE v.fecha_venta BETWEEN @fechaInicio AND @fechaFin
                        GROUP BY c.nombre
                        ORDER BY total_ventas DESC";
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = fechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = fechaFin;

                    string datosJSON = "[";
                    bool primero = true;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!primero) datosJSON += ",";
                            primero = false;

                            string categoria = reader.GetString(0);
                            double totalVentas = reader.GetDouble(1);
                            int cantidadItems = reader.GetInt32(2);

                            datosJSON += $"{{\"categoria\":\"{categoria}\",\"totalVentas\":{totalVentas},\"cantidadItems\":{cantidadItems}}}";
                        }
                    }

                    datosJSON += "]";

                    // Guardar el reporte
                    comando.Parameters.Clear();
                    comando.CommandText = @"
                        INSERT INTO reportes (nombre, descripcion, fecha_inicio, fecha_fin, tipo, formato, datos_json, creado_por, fecha_creacion)
                        VALUES (@nombre, @descripcion, @fechaInicio, @fechaFin, @tipo, @formato, @datosJSON, @creadoPor, CURRENT_TIMESTAMP)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = $"Ventas por Categoría {fechaInicio.ToString("dd/MM/yyyy")} - {fechaFin.ToString("dd/MM/yyyy")}";
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = $"Reporte de ventas por categoría desde {fechaInicio.ToString("dd/MM/yyyy")} hasta {fechaFin.ToString("dd/MM/yyyy")}";
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = fechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = fechaFin;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = "VentasPorCategoria";
                    comando.Parameters.Add("@formato", NpgsqlDbType.Varchar).Value = "JSON";
                    comando.Parameters.Add("@datosJSON", NpgsqlDbType.Text).Value = datosJSON;
                    comando.Parameters.Add("@creadoPor", NpgsqlDbType.Integer).Value = administradorId;

                    int reporteId = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Reporte generado con ID: {reporteId}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al generar reporte de ventas por categoría: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public string GenerarReportePrendasMasVendidas(DateTime fechaInicio, DateTime fechaFin, int administradorId, int limit = 10)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Obtener datos para el reporte
                    comando.CommandText = @"
                        SELECT p.nombre, p.tipo, p.color, SUM(dp.cantidad) as cantidad_vendida, SUM(dp.subtotal) as total_ventas
                        FROM detalles_pedido dp
                        JOIN prendas p ON dp.prenda_id = p.id
                        JOIN pedidos ped ON dp.pedido_id = ped.id
                        JOIN ventas v ON v.pedido_id = ped.id
                        WHERE v.fecha_venta BETWEEN @fechaInicio AND @fechaFin
                        GROUP BY p.id, p.nombre, p.tipo, p.color
                        ORDER BY cantidad_vendida DESC
                        LIMIT @limit";
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = fechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = fechaFin;
                    comando.Parameters.Add("@limit", NpgsqlDbType.Integer).Value = limit;

                    string datosJSON = "[";
                    bool primero = true;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (!primero) datosJSON += ",";
                            primero = false;

                            string nombre = reader.GetString(0);
                            string tipo = reader.GetString(1);
                            string color = reader.GetString(2);
                            int cantidadVendida = reader.GetInt32(3);
                            double totalVentas = reader.GetDouble(4);

                            datosJSON += $"{{\"nombre\":\"{nombre}\",\"tipo\":\"{tipo}\",\"color\":\"{color}\",\"cantidadVendida\":{cantidadVendida},\"totalVentas\":{totalVentas}}}";
                        }
                    }

                    datosJSON += "]";

                    // Guardar el reporte
                    comando.Parameters.Clear();
                    comando.CommandText = @"
                        INSERT INTO reportes (nombre, descripcion, fecha_inicio, fecha_fin, tipo, formato, datos_json, creado_por, fecha_creacion)
                        VALUES (@nombre, @descripcion, @fechaInicio, @fechaFin, @tipo, @formato, @datosJSON, @creadoPor, CURRENT_TIMESTAMP)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = $"Top {limit} Prendas Más Vendidas {fechaInicio.ToString("dd/MM/yyyy")} - {fechaFin.ToString("dd/MM/yyyy")}";
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = $"Reporte de las {limit} prendas más vendidas desde {fechaInicio.ToString("dd/MM/yyyy")} hasta {fechaFin.ToString("dd/MM/yyyy")}";
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = fechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = fechaFin;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = "PrendasMasVendidas";
                    comando.Parameters.Add("@formato", NpgsqlDbType.Varchar).Value = "JSON";
                    comando.Parameters.Add("@datosJSON", NpgsqlDbType.Text).Value = datosJSON;
                    comando.Parameters.Add("@creadoPor", NpgsqlDbType.Integer).Value = administradorId;

                    int reporteId = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Reporte generado con ID: {reporteId}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al generar reporte de prendas más vendidas: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}