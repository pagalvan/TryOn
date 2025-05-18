using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class VentaRepository : BaseDatos, IRepository<Venta>
    {
        private readonly PedidoRepository _pedidoRepository;

        public VentaRepository()
        {
            _pedidoRepository = new PedidoRepository();
        }

        public string Guardar(Venta venta)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO ventas (pedido_id, fecha_venta, metodo_pago, monto_total)
                        VALUES (@pedidoId, @fechaVenta, @metodoPago, @montoTotal)
                        RETURNING id";

                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = venta.Pedido.Id;
                    comando.Parameters.Add("@fechaVenta", NpgsqlDbType.Timestamp).Value = venta.FechaVenta;
                    comando.Parameters.Add("@metodoPago", NpgsqlDbType.Varchar).Value = venta.MetodoPago;
                    comando.Parameters.Add("@montoTotal", NpgsqlDbType.Double).Value = venta.MontoTotal;

                    venta.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Actualizar estado del pedido a "Completado"
                    _pedidoRepository.ActualizarEstado(venta.Pedido.Id, "Completado");

                    return $"Venta guardada con ID: {venta.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la venta: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Venta> Consultar()
        {
            List<Venta> ventas = new List<Venta>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT v.id, v.pedido_id, v.fecha_venta, v.metodo_pago, v.monto_total
                        FROM ventas v
                        ORDER BY v.fecha_venta DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Venta venta = new Venta
                            {
                                Id = reader.GetInt32(0),
                                Pedido = new Pedido { Id = reader.GetInt32(1) },
                                FechaVenta = reader.GetDateTime(2),
                                MetodoPago = reader.GetString(3),
                                MontoTotal = reader.GetDouble(4)
                            };
                            ventas.Add(venta);
                        }
                    }

                    // Cargar los pedidos completos
                    foreach (var venta in ventas)
                    {
                        venta.Pedido = _pedidoRepository.BuscarPorId(venta.Pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar ventas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return ventas;
        }

        public string Modificar(Venta venta)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE ventas 
                        SET pedido_id = @pedidoId, fecha_venta = @fechaVenta, 
                            metodo_pago = @metodoPago, monto_total = @montoTotal
                        WHERE id = @id";

                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = venta.Pedido.Id;
                    comando.Parameters.Add("@fechaVenta", NpgsqlDbType.Timestamp).Value = venta.FechaVenta;
                    comando.Parameters.Add("@metodoPago", NpgsqlDbType.Varchar).Value = venta.MetodoPago;
                    comando.Parameters.Add("@montoTotal", NpgsqlDbType.Double).Value = venta.MontoTotal;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = venta.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Venta modificada correctamente" : "No se encontró la venta para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la venta: {ex.Message}";
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

                    // Obtener el ID del pedido asociado
                    comando.CommandText = "SELECT pedido_id FROM ventas WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    object result = comando.ExecuteScalar();
                    if (result != null)
                    {
                        int pedidoId = Convert.ToInt32(result);

                        // Eliminar la venta
                        comando.Parameters.Clear();
                        comando.CommandText = "DELETE FROM ventas WHERE id = @id";
                        comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                        int filasAfectadas = comando.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            // Actualizar estado del pedido a "Pendiente"
                            _pedidoRepository.ActualizarEstado(pedidoId, "Pendiente");
                            return "Venta eliminada correctamente";
                        }
                        else
                        {
                            return "No se encontró la venta para eliminar";
                        }
                    }
                    else
                    {
                        return "No se encontró la venta para eliminar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la venta: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Venta BuscarPorId(int id)
        {
            Venta venta = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT v.id, v.pedido_id, v.fecha_venta, v.metodo_pago, v.monto_total
                        FROM ventas v
                        WHERE v.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            venta = new Venta
                            {
                                Id = reader.GetInt32(0),
                                Pedido = new Pedido { Id = reader.GetInt32(1) },
                                FechaVenta = reader.GetDateTime(2),
                                MetodoPago = reader.GetString(3),
                                MontoTotal = reader.GetDouble(4)
                            };
                        }
                    }

                    // Cargar el pedido completo si encontramos la venta
                    if (venta != null)
                    {
                        venta.Pedido = _pedidoRepository.BuscarPorId(venta.Pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar venta por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return venta;
        }

        public Venta BuscarPorPedido(int pedidoId)
        {
            Venta venta = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT v.id, v.pedido_id, v.fecha_venta, v.metodo_pago, v.monto_total
                        FROM ventas v
                        WHERE v.pedido_id = @pedidoId";
                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = pedidoId;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            venta = new Venta
                            {
                                Id = reader.GetInt32(0),
                                Pedido = new Pedido { Id = reader.GetInt32(1) },
                                FechaVenta = reader.GetDateTime(2),
                                MetodoPago = reader.GetString(3),
                                MontoTotal = reader.GetDouble(4)
                            };
                        }
                    }

                    // Cargar el pedido completo si encontramos la venta
                    if (venta != null)
                    {
                        venta.Pedido = _pedidoRepository.BuscarPorId(venta.Pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar venta por pedido: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return venta;
        }

        public List<Venta> ConsultarPorFecha(DateTime fechaInicio, DateTime fechaFin)
        {
            List<Venta> ventas = new List<Venta>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT v.id, v.pedido_id, v.fecha_venta, v.metodo_pago, v.monto_total
                        FROM ventas v
                        WHERE v.fecha_venta BETWEEN @fechaInicio AND @fechaFin
                        ORDER BY v.fecha_venta DESC";
                    comando.Parameters.Add("@fechaInicio", NpgsqlDbType.Timestamp).Value = fechaInicio;
                    comando.Parameters.Add("@fechaFin", NpgsqlDbType.Timestamp).Value = fechaFin;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Venta venta = new Venta
                            {
                                Id = reader.GetInt32(0),
                                Pedido = new Pedido { Id = reader.GetInt32(1) },
                                FechaVenta = reader.GetDateTime(2),
                                MetodoPago = reader.GetString(3),
                                MontoTotal = reader.GetDouble(4)
                            };
                            ventas.Add(venta);
                        }
                    }

                    // Cargar los pedidos completos
                    foreach (var venta in ventas)
                    {
                        venta.Pedido = _pedidoRepository.BuscarPorId(venta.Pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar ventas por fecha: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return ventas;
        }
    }
}