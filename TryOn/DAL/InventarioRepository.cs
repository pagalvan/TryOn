using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class InventarioRepository : BaseDatos, IRepository<Inventario>
    {
        public string Guardar(Inventario inventario)
        {
            try
            {
                AbrirConexion();

                // Primero creamos un registro de inventario vacío
                int inventarioId;
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO inventarios (fecha_actualizacion)
                        VALUES (CURRENT_TIMESTAMP)
                        RETURNING id";

                    inventarioId = Convert.ToInt32(comando.ExecuteScalar());
                }

                // Luego guardamos cada prenda en el inventario
                if (inventario.Prendas != null && inventario.Prendas.Count > 0)
                {
                    foreach (var prenda in inventario.Prendas)
                    {
                        GuardarPrendaEnInventario(inventarioId, prenda);
                    }
                }

                return $"Inventario guardado con ID: {inventarioId}";
            }
            catch (Exception ex)
            {
                return $"Error al guardar el inventario: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void GuardarPrendaEnInventario(int inventarioId, Prenda prenda)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    INSERT INTO inventario_prendas (inventario_id, prenda_id, cantidad, ubicacion)
                    VALUES (@inventarioId, @prendaId, @cantidad, @ubicacion)";

                comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prenda.Id;
                comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = prenda.Cantidad;
                comando.Parameters.Add("@ubicacion", NpgsqlDbType.Varchar).Value = prenda.Ubicacion ?? (object)DBNull.Value;

                comando.ExecuteNonQuery();
            }
        }

        public List<Inventario> Consultar()
        {
            List<Inventario> inventarios = new List<Inventario>();
            try
            {
                AbrirConexion();

                // Obtener todos los inventarios
                List<int> inventarioIds = new List<int>();
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, fecha_actualizacion
                        FROM inventarios
                        ORDER BY fecha_actualizacion DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int inventarioId = reader.GetInt32(0);
                            inventarioIds.Add(inventarioId);

                            Inventario inventario = new Inventario
                            {
                                Id = inventarioId,
                                FechaActualizacion = reader.GetDateTime(1),
                                Prendas = new List<Prenda>()
                            };

                            inventarios.Add(inventario);
                        }
                    }
                }

                // Cargar las prendas para cada inventario
                foreach (var inventario in inventarios)
                {
                    inventario.Prendas = ConsultarPrendasPorInventario(inventario.Id);
                }

                return inventarios;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar inventarios: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
        }

        private List<Prenda> ConsultarPrendasPorInventario(int inventarioId)
        {
            List<Prenda> prendas = new List<Prenda>();
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    SELECT ip.prenda_id, ip.cantidad, ip.ubicacion,
                           p.id, p.nombre, p.tipo, p.talla, p.color, p.precio, p.imagen, p.descripcion
                    FROM inventario_prendas ip
                    JOIN prendas p ON ip.prenda_id = p.id
                    WHERE ip.inventario_id = @inventarioId";
                comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;

                using (var reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Prenda prenda = new Prenda
                        {
                            Id = reader.GetInt32(3),
                            Nombre = reader.GetString(4),
                            Tipo = reader.GetString(5),
                            Talla = reader.GetString(6),
                            Color = reader.GetString(7),
                            Precio = reader.GetDouble(8),
                            Imagen = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Descripcion = reader.IsDBNull(10) ? null : reader.GetString(10),
                            Cantidad = reader.GetInt32(1),
                            Ubicacion = reader.IsDBNull(2) ? null : reader.GetString(2)
                        };

                        prendas.Add(prenda);
                    }
                }
            }

            return prendas;
        }

        public string Modificar(Inventario inventario)
        {
            try
            {
                AbrirConexion();

                // Actualizar la fecha de actualización del inventario
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE inventarios 
                        SET fecha_actualizacion = CURRENT_TIMESTAMP
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = inventario.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        return "No se encontró el inventario para modificar";
                    }
                }

                // Eliminar todas las prendas actuales del inventario
                EliminarPrendasDeInventario(inventario.Id);

                // Guardar las nuevas prendas
                if (inventario.Prendas != null && inventario.Prendas.Count > 0)
                {
                    foreach (var prenda in inventario.Prendas)
                    {
                        GuardarPrendaEnInventario(inventario.Id, prenda);
                    }
                }

                return "Inventario modificado correctamente";
            }
            catch (Exception ex)
            {
                return $"Error al modificar el inventario: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarPrendasDeInventario(int inventarioId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM inventario_prendas WHERE inventario_id = @inventarioId";
                comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                comando.ExecuteNonQuery();
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                AbrirConexion();

                // Primero eliminar las prendas asociadas al inventario
                EliminarPrendasDeInventario(id);

                // Luego eliminar el inventario
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = "DELETE FROM inventarios WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Inventario eliminado correctamente" : "No se encontró el inventario para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el inventario: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Inventario BuscarPorId(int id)
        {
            Inventario inventario = null;
            try
            {
                AbrirConexion();

                // Obtener el inventario
                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, fecha_actualizacion
                        FROM inventarios
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            inventario = new Inventario
                            {
                                Id = reader.GetInt32(0),
                                FechaActualizacion = reader.GetDateTime(1),
                                Prendas = new List<Prenda>()
                            };
                        }
                    }
                }

                // Si encontramos el inventario, cargar sus prendas
                if (inventario != null)
                {
                    inventario.Prendas = ConsultarPrendasPorInventario(inventario.Id);
                }

                return inventario;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar inventario por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
        }

        public string ActualizarCantidadPrenda(int inventarioId, int prendaId, int cantidad)
        {
            try
            {
                AbrirConexion();

                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;

                    // Verificar si la prenda ya existe en el inventario
                    comando.CommandText = @"
                        SELECT cantidad 
                        FROM inventario_prendas 
                        WHERE inventario_id = @inventarioId AND prenda_id = @prendaId";
                    comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                    object result = comando.ExecuteScalar();

                    if (result != null)
                    {
                        // Actualizar la cantidad existente
                        int cantidadActual = Convert.ToInt32(result);

                        comando.Parameters.Clear();
                        comando.CommandText = @"
                            UPDATE inventario_prendas 
                            SET cantidad = @cantidad 
                            WHERE inventario_id = @inventarioId AND prenda_id = @prendaId";
                        comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidadActual + cantidad;
                        comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                        comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                        comando.ExecuteNonQuery();

                        // Actualizar la fecha de actualización del inventario
                        ActualizarFechaInventario(inventarioId);

                        return "Cantidad de prenda actualizada correctamente";
                    }
                    else
                    {
                        // La prenda no existe en el inventario, obtener información de la prenda
                        comando.Parameters.Clear();
                        comando.CommandText = "SELECT id FROM prendas WHERE id = @prendaId";
                        comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                        result = comando.ExecuteScalar();

                        if (result == null)
                        {
                            return "La prenda especificada no existe";
                        }

                        // Agregar la prenda al inventario
                        comando.Parameters.Clear();
                        comando.CommandText = @"
                            INSERT INTO inventario_prendas (inventario_id, prenda_id, cantidad)
                            VALUES (@inventarioId, @prendaId, @cantidad)";
                        comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                        comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;
                        comando.Parameters.Add("@cantidad", NpgsqlDbType.Integer).Value = cantidad;

                        comando.ExecuteNonQuery();

                        // Actualizar la fecha de actualización del inventario
                        ActualizarFechaInventario(inventarioId);

                        return "Prenda agregada al inventario correctamente";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al actualizar cantidad de prenda: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void ActualizarFechaInventario(int inventarioId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = @"
                    UPDATE inventarios 
                    SET fecha_actualizacion = CURRENT_TIMESTAMP
                    WHERE id = @id";
                comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = inventarioId;
                comando.ExecuteNonQuery();
            }
        }

        public string EliminarPrendaDeInventario(int inventarioId, int prendaId)
        {
            try
            {
                AbrirConexion();

                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        DELETE FROM inventario_prendas 
                        WHERE inventario_id = @inventarioId AND prenda_id = @prendaId";
                    comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Actualizar la fecha de actualización del inventario
                        ActualizarFechaInventario(inventarioId);
                        return "Prenda eliminada del inventario correctamente";
                    }
                    else
                    {
                        return "No se encontró la prenda en el inventario";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar prenda del inventario: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public int ObtenerCantidadPrenda(int inventarioId, int prendaId)
        {
            try
            {
                AbrirConexion();

                using (var comando = new NpgsqlCommand())
                {
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT cantidad 
                        FROM inventario_prendas 
                        WHERE inventario_id = @inventarioId AND prenda_id = @prendaId";
                    comando.Parameters.Add("@inventarioId", NpgsqlDbType.Integer).Value = inventarioId;
                    comando.Parameters.Add("@prendaId", NpgsqlDbType.Integer).Value = prendaId;

                    object result = comando.ExecuteScalar();

                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cantidad de prenda: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}
