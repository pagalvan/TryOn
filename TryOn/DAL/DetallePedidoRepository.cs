using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class DetallePedidoRepository : BaseDatos, IRepository<DetallePedido>
    {
        public string Guardar(DetallePedido detallePedido)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO detalles_pedido (pedido_id, prenda_id, cantidad)
                        VALUES (@pedidoId, @prendaId, @cantidad)
                        RETURNING id";

                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = detallePedido.Pedido.Id;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = detallePedido.Prenda.Id;
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = detallePedido.Cantidad;

                    detallePedido.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Detalle de pedido guardado con ID: {detallePedido.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el detalle de pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<DetallePedido> Consultar()
        {
            List<DetallePedido> detallesPedido = new List<DetallePedido>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT dp.id, dp.pedido_id, dp.prenda_id, dp.cantidad,
                               ped.id, ped.cliente_id, ped.fecha, ped.estado,
                               p.id, p.nombre, p.tipo, p.talla, p.color, p.precio
                        FROM detalles_pedido dp
                        JOIN pedidos ped ON dp.pedido_id = ped.id
                        JOIN prendas p ON dp.prenda_id = p.id
                        ORDER BY dp.id";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32(4),
                                Fecha = reader.GetDateTime(6),
                                Estado = reader.GetString(7)
                            };

                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(8),
                                Nombre = reader.GetString(9),
                                Tipo = reader.GetString(10),
                                Talla = reader.GetString(11),
                                Color = reader.GetString(12),
                                Precio = reader.GetDouble(13)
                            };

                            DetallePedido detallePedido = new DetallePedido
                            {
                                Id = reader.GetInt32(0),
                                Pedido = pedido,
                                Prenda = prenda,
                                Cantidad = reader.GetInt32(3)
                            };
                            detallesPedido.Add(detallePedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar detalles de pedido: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return detallesPedido;
        }

        public string Modificar(DetallePedido detallePedido)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE detalles_pedido 
                        SET pedido_id = @pedidoId, prenda_id = @prendaId, cantidad = @cantidad
                        WHERE id = @id";

                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = detallePedido.Pedido.Id;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = detallePedido.Prenda.Id;
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = detallePedido.Cantidad;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = detallePedido.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Detalle de pedido modificado correctamente" : "No se encontró el detalle de pedido para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el detalle de pedido: {ex.Message}";
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
                    comando.CommandText = "DELETE FROM detalles_pedido WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Detalle de pedido eliminado correctamente" : "No se encontró el detalle de pedido para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el detalle de pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public DetallePedido BuscarPorId(int id)
        {
            DetallePedido detallePedido = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT dp.id, dp.pedido_id, dp.prenda_id, dp.cantidad,
                               ped.id, ped.cliente_id, ped.fecha, ped.estado,
                               p.id, p.nombre, p.tipo, p.talla, p.color, p.precio
                        FROM detalles_pedido dp
                        JOIN pedidos ped ON dp.pedido_id = ped.id
                        JOIN prendas p ON dp.prenda_id = p.id
                        WHERE dp.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32(4),
                                Fecha = reader.GetDateTime(6),
                                Estado = reader.GetString(7)
                            };

                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(8),
                                Nombre = reader.GetString(9),
                                Tipo = reader.GetString(10),
                                Talla = reader.GetString(11),
                                Color = reader.GetString(12),
                                Precio = reader.GetDouble(13)
                            };

                            detallePedido = new DetallePedido
                            {
                                Id = reader.GetInt32(0),
                                Pedido = pedido,
                                Prenda = prenda,
                                Cantidad = reader.GetInt32(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar detalle de pedido por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return detallePedido;
        }

        public List<DetallePedido> ConsultarPorPedido(int pedidoId)
        {
            List<DetallePedido> detallesPedido = new List<DetallePedido>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT dp.id, dp.pedido_id, dp.prenda_id, dp.cantidad,
                               p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.imagen
                        FROM detalles_pedido dp
                        JOIN prendas p ON dp.prenda_id = p.id
                        WHERE dp.pedido_id = @pedidoId
                        ORDER BY dp.id";
                    comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = pedidoId;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(5),
                                Tipo = reader.GetString(6),
                                Talla = reader.GetString(7),
                                Color = reader.GetString(8),
                                Precio = reader.GetDouble(9),
                                Imagen = reader.IsDBNull(10) ? null : reader.GetString(10)
                            };

                            DetallePedido detallePedido = new DetallePedido
                            {
                                Id = reader.GetInt32(0),
                                Pedido = new Pedido { Id = reader.GetInt32(1) },
                                Prenda = prenda,
                                Cantidad = reader.GetInt32(3)
                            };
                            detallesPedido.Add(detallePedido);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar detalles por pedido: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return detallesPedido;
        }
    }
}
