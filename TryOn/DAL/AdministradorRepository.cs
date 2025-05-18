using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DAL
{
    public class AdministradorRepository : BaseDatos, IRepository<Administrador>
    {
        public string Guardar(Administrador administrador)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Verificar si ya existe un usuario con el email proporcionado
                    comando.CommandText = "SELECT COUNT(*) FROM usuarios WHERE email = @email";
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = administrador.Email;

                    int cantidadUsuarios = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadUsuarios > 0)
                    {
                        return "Ya existe un usuario con ese email";
                    }

                    // Guardar el administrador (que hereda de Usuario)
                    comando.Parameters.Clear();
                    comando.CommandText = @"
                        INSERT INTO administradores (nombre, apellido, telefono, email, contrasena, cargo, departamento)
                        VALUES (@nombre, @apellido, @telefono, @email, @contrasena, @cargo, @departamento)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = administrador.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = administrador.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = administrador.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = administrador.Email;
                    comando.Parameters.Add("@contrasena", NpgsqlDbType.Varchar).Value = HashPassword(administrador.Contrasena);
                    comando.Parameters.Add("@cargo", NpgsqlDbType.Varchar).Value = administrador.Cargo;
                    comando.Parameters.Add("@departamento", NpgsqlDbType.Varchar).Value = administrador.Departamento;

                    administrador.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Administrador guardado con ID: {administrador.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el administrador: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public List<Administrador> Consultar()
        {
            List<Administrador> administradores = new List<Administrador>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email, cargo, departamento
                        FROM administradores
                        ORDER BY apellido, nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Administrador administrador = new Administrador
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Cargo = reader.GetString(5),
                                Departamento = reader.GetString(6),
                                Contrasena = "********" // No devolvemos la contraseña real
                            };
                            administradores.Add(administrador);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar administradores: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return administradores;
        }

        public string Modificar(Administrador administrador)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Verificar si el email ya está en uso por otro usuario
                    comando.CommandText = @"
                        SELECT COUNT(*) 
                        FROM administradores 
                        WHERE email = @email AND id <> @id";
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = administrador.Email;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = administrador.Id;

                    int cantidadUsuarios = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadUsuarios > 0)
                    {
                        return "Ya existe otro usuario con ese email";
                    }

                    // Actualizar datos básicos
                    comando.Parameters.Clear();
                    comando.CommandText = @"
                        UPDATE administradores 
                        SET nombre = @nombre, apellido = @apellido, telefono = @telefono, 
                            email = @email, cargo = @cargo, departamento = @departamento
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = administrador.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = administrador.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = administrador.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = administrador.Email;
                    comando.Parameters.Add("@cargo", NpgsqlDbType.Varchar).Value = administrador.Cargo;
                    comando.Parameters.Add("@departamento", NpgsqlDbType.Varchar).Value = administrador.Departamento;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = administrador.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    // Si se proporcionó una nueva contraseña, actualizarla
                    if (!string.IsNullOrEmpty(administrador.Contrasena) && administrador.Contrasena != "********")
                    {
                        comando.Parameters.Clear();
                        comando.CommandText = "UPDATE administradores SET contrasena = @contrasena WHERE id = @id";
                        comando.Parameters.Add("@contrasena", NpgsqlDbType.Varchar).Value = HashPassword(administrador.Contrasena);
                        comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = administrador.Id;

                        comando.ExecuteNonQuery();
                    }

                    return filasAfectadas > 0 ? "Administrador modificado correctamente" : "No se encontró el administrador para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el administrador: {ex.Message}";
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

                    // Verificar si hay reportes creados por este administrador
                    comando.CommandText = "SELECT COUNT(*) FROM reportes WHERE creado_por = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadReportes = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadReportes > 0)
                    {
                        return $"No se puede eliminar el administrador porque ha creado {cantidadReportes} reportes";
                    }

                    // Eliminar el administrador
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM administradores WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Administrador eliminado correctamente" : "No se encontró el administrador para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el administrador: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Administrador BuscarPorId(int id)
        {
            Administrador administrador = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email, cargo, departamento
                        FROM administradores
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            administrador = new Administrador
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Cargo = reader.GetString(5),
                                Departamento = reader.GetString(6),
                                Contrasena = "********" // No devolvemos la contraseña real
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar administrador por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return administrador;
        }

        public Administrador BuscarPorEmail(string email)
        {
            Administrador administrador = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email, cargo, departamento
                        FROM administradores
                        WHERE email = @email";
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = email;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            administrador = new Administrador
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Cargo = reader.GetString(5),
                                Departamento = reader.GetString(6),
                                Contrasena = "********" // No devolvemos la contraseña real
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar administrador por email: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return administrador;
        }

        public bool ValidarCredenciales(string email, string password)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT contrasena FROM administradores WHERE email = @email";
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = email;

                    object result = comando.ExecuteScalar();
                    if (result != null)
                    {
                        string storedHash = result.ToString();
                        string inputHash = HashPassword(password);

                        return storedHash == inputHash;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}