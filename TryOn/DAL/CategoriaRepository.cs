using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;

namespace DAL
{
    public class CategoriaRepository : BaseDatos, IRepository<Categoria>
    {
        public string Guardar(Categoria categoria)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "INSERT INTO categorias (nombre, descripcion) VALUES (@nombre, @descripcion) RETURNING id";
                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = categoria.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = categoria.Descripcion ?? (object)DBNull.Value;

                    categoria.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Categoría guardada con ID: {categoria.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la categoría: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Categoria> Consultar()
        {
            List<Categoria> categorias = new List<Categoria>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT id, nombre, descripcion FROM categorias ORDER BY nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = new Categoria
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Prendas = new List<Prenda>()
                            };
                            categorias.Add(categoria);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar categorías: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return categorias;
        }

        public string Modificar(Categoria categoria)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE categorias SET nombre = @nombre, descripcion = @descripcion WHERE id = @id";
                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = categoria.Nombre;
                    comando.Parameters.Add("@descripcion", NpgsqlDbType.Text).Value = categoria.Descripcion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = categoria.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Categoría modificada correctamente" : "No se encontró la categoría para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la categoría: {ex.Message}";
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

                    // Primero verificamos si hay prendas asociadas a esta categoría
                    comando.CommandText = "SELECT COUNT(*) FROM prendas WHERE categoria_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadPrendas = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadPrendas > 0)
                    {
                        return $"No se puede eliminar la categoría porque tiene {cantidadPrendas} prendas asociadas";
                    }

                    // Si no hay prendas asociadas, procedemos a eliminar
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM categorias WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Categoría eliminada correctamente" : "No se encontró la categoría para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la categoría: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Categoria BuscarPorId(int id)
        {
            Categoria categoria = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT id, nombre, descripcion FROM categorias WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            categoria = new Categoria
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Prendas = new List<Prenda>()
                            };
                        }
                    }
                }

                // Si encontramos la categoría, cargamos sus prendas
                if (categoria != null)
                {
                    categoria.Prendas = ConsultarPrendasPorCategoria(id);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar categoría por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return categoria;
        }

        public List<Prenda> ConsultarPrendasPorCategoria(int categoriaId)
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
                               p.stock, p.imagen, p.modelo_3d, p.descripcion, p.destacado, p.activo
                        FROM prendas p
                        WHERE p.categoria_id = @categoriaId
                        ORDER BY p.nombre";
                    comando.Parameters.Add("@categoriaId", NpgsqlDbType.Integer).Value = categoriaId;

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
                                Precio = reader.GetDouble(5),
                                PrecioDescuento = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                Stock = reader.GetInt32(7),
                                Imagen = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Modelo3D = reader.IsDBNull(9) ? null : reader.GetString(9),
                                Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                                Destacado = reader.GetBoolean(11),
                                Activo = reader.GetBoolean(12),
                                ImagenesAdicionales = new List<string>()
                            };
                            prendas.Add(prenda);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar prendas por categoría: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return prendas;
        }

        public List<Categoria> ConsultarCategoriasActivas()
        {
            List<Categoria> categorias = new List<Categoria>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT id, nombre, descripcion FROM categorias WHERE activo = true ORDER BY nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Categoria categoria = new Categoria
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Descripcion = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Prendas = new List<Prenda>()
                            };
                            categorias.Add(categoria);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar categorías activas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return categorias;
        }
    }
}
