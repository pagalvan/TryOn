using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class PrendaRepository : BaseDatos, IRepository<Prenda>
    {
        public string Guardar(Prenda prenda)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO prendas (nombre, tipo, talla, color, precio, precio_descuento, stock, 
                                            imagen, modelo_3d, descripcion, categoria_id, destacado, activo)
                        VALUES (@nombre, @tipo, @talla, @color, @precio, @precioDescuento, @stock, 
                                @imagen, @modelo3d, @descripcion, @categoriaId, @destacado, @activo)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = prenda.Nombre;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = prenda.Tipo;
                    comando.Parameters.Add("@talla", NpgsqlDbType.Varchar).Value = prenda.Talla;
                    comando.Parameters.Add("@color", NpgsqlDbType.Varchar).Value = prenda.Color;
                    comando.Parameters.Add("@precio", NpgsqlDbType.Double).Value = prenda.Precio;
                    comando.Parameters.Add("@precioDescuento", NpgsqlDbType.Double).Value = prenda.PrecioDescuento.HasValue ? (object)prenda.PrecioDescuento.Value : DBNull.Value;
                    comando.Parameters.Add("@stock", NpgsqlDbType.Integer).Value = prenda.Stock;
                    comando.Parameters.Add("@imagen", NpgsqlDbType.Varchar).Value = prenda.Imagen ?? (object)DBNull.Value;
                    comando.Parameters.Add("@modelo3d", NpgsqlDbType.Varchar).Value = prenda.Modelo3D ?? (object)DBNull.Value;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = prenda.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@categoriaId", NpgsqlDbType.Integer).Value = prenda.Categoria?.Id ?? (object)DBNull.Value;
                    comando.Parameters.Add("@destacado", NpgsqlDbType.Boolean).Value = prenda.Destacado;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = prenda.Activo;

                    prenda.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Guardar imágenes adicionales si existen
                    if (prenda.ImagenesAdicionales != null && prenda.ImagenesAdicionales.Count > 0)
                    {
                        for (int i = 0; i < prenda.ImagenesAdicionales.Count; i++)
                        {
                            GuardarImagenAdicional(prenda.Id, prenda.ImagenesAdicionales[i], i);
                        }
                    }

                    return $"Prenda guardada con ID: {prenda.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la prenda: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void GuardarImagenAdicional(int prendaId, string urlImagen, int orden)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "INSERT INTO imagenes_prendas (prenda_id, url_imagen, orden) VALUES (@prendaId, @urlImagen, @orden)";
                comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;
                comando.Parameters.Add("@urlImagen", NpgsqlDbType.Varchar).Value = urlImagen;
                comando.Parameters.Add("@orden", NpgsqlDbType.Integer).Value = orden;
                comando.ExecuteNonQuery();
            }
        }

        public List<Prenda> Consultar()
        {
            List<Prenda> prendas = new List<Prenda>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.precio_descuento, 
                               p.stock, p.imagen, p.modelo_3d, p.descripcion, p.categoria_id, p.destacado, p.activo,
                               c.id, c.nombre, c.descripcion
                        FROM prendas p
                        LEFT JOIN categorias c ON p.categoria_id = c.id
                        ORDER BY p.nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = null;
                            if (!reader.IsDBNull(11)) // Si tiene categoría
                            {
                                categoria = new Categoria
                                {
                                    Id = reader.GetInt32(14),
                                    Nombre = reader.GetString(15),
                                    Descripcion = reader.IsDBNull(16) ? null : reader.GetString(16)
                                };
                            }

                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Tipo = reader.GetString(2),
                                Talla = reader.GetString(3),
                                Color = reader.GetString(4),
                                Precio = reader.GetDouble(5),
                                PrecioDescuento = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                Stock = reader.GetInt32(7),
                                Imagen = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Modelo3D = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Categoria = categoria,
                                Destacado = reader.GetBoolean(12),
                                Activo = reader.GetBoolean(13),
                                ImagenesAdicionales = new List<string>()
                            };
                            prendas.Add(prenda);
                        }
                    }

                    // Cargar imágenes adicionales para cada prenda
                    foreach (var prenda in prendas)
                    {
                        prenda.ImagenesAdicionales = ConsultarImagenesAdicionales(prenda.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return prendas;
        }

        private List<string> ConsultarImagenesAdicionales(int prendaId)
        {
            List<string> imagenes = new List<string>();
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "SELECT url_imagen FROM imagenes_prendas WHERE prenda_id = @prendaId ORDER BY orden";
                comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        imagenes.Add(reader.GetString(0));
                    }
                }
            }
            return imagenes;
        }

        public string Modificar(Prenda prenda)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE prendas 
                        SET nombre = @nombre, tipo = @tipo, talla = @talla, color = @color, 
                            precio = @precio, precio_descuento = @precioDescuento, stock = @stock, 
                            imagen = @imagen, modelo_3d = @modelo3d, descripcion = @descripcion, 
                            categoria_id = @categoriaId, destacado = @destacado, activo = @activo
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = prenda.Nombre;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = prenda.Tipo;
                    comando.Parameters.Add("@talla", NpgsqlDbType.Varchar).Value = prenda.Talla;
                    comando.Parameters.Add("@color", NpgsqlDbType.Varchar).Value = prenda.Color;
                    comando.Parameters.Add("@precio", NpgsqlDbType.Double).Value = prenda.Precio;
                    comando.Parameters.Add("@precioDescuento", NpgsqlDbType.Double).Value = prenda.PrecioDescuento.HasValue ? (object)prenda.PrecioDescuento.Value : DBNull.Value;
                    comando.Parameters.Add("@stock", NpgsqlDbType.Integer).Value = prenda.Stock;
                    comando.Parameters.Add("@imagen", NpgsqlDbType.Varchar).Value = prenda.Imagen ?? (object)DBNull.Value;
                    comando.Parameters.Add("@modelo3d", NpgsqlDbType.Varchar).Value = prenda.Modelo3D ?? (object)DBNull.Value;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = prenda.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@categoriaId", NpgsqlDbType.Integer).Value = prenda.Categoria?.Id ?? (object)DBNull.Value;
                    comando.Parameters.Add("@destacado", NpgsqlDbType.Boolean).Value = prenda.Destacado;
                    comando.Parameters.Add("@activo", NpgsqlDbType.Boolean).Value = prenda.Activo;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = prenda.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Eliminar imágenes adicionales existentes
                        EliminarImagenesAdicionales(prenda.Id);

                        // Guardar nuevas imágenes adicionales
                        if (prenda.ImagenesAdicionales != null && prenda.ImagenesAdicionales.Count > 0)
                        {
                            for (int i = 0; i < prenda.ImagenesAdicionales.Count; i++)
                            {
                                GuardarImagenAdicional(prenda.Id, prenda.ImagenesAdicionales[i], i);
                            }
                        }

                        return "Prenda modificada correctamente";
                    }
                    else
                    {
                        return "No se encontró la prenda para modificar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la prenda: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarImagenesAdicionales(int prendaId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM imagenes_prendas WHERE prenda_id = @prendaId";
                comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;
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

                    // Primero verificamos si hay items de carrito con esta prenda
                    comando.CommandText = "SELECT COUNT(*) FROM items_carrito WHERE prenda_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadItems = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadItems > 0)
                    {
                        return $"No se puede eliminar la prenda porque está en {cantidadItems} carritos de compra";
                    }

                    // Verificamos si hay detalles de pedido con esta prenda
                    comando.Parameters.Clear();
                    comando.CommandText = "SELECT COUNT(*) FROM detalles_pedido WHERE prenda_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadDetalles = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadDetalles > 0)
                    {
                        return $"No se puede eliminar la prenda porque está en {cantidadDetalles} pedidos";
                    }

                    // Eliminar imágenes adicionales
                    EliminarImagenesAdicionales(id);

                    // Eliminar relaciones con promociones
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM promociones_prendas WHERE prenda_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                    comando.ExecuteNonQuery();

                    // Eliminar pruebas de prendas
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM pruebas_prendas WHERE prenda_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;
                    comando.ExecuteNonQuery();

                    // Finalmente eliminar la prenda
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM prendas WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Prenda eliminada correctamente" : "No se encontró la prenda para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la prenda: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Prenda BuscarPorId(int id)
        {
            Prenda prenda = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.precio_descuento, 
                               p.stock, p.imagen, p.modelo_3d, p.descripcion, p.categoria_id, p.destacado, p.activo,
                               c.id, c.nombre, c.descripcion
                        FROM prendas p
                        LEFT JOIN categorias c ON p.categoria_id = c.id
                        WHERE p.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Categoria categoria = null;
                            if (!reader.IsDBNull(11)) // Si tiene categoría
                            {
                                categoria = new Categoria
                                {
                                    Id = reader.GetInt32(14),
                                    Nombre = reader.GetString(15),
                                    Descripcion = reader.IsDBNull(16) ? null : reader.GetString(16)
                                };
                            }

                            prenda = new Prenda
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Tipo = reader.GetString(2),
                                Talla = reader.GetString(3),
                                Color = reader.GetString(4),
                                Precio = reader.GetDouble(5),
                                PrecioDescuento = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                Stock = reader.GetInt32(7),
                                Imagen = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Modelo3D = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Categoria = categoria,
                                Destacado = reader.GetBoolean(12),
                                Activo = reader.GetBoolean(13),
                                ImagenesAdicionales = new List<string>()
                            };
                        }
                    }

                    // Cargar imágenes adicionales si encontramos la prenda
                    if (prenda != null)
                    {
                        prenda.ImagenesAdicionales = ConsultarImagenesAdicionales(prenda.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar prenda por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return prenda;
        }

        public List<Prenda> ConsultarPrendasDestacadas()
        {
            List<Prenda> prendas = new List<Prenda>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.precio_descuento, 
                               p.stock, p.imagen, p.modelo_3d, p.descripcion, p.categoria_id, p.destacado, p.activo,
                               c.id, c.nombre, c.descripcion
                        FROM prendas p
                        LEFT JOIN categorias c ON p.categoria_id = c.id
                        WHERE p.destacado = true AND p.activo = true
                        ORDER BY p.nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = null;
                            if (!reader.IsDBNull(11)) // Si tiene categoría
                            {
                                categoria = new Categoria
                                {
                                    Id = reader.GetInt32(14),
                                    Nombre = reader.GetString(15),
                                    Descripcion = reader.IsDBNull(16) ? null : reader.GetString(16)
                                };
                            }

                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Tipo = reader.GetString(2),
                                Talla = reader.GetString(3),
                                Color = reader.GetString(4),
                                Precio = reader.GetDouble(5),
                                PrecioDescuento = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                Stock = reader.GetInt32(7),
                                Imagen = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Modelo3D = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Categoria = categoria,
                                Destacado = reader.GetBoolean(12),
                                Activo = reader.GetBoolean(13),
                                ImagenesAdicionales = new List<string>()
                            };
                            prendas.Add(prenda);
                        }
                    }

                    // Cargar imágenes adicionales para cada prenda
                    foreach (var prenda in prendas)
                    {
                        prenda.ImagenesAdicionales = ConsultarImagenesAdicionales(prenda.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas destacadas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return prendas;
        }

        public List<Prenda> BuscarPrendas(string termino)
        {
            List<Prenda> prendas = new List<Prenda>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.precio_descuento, 
                               p.stock, p.imagen, p.modelo_3d, p.descripcion, p.categoria_id, p.destacado, p.activo,
                               c.id, c.nombre, c.descripcion
                        FROM prendas p
                        LEFT JOIN categorias c ON p.categoria_id = c.id
                        WHERE (p.nombre ILIKE @termino OR p.tipo ILIKE @termino OR p.color ILIKE @termino OR p.descripcion ILIKE @termino)
                        AND p.activo = true
                        ORDER BY p.nombre";
                    comando.Parameters.Add("@termino", NpgsqlDbType.Varchar).Value = "%" + termino + "%";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = null;
                            if (!reader.IsDBNull(11)) // Si tiene categoría
                            {
                                categoria = new Categoria
                                {
                                    Id = reader.GetInt32(14),
                                    Nombre = reader.GetString(15),
                                    Descripcion = reader.IsDBNull(16) ? null : reader.GetString(16)
                                };
                            }

                            Prenda prenda = new Prenda
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Tipo = reader.GetString(2),
                                Talla = reader.GetString(3),
                                Color = reader.GetString(4),
                                Precio = reader.GetDouble(5),
                                PrecioDescuento = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                Stock = reader.GetInt32(7),
                                Imagen = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Modelo3D = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Categoria = categoria,
                                Destacado = reader.GetBoolean(12),
                                Activo = reader.GetBoolean(13),
                                ImagenesAdicionales = new List<string>()
                            };
                            prendas.Add(prenda);
                        }
                    }

                    // Cargar imágenes adicionales para cada prenda
                    foreach (var prenda in prendas)
                    {
                        prenda.ImagenesAdicionales = ConsultarImagenesAdicionales(prenda.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar prendas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return prendas;
        }

        public string ActualizarStock(int prendaId, int cantidad)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE prendas SET stock = stock + @cantidad WHERE id = @id";
                    comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidad;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = prendaId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Stock actualizado correctamente" : "No se encontró la prenda para actualizar stock";
                }
            }
            catch (Exception ex)
            {
                return $"Error al actualizar stock: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}
