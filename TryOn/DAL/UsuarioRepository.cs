using Entities;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DAL
{
    public class UsuarioRepository : BaseDatos, IRepository<Usuario>
    {
        public string Guardar(Usuario usuario)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        INSERT INTO usuarios (nombre, apellido, telefono, email, contrasena)
                        VALUES (@nombre, @apellido, @telefono, @email, @contrasena)
                        RETURNING id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = usuario.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = usuario.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = usuario.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = usuario.Email;
                    comando.Parameters.Add("@contrasena", NpgsqlDbType.Varchar).Value = HashPassword(usuario.Password);

                    usuario.Id = Convert.ToInt32(comando.ExecuteScalar());
                    return $"Usuario guardado con ID: {usuario.Id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error al guardar el usuario: {ex.Message}";
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

        public List<Usuario> Consultar()
        {
            List<Usuario> usuarios = new List<Usuario>();
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email
                        FROM usuarios
                        ORDER BY apellido, nombre";

                    using (var reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Usuario usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Password = "********" // No devolvemos la contraseña real
                            };
                            usuarios.Add(usuario);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar usuarios: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return usuarios;
        }

        public string Modificar(Usuario usuario)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;

                    // Actualizar datos básicos
                    comando.CommandText = @"
                        UPDATE usuarios 
                        SET nombre = @nombre, apellido = @apellido, telefono = @telefono, email = @email
                        WHERE id = @id";

                    comando.Parameters.Add("@nombre", NpgsqlDbType.Varchar).Value = usuario.Nombre;
                    comando.Parameters.Add("@apellido", NpgsqlDbType.Varchar).Value = usuario.Apellido;
                    comando.Parameters.Add("@telefono", NpgsqlDbType.Varchar).Value = usuario.Telefono ?? (object)DBNull.Value;
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = usuario.Email;
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = usuario.Id;

                    int filasAfectadas = comando.ExecuteNonQuery();

                    // Si se proporcionó una nueva contraseña, actualizarla
                    if (!string.IsNullOrEmpty(usuario.Password) && usuario.Password != "********")
                    {
                        comando.Parameters.Clear();
                        comando.CommandText = "UPDATE usuarios SET contrasena = @contrasena WHERE id = @id";
                        comando.Parameters.Add("@contrasena", NpgsqlDbType.Varchar).Value = HashPassword(usuario.Password);
                        comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = usuario.Id;

                        comando.ExecuteNonQuery();
                    }

                    return filasAfectadas > 0 ? "Usuario modificado correctamente" : "No se encontró el usuario para modificar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al modificar el usuario: {ex.Message}";
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

                    // Verificar si el usuario es un administrador
                    comando.CommandText = "SELECT COUNT(*) FROM administradores WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadAdmin = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadAdmin > 0)
                    {
                        return "No se puede eliminar el usuario porque es un administrador";
                    }

                    // Verificar si el usuario es un cliente
                    comando.Parameters.Clear();
                    comando.CommandText = "SELECT COUNT(*) FROM clientes WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int cantidadCliente = Convert.ToInt32(comando.ExecuteScalar());
                    if (cantidadCliente > 0)
                    {
                        return "No se puede eliminar el usuario porque está asociado a un cliente";
                    }

                    // Eliminar el usuario
                    comando.Parameters.Clear();
                    comando.CommandText = "DELETE FROM usuarios WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Usuario eliminado correctamente" : "No se encontró el usuario para eliminar";
                }
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el usuario: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }

        public Usuario BuscarPorId(int id)
        {
            Usuario usuario = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email
                        FROM usuarios
                        WHERE id = @id";
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = id;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Password = "********" // No devolvemos la contraseña real
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar usuario por ID: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return usuario;
        }

        public Usuario BuscarPorEmail(string email)
        {
            Usuario usuario = null;
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = @"
                        SELECT id, nombre, apellido, telefono, email
                        FROM usuarios
                        WHERE email = @email";
                    comando.Parameters.Add("@email", NpgsqlDbType.Varchar).Value = email;

                    using (var reader = comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Id = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Apellido = reader.GetString(2),
                                Telefono = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Email = reader.GetString(4),
                                Password = "********" // No devolvemos la contraseña real
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar usuario por email: {ex.Message}");
            }
            finally
            {
                CerrarConexion();
            }
            return usuario;
        }

        public bool ValidarCredenciales(string email, string password)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "SELECT contrasena FROM usuarios WHERE email = @email";
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

        public string CambiarContrasena(int usuarioId, string nuevaContrasena)
        {
            try
            {
                using (var comando = new NpgsqlCommand())
                {
                    AbrirConexion();
                    comando.Connection = conexion;
                    comando.CommandText = "UPDATE usuarios SET contrasena = @contrasena WHERE id = @id";
                    comando.Parameters.Add("@contrasena", NpgsqlDbType.Varchar).Value = HashPassword(nuevaContrasena);
                    comando.Parameters.Add("@id", NpgsqlDbType.Integer).Value = usuarioId;

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0 ? "Contraseña actualizada correctamente" : "No se encontró el usuario para actualizar la contraseña";
                }
            }
            catch (Exception ex)
            {
                return $"Error al cambiar la contraseña: {ex.Message}";
            }
            finally
            {
                CerrarConexion();
            }
        }
    }
}
