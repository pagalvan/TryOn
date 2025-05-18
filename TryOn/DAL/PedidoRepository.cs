using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class PedidoRepository : BaseDatos, IRepository<Pedido>
    {
        private readonly DetallePedidoRepository _detallePedidoRepository;

        public PedidoRepository()
        {
            _detallePedidoRepository = new DetallePedidoRepository();
        }

        public string Guardar(Pedido pedido)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO pedidos (cliente_id, fecha, estado)
                        VALUES (@clienteId, @fecha, @estado)
                        RETURNING id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = pedido.Cliente.Id;
                    comando.Parameters.Add("@fecha", NpgsqlDbType.Timestamp).Value = pedido.Fecha;
                    comando.Parameters.Add("@estado", NpgsqlDbType.Varchar).Value = pedido.Estado;

                    pedido.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Guardar detalles del pedido
                    if (pedido.Detalles != null && pedido.Detalles.Count > 0)
                    {
                        foreach (var detalle in pedido.Detalles)
                        {
                            detalle.Pedido = new Pedido { Id = pedido.Id };
                            _detallePedidoRepository.Guardar(detalle);
                        }
                    }

                    return $"Pedido guardado con ID: {pedido.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Pedido> Consultar()
        {
            List<Pedido> pedidos = new List<Pedido>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.fecha, p.estado,
                               c.id, c.nombre, c.apellido, c.direccion
                        FROM pedidos p
                        JOIN clientes c ON p.cliente_id = c.id
                        ORDER BY p.fecha DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(5),
                                Apellido = reader.GetString(6),
                                Direccion = reader.IsDBNull(7) ? null : reader.GetString(7)
                            };

                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32(0),
                                Cliente = cliente,
                                Fecha = reader.GetDateTime(2),
                                Estado = reader.GetString(3),
                                Detalles = new List<DetallePedido>()
                            };
                            pedidos.Add(pedido);
                        }
                    }

                    // Cargar detalles para cada pedido
                    foreach (var pedido in pedidos)
                    {
                        pedido.Detalles = _detallePedidoRepository.ConsultarPorPedido(pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return pedidos;
        }

        public string Modificar(Pedido pedido)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE pedidos 
                        SET cliente_id = @clienteId, fecha = @fecha, estado = @estado
                        WHERE id = @id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = pedido.Cliente.Id;
                    comando.Parameters.Add("@fecha", NpgsqlDbType.Timestamp).Value = pedido.Fecha;
                    comando.Parameters.Add("@estado", NpgsqlDbType.Varchar).Value = pedido.Estado;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = pedido.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Eliminar detalles existentes
                        EliminarDetallesPedido(pedido.Id);

                        // Guardar nuevos detalles
                        if (pedido.Detalles != null && pedido.Detalles.Count > 0)
                        {
                            foreach (var detalle in pedido.Detalles)
                            {
                                detalle.Pedido = new Pedido { Id = pedido.Id };
                                _detallePedidoRepository.Guardar(detalle);
                            }
                        }

                        return "Pedido modificado correctamente";
                    }
                    else
                    {
                        return "No se encontró el pedido para modificar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarDetallesPedido(int pedidoId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM detalles_pedido WHERE pedido_id = @pedidoId";
                comando.Parameters.Add("@pedidoId", NpgsqlDbType.Integer).Value = pedidoId;
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

                    // Verificar si hay ventas asociadas a este pedido
                    comando.CommandText = "SELECT COUNT(*) FROM ventas WHERE pedido_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadVentas = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadVentas > 0)
                    {
                        return $"No se puede eliminar el pedido porque tiene {cantidadVentas} ventas asociadas";
                    }

                    // Eliminar detalles del pedido
                    EliminarDetallesPedido(id);

                    // Eliminar el pedido
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM pedidos WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Pedido eliminado correctamente" : "No se encontró el pedido para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Pedido BuscarPorId(int id)
        {
            Pedido pedido = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.fecha, p.estado,
                               c.id, c.nombre, c.apellido, c.direccion
                        FROM pedidos p
                        JOIN clientes c ON p.cliente_id = c.id
                        WHERE p.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(5),
                                Apellido = reader.GetString(6),
                                Direccion = reader.IsDBNull(7) ? null : reader.GetString(7)
                            };

                            pedido = new Pedido
                            {
                                Id = reader.GetInt32(0),
                                Cliente = cliente,
                                Fecha = reader.GetDateTime(2),
                                Estado = reader.GetString(3),
                                Detalles = new List<DetallePedido>()
                            };
                        }
                    }

                    // Cargar detalles si encontramos el pedido
                    if (pedido != null)
                    {
                        pedido.Detalles = _detallePedidoRepository.ConsultarPorPedido(pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar pedido por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return pedido;
        }

        public List<Pedido> ConsultarPorCliente(int clienteId)
        {
            List<Pedido> pedidos = new List<Pedido>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.fecha, p.estado
                        FROM pedidos p
                        WHERE p.cliente_id = @clienteId
                        ORDER BY p.fecha DESC";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Fecha = reader.GetDateTime(2),
                                Estado = reader.GetString(3),
                                Detalles = new List<DetallePedido>()
                            };
                            pedidos.Add(pedido);
                        }
                    }

                    // Cargar detalles para cada pedido
                    foreach (var pedido in pedidos)
                    {
                        pedido.Detalles = _detallePedidoRepository.ConsultarPorPedido(pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos por cliente: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return pedidos;
        }

        public List<Pedido> ConsultarPorEstado(string estado)
        {
            List<Pedido> pedidos = new List<Pedido>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.fecha, p.estado,
                               c.id, c.nombre, c.apellido, c.direccion
                        FROM pedidos p
                        JOIN clientes c ON p.cliente_id = c.id
                        WHERE p.estado = @estado
                        ORDER BY p.fecha DESC";
                    comando.Parameters.Add("@estado", NpgsqlDbType.Varchar).Value = estado;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(5),
                                Apellido = reader.GetString(6),
                                Direccion = reader.IsDBNull(7) ? null : reader.GetString(7)
                            };

                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32(0),
                                Cliente = cliente,
                                Fecha = reader.GetDateTime(2),
                                Estado = reader.GetString(3),
                                Detalles = new List<DetallePedido>()
                            };
                            pedidos.Add(pedido);
                        }
                    }

                    // Cargar detalles para cada pedido
                    foreach (var pedido in pedidos)
                    {
                        pedido.Detalles = _detallePedidoRepository.ConsultarPorPedido(pedido.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar pedidos por estado: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return pedidos;
        }

        public string ActualizarEstado(int pedidoId, string nuevoEstado)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE pedidos SET estado = @estado WHERE id = @id";
                    comando.Parameters.Add("@estado", NpgsqlDbType.Varchar).Value = nuevoEstado;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = pedidoId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Estado del pedido actualizado correctamente" : "No se encontró el pedido para actualizar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al actualizar estado del pedido: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}