using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class PreferenciaRepository : BaseDatos, IRepository<Preferencia>
    {
        public string Guardar(Preferencia preferencia)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO preferencias (cliente_id, categoria, valor, prioridad)
                        VALUES (@clienteId, @categoria, @valor, @prioridad)
                        RETURNING id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = preferencia.Cliente.Id;
                    comando.Parameters.Add("@categoria", NpgsqlDbType.Varchar).Value = preferencia.Categoria;
                    comando.Parameters.Add("@valor", NpgsqlDbType.Varchar).Value = preferencia.Valor;
                    comando.Parameters.Add("@prioridad", NpgsqlDbType.Integer).Value = preferencia.Prioridad;

                    preferencia.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Preferencia guardada con ID: {preferencia.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la preferencia: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Preferencia> Consultar()
        {
            List<Preferencia> preferencias = new List<Preferencia>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.categoria, p.valor, p.prioridad
                        FROM preferencias p
                        ORDER BY p.cliente_id, p.prioridad DESC";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Preferencia preferencia = new Preferencia
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Categoria = reader.GetString(2),
                                Valor = reader.GetString(3),
                                Prioridad = reader.GetInt32(4)
                            };
                            preferencias.Add(preferencia);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar preferencias: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return preferencias;
        }

        public string Modificar(Preferencia preferencia)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE preferencias 
                        SET cliente_id = @clienteId, categoria = @categoria, valor = @valor, prioridad = @prioridad
                        WHERE id = @id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = preferencia.Cliente.Id;
                    comando.Parameters.Add("@categoria", NpgsqlDbType.Varchar).Value = preferencia.Categoria;
                    comando.Parameters.Add("@valor", NpgsqlDbType.Varchar).Value = preferencia.Valor;
                    comando.Parameters.Add("@prioridad", NpgsqlDbType.Integer).Value = preferencia.Prioridad;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = preferencia.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Preferencia modificada correctamente" : "No se encontró la preferencia para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la preferencia: {ex.Message}";
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
                    comando.CommandText = "DELETE FROM preferencias WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Preferencia eliminada correctamente" : "No se encontró la preferencia para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la preferencia: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Preferencia BuscarPorId(int id)
        {
            Preferencia preferencia = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.categoria, p.valor, p.prioridad
                        FROM preferencias p
                        WHERE p.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            preferencia = new Preferencia
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Categoria = reader.GetString(2),
                                Valor = reader.GetString(3),
                                Prioridad = reader.GetInt32(4)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar preferencia por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return preferencia;
        }

        public List<Preferencia> ConsultarPorCliente(int clienteId)
        {
            List<Preferencia> preferencias = new List<Preferencia>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.categoria, p.valor, p.prioridad
                        FROM preferencias p
                        WHERE p.cliente_id = @clienteId
                        ORDER BY p.prioridad DESC";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Preferencia preferencia = new Preferencia
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Categoria = reader.GetString(2),
                                Valor = reader.GetString(3),
                                Prioridad = reader.GetInt32(4)
                            };
                            preferencias.Add(preferencia);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar preferencias por cliente: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return preferencias;
        }

        public List<Preferencia> ConsultarPorCategoria(string categoria)
        {
            List<Preferencia> preferencias = new List<Preferencia>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT p.id, p.cliente_id, p.categoria, p.valor, p.prioridad
                        FROM preferencias p
                        WHERE p.categoria = @categoria
                        ORDER BY p.prioridad DESC";
                    comando.Parameters.Add("@categoria", NpgsqlDbType.Varchar).Value = categoria;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Preferencia preferencia = new Preferencia
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Categoria = reader.GetString(2),
                                Valor = reader.GetString(3),
                                Prioridad = reader.GetInt32(4)
                            };
                            preferencias.Add(preferencia);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar preferencias por categoría: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return preferencias;
        }
    }
}