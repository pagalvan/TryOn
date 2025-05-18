using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class CarritoRepository : BaseDatos, IRepository<CarritoCompra>
    {
        public string Guardar(CarritoCompra carrito)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "INSERT INTO carritos (cliente_id, fecha_creacion, activo) VALUES (@clienteId, CURRENT_TIMESTAMP, @activo) RETURNING id";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = carrito.Cliente.Id;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = carrito.Activo;

                    carrito.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Guardar items del carrito
                    if (carrito.Items != null && carrito.Items.Count > 0)
                    {
                        foreach (var item in carrito.Items)
                        {
                            GuardarItemCarrito(carrito.Id, item);
                        }
                    }

                    return $"Carrito guardado con ID: {carrito.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private string GuardarItemCarrito(int carritoId, ItemCarrito item)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = "INSERT INTO items_carrito (carrito_id, prenda_id, cantidad) VALUES (@carritoId, @prendaId, @cantidad) RETURNING id";
                    comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = item.Prenda.Id;
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = item.Cantidad;

                    item.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Item guardado con ID: {item.Id}";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al guardar item del carrito: {ex.Message}");
            }
        }

        public List<CarritoCompra> Consultar()
        {
            List<CarritoCompra> carritos = new List<CarritoCompra>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT c.id, c.cliente_id, c.fecha_creacion, c.activo,
                               cl.id, p.id, p.nombre, p.apellido, p.telefono, cl.direccion
                        FROM carritos c
                        JOIN clientes cl ON c.cliente_id = cl.id
                        JOIN personas p ON cl.persona_id = p.id
                        ORDER BY c.fecha_creacion DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(6),
                                Apellido = reader.GetString(7),
                                Telefono = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Direccion = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            CarritoCompra carrito = new CarritoCompra
                            {
                                Id = reader.GetInt32(0),
                                Cliente = cliente,
                                FechaCreacion = reader.GetDateTime(2),
                                Activo = reader.GetBoolean(3),
                                Items = new List<ItemCarrito>()
                            };

                            carritos.Add(carrito);
                        }
                    }

                    // Cargar items para cada carrito
                    foreach (var carrito in carritos)
                    {
                        carrito.Items = ConsultarItemsCarrito(carrito.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar carritos: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return carritos;
        }

        private List<ItemCarrito> ConsultarItemsCarrito(int carritoId)
        {
            List<ItemCarrito> items = new List<ItemCarrito>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT i.id, i.carrito_id, i.prenda_id, i.cantidad, i.fecha_agregado,
                               p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.precio_descuento, 
                               p.stock, p.imagen, p.modelo_3d, p.descripcion
                        FROM items_carrito i
                        JOIN prendas p ON i.prenda_id = p.id
                        WHERE i.carrito_id = @carritoId";
                    comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(5),
                                Nombre = reader.GetString(6),
                                Tipo = reader.GetString(7),
                                Talla = reader.GetString(8),
                                Color = reader.GetString(9),
                                Precio = reader.GetDouble(10),
                                PrecioDescuento = reader.IsDBNull(11) ? (double?)null : reader.GetDouble(11),
                                Stock = reader.GetInt32(12),
                                Imagen = reader.IsDBNull(13) ? null : reader.GetString(13),
                                Modelo3D = reader.IsDBNull(14) ? null : reader.GetString(14),
                                Descripcion = reader.IsDBNull(15) ? null : reader.GetString(15)
                            };

                            ItemCarrito item = new ItemCarrito
                            {
                                Id = reader.GetInt32(0),
                                Prenda = prenda,
                                Cantidad = reader.GetInt32(3)
                            };

                            items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar items del carrito: {ex.Message}");
            }
            return items;
        }

        public string Modificar(CarritoCompra carrito)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE carritos SET cliente_id = @clienteId, activo = @activo WHERE id = @id";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = carrito.Cliente.Id;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = carrito.Activo;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = carrito.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Eliminar items existentes
                        EliminarItemsCarrito(carrito.Id);

                        // Guardar nuevos items
                        if (carrito.Items != null && carrito.Items.Count > 0)
                        {
                            foreach (var item in carrito.Items)
                            {
                                GuardarItemCarrito(carrito.Id, item);
                            }
                        }

                        return "Carrito modificado correctamente";
                    }
                    else
                    {
                        return "No se encontró el carrito para modificar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarItemsCarrito(int carritoId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM items_carrito WHERE carrito_id = @carritoId";
                comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;
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

                    // Eliminar items del carrito
                    EliminarItemsCarrito(id);

                    // Eliminar el carrito
                    comando.CommandText = "DELETE FROM carritos WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Carrito eliminado correctamente" : "No se encontró el carrito para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public CarritoCompra BuscarPorId(int id)
        {
            CarritoCompra carrito = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT c.id, c.cliente_id, c.fecha_creacion, c.activo,
                               cl.id, p.id, p.nombre, p.apellido, p.telefono, cl.direccion
                        FROM carritos c
                        JOIN clientes cl ON c.cliente_id = cl.id
                        JOIN personas p ON cl.persona_id = p.id
                        WHERE c.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(4),
                                Nombre = reader.GetString(6),
                                Apellido = reader.GetString(7),
                                Telefono = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Direccion = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            carrito = new CarritoCompra
                            {
                                Id = reader.GetInt32(0),
                                Cliente = cliente,
                                FechaCreacion = reader.GetDateTime(2),
                                Activo = reader.GetBoolean(3),
                                Items = new List<ItemCarrito>()
                            };
                        }
                    }

                    // Cargar items si encontramos el carrito
                    if (carrito != null)
                    {
                        carrito.Items = ConsultarItemsCarrito(carrito.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar carrito por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return carrito;
        }

        public CarritoCompra BuscarCarritoActivoCliente(int clienteId)
        {
            CarritoCompra carrito = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT id FROM carritos WHERE cliente_id = @clienteId AND activo = true";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                    object result = comando.ExecuteScalar();

                    if (result != null)
                    {
                        int carritoId = Convert.ToInt32(result);
                        carrito = BuscarPorId(carritoId);
                    }
                    else
                    {
                        // Si no existe un carrito activo, creamos uno nuevo
                        comando.Parameters.Clear();
                        comando.CommandText = "INSERT INTO carritos (cliente_id, fecha_creacion, activo) VALUES (@clienteId, CURRENT_TIMESTAMP, true) RETURNING id";
                        comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                        int nuevoCarritoId = Convert.ToInt32(comando.ExecuteScalar());

                        // Obtenemos el cliente
                        Cliente cliente = ObtenerCliente(clienteId);

                        carrito = new CarritoCompra
                        {
                            Id = nuevoCarritoId,
                            Cliente = cliente,
                            FechaCreacion = DateTime.Now,
                            Activo = true,
                            Items = new List<ItemCarrito>()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar carrito activo del cliente: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return carrito;
        }

        private Cliente ObtenerCliente(int clienteId)
        {
            Cliente cliente = null;
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    SELECT cl.id, p.id, p.nombre, p.apellido, p.telefono, cl.direccion
                    FROM clientes cl
                    JOIN personas p ON cl.persona_id = p.id
                    WHERE cl.id = @clienteId";
                comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                using (var reader = comando.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            Id = reader.GetInt32(0),
                            Nombre = reader.GetString(2),
                            Apellido = reader.GetString(3),
                            Telefono = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Direccion = reader.IsDBNull(5) ? null : reader.GetString(5)
                        };
                    }
                }
            }
            return cliente;
        }

        public string AgregarItemCarrito(int carritoId, int prendaId, int cantidad)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Verificar si el item ya existe en el carrito
                    comando.CommandText = "SELECT id, cantidad FROM items_carrito WHERE carrito_id = @carritoId AND prenda_id = @prendaId";
                    comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // El item ya existe, actualizamos la cantidad
                            int itemId = reader.GetInt32(0);
                            int cantidadActual = reader.GetInt32(1);
                            reader.Close();

                            comando.Parameters.Clear();
                            comando.CommandText = "UPDATE items_carrito SET cantidad = @cantidad WHERE id = @id";
                            comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidadActual + cantidad;
                            comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = itemId;
                            comando.ExecuteNonQuery();

                            return "Item actualizado en el carrito";
                        }
                    }

                    // El item no existe, lo agregamos
                    comando.Parameters.Clear();
                    comando.CommandText = "INSERT INTO items_carrito (carrito_id, prenda_id, cantidad, fecha_agregado) VALUES (@carritoId, @prendaId, @cantidad, CURRENT_TIMESTAMP)";
                    comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidad;
                    comando.ExecuteNonQuery();

                    return "Item agregado al carrito";
                }
            }
            catch (Exception ex)
            {
                return $"Error al agregar item al carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public string EliminarItemCarrito(int itemId)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "DELETE FROM items_carrito WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = itemId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Item eliminado del carrito" : "No se encontró el item para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar item del carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public string ActualizarCantidadItem(int itemId, int cantidad)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE items_carrito SET cantidad = @cantidad WHERE id = @id";
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidad;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = itemId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Cantidad actualizada" : "No se encontró el item para actualizar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al actualizar cantidad: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public string VaciarCarrito(int carritoId)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "DELETE FROM items_carrito WHERE carrito_id = @carritoId";
                    comando.Parameters.Add("@carritoId", NpgsqlDbType.Integer).Value = carritoId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return "Carrito vaciado correctamente";
                }
            }
            catch (Exception ex)
            {
                return $"Error al vaciar el carrito: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}