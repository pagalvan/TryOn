using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class MedidaRepository : BaseDatos, IRepository<Medida>
    {
        public string Guardar(Medida medida)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO medidas (cliente_id, tipo, valor, unidad)
                        VALUES (@clienteId, @tipo, @valor, @unidad)
                        RETURNING id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = medida.Cliente.Id;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = medida.Tipo;
                    comando.Parameters.Add("@valor", NpgsqlDbType.Double).Value = medida.Valor;
                    comando.Parameters.Add("@unidad", NpgsqlDbType.Varchar).Value = medida.Unidad;

                    medida.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Medida guardada con ID: {medida.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar la medida: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Medida> Consultar()
        {
            List<Medida> medidas = new List<Medida>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT m.id, m.cliente_id, m.tipo, m.valor, m.unidad
                        FROM medidas m
                        ORDER BY m.cliente_id, m.tipo";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medida medida = new Medida
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Tipo = reader.GetString(2),
                                Valor = reader.GetDouble(3),
                                Unidad = reader.GetString(4)
                            };
                            medidas.Add(medida);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar medidas: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return medidas;
        }

        public string Modificar(Medida medida)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE medidas 
                        SET cliente_id = @clienteId, tipo = @tipo, valor = @valor, unidad = @unidad
                        WHERE id = @id";

                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = medida.Cliente.Id;
                    comando.Parameters.Add("@tipo", NpgsqlDbType.Varchar).Value = medida.Tipo;
                    comando.Parameters.Add("@valor", NpgsqlDbType.Double).Value = medida.Valor;
                    comando.Parameters.Add("@unidad", NpgsqlDbType.Varchar).Value = medida.Unidad;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = medida.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Medida modificada correctamente" : "No se encontró la medida para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar la medida: {ex.Message}";
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
                    comando.CommandText = "DELETE FROM medidas WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Medida eliminada correctamente" : "No se encontró la medida para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar la medida: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Medida BuscarPorId(int id)
        {
            Medida medida = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT m.id, m.cliente_id, m.tipo, m.valor, m.unidad
                        FROM medidas m
                        WHERE m.id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            medida = new Medida
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Tipo = reader.GetString(2),
                                Valor = reader.GetDouble(3),
                                Unidad = reader.GetString(4)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar medida por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return medida;
        }

        public List<Medida> ConsultarPorCliente(int clienteId)
        {
            List<Medida> medidas = new List<Medida>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT m.id, m.cliente_id, m.tipo, m.valor, m.unidad
                        FROM medidas m
                        WHERE m.cliente_id = @clienteId
                        ORDER BY m.tipo";
                    comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Medida medida = new Medida
                            {
                                Id = reader.GetInt32(0),
                                Cliente = new Cliente { Id = reader.GetInt32(1) },
                                Tipo = reader.GetString(2),
                                Valor = reader.GetDouble(3),
                                Unidad = reader.GetString(4)
                            };
                            medidas.Add(medida);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar medidas por cliente: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return medidas;
        }
    }
}