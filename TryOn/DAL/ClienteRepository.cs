using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace DAL
{
    public class ClienteRepository : BaseDatos, IRepository<Cliente>
    {
        private readonly MedidaRepository _medidaRepository;
        private readonly PreferenciaRepository _preferenciaRepository;

        public ClienteRepository()
        {
            _medidaRepository = new MedidaRepository();
            _preferenciaRepository = new PreferenciaRepository();
        }

        public string Guardar(Cliente cliente)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO clientes (nombre, apellido, telefono, direccion)
                        VALUES (@nombre, @apellido, @telefono, @direccion)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = cliente.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = cliente.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = cliente.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@direccion", NpgsqlDbType.Text).Value = cliente.Direccion ?? (object)DBNull.Value;

                    cliente.Id = Convert.ToInt32(comando.ExecuteScalar());

                    // Guardar medidas si existen
                    if (cliente.Medidas != null && cliente.Medidas.Count > 0)
                    {
                        foreach (var medida in cliente.Medidas)
                        {
                            medida.Cliente = new Cliente { Id = cliente.Id };
                            _medidaRepository.Guardar(medida);
                        }
                    }

                    // Guardar preferencias si existen
                    if (cliente.Preferencias != null && cliente.Preferencias.Count > 0)
                    {
                        foreach (var preferencia in cliente.Preferencias)
                        {
                            preferencia.Cliente = new Cliente { Id = cliente.Id };
                            _preferenciaRepository.Guardar(preferencia);
                        }
                    }

                    return $"Cliente guardado con ID: {cliente.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el cliente: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public List<Cliente> Consultar()
        {
            List<Cliente> clientes = new List<Cliente>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, direccion
                        FROM clientes
                        ORDER BY apellido, nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cliente cliente = new Cliente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Direccion = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Medidas = new List<Medida>(),
                                Preferencias = new List<Preferencia>()
                            };
                            clientes.Add(cliente);
                        }
                    }

                    // Cargar medidas y preferencias para cada cliente
                    foreach (var cliente in clientes)
                    {
                        cliente.Medidas = _medidaRepository.ConsultarPorCliente(cliente.Id);
                        cliente.Preferencias = _preferenciaRepository.ConsultarPorCliente(cliente.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar clientes: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return clientes;
        }

        public string Modificar(Cliente cliente)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        UPDATE clientes 
                        SET nombre = @nombre, apellido = @apellido, telefono = @telefono, direccion = @direccion
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = cliente.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = cliente.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = cliente.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@direccion", NpgsqlDbType.Text).Value = cliente.Direccion ?? (object)DBNull.Value;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = cliente.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        // Actualizar medidas
                        if (cliente.Medidas != null)
                        {
                            // Eliminar medidas existentes
                            EliminarMedidasCliente(cliente.Id);

                            // Guardar nuevas medidas
                            foreach (var medida in cliente.Medidas)
                            {
                                medida.Cliente = new Cliente { Id = cliente.Id };
                                _medidaRepository.Guardar(medida);
                            }
                        }

                        // Actualizar preferencias
                        if (cliente.Preferencias != null)
                        {
                            // Eliminar preferencias existentes
                            EliminarPreferenciasCliente(cliente.Id);

                            // Guardar nuevas preferencias
                            foreach (var preferencia in cliente.Preferencias)
                            {
                                preferencia.Cliente = new Cliente { Id = cliente.Id };
                                _preferenciaRepository.Guardar(preferencia);
                            }
                        }

                        return "Cliente modificado correctamente";
                    }
                    else
                    {
                        return "No se encontró el cliente para modificar";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el cliente: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private void EliminarMedidasCliente(int clienteId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM medidas WHERE cliente_id = @clienteId";
                comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;
                comando.ExecuteNonQuery();
            }
        }

        private void EliminarPreferenciasCliente(int clienteId)
        {
            using (var comando = new NpgsqlCommand())
            {
                comando.Connection = conexion;
                comando.CommandText = "DELETE FROM preferencias WHERE cliente_id = @clienteId";
                comando.Parameters.Add("@clienteId", NpgsqlDbType.Integer).Value = clienteId;
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

                    // Verificar si hay pedidos asociados a este cliente
                    comando.CommandText = "SELECT COUNT(*) FROM pedidos WHERE cliente_id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadPedidos = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadPedidos > 0)
                    {
                        return $"No se puede eliminar el cliente porque tiene {cantidadPedidos} pedidos asociados";
                    }

                    // Eliminar medidas y preferencias
                    EliminarMedidasCliente(id);
                    EliminarPreferenciasCliente(id);

                    // Eliminar el cliente
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM clientes WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Cliente eliminado correctamente" : "No se encontró el cliente para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el cliente: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Cliente BuscarPorId(int id)
        {
            Cliente cliente = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, direccion
                        FROM clientes
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cliente = new Cliente
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Direccion = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Medidas = new List<Medida>(),
                                Preferencias = new List<Preferencia>()
                            };
                        }
                    }

                    // Cargar medidas y preferencias si encontramos el cliente
                    if (cliente != null)
                    {
                        cliente.Medidas = _medidaRepository.ConsultarPorCliente(cliente.Id);
                        cliente.Preferencias = _preferenciaRepository.ConsultarPorCliente(cliente.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar cliente por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return cliente;
        }
    }
}

