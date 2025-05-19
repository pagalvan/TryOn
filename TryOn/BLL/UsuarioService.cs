using DAL;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BLL
{
    public class UsuarioService : IService<Usuario>
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuarioService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        public string Guardar(Usuario usuario)
        {
            try
            {
                // Validaciones
                var validacion = ValidarUsuario(usuario);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                // Verificar si ya existe un usuario con el mismo email
                var usuarioExistente = _usuarioRepository.BuscarPorEmail(usuario.Email);
                if (usuarioExistente != null)
                {
                    return $"Ya existe un usuario con el email {usuario.Email}";
                }

                // Guardar el usuario
                return _usuarioRepository.Guardar(usuario);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el usuario: {ex.Message}";
            }
        }

        public List<Usuario> Consultar()
        {
            try
            {
                return _usuarioRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar usuarios: {ex.Message}");
            }
        }

        public Usuario BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del usuario debe ser mayor que cero");
                }

                return _usuarioRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar usuario: {ex.Message}");
            }
        }

        public Usuario BuscarPorEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("El email no puede estar vacío");
                }

                return _usuarioRepository.BuscarPorEmail(email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar usuario por email: {ex.Message}");
            }
        }

        public string Modificar(Usuario usuario)
        {
            try
            {
                // Validaciones
                var validacion = ValidarUsuario(usuario);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                if (usuario.Id <= 0)
                {
                    return "El ID del usuario debe ser mayor que cero";
                }

                // Verificar que el usuario exista
                var usuarioExistente = _usuarioRepository.BuscarPorId(usuario.Id);
                if (usuarioExistente == null)
                {
                    return $"No se encontró un usuario con ID {usuario.Id}";
                }

                // Verificar si el email ya está en uso por otro usuario
                var usuarioPorEmail = _usuarioRepository.BuscarPorEmail(usuario.Email);
                if (usuarioPorEmail != null && usuarioPorEmail.Id != usuario.Id)
                {
                    return $"El email {usuario.Email} ya está en uso por otro usuario";
                }

                // Modificar el usuario
                return _usuarioRepository.Modificar(usuario);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el usuario: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID del usuario debe ser mayor que cero";
                }

                // Verificar que el usuario exista
                var usuarioExistente = _usuarioRepository.BuscarPorId(id);
                if (usuarioExistente == null)
                {
                    return $"No se encontró un usuario con ID {id}";
                }

                // Eliminar el usuario
                return _usuarioRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el usuario: {ex.Message}";
            }
        }

        public bool ValidarCredenciales(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("El email no puede estar vacío");
                }

                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentException("La contraseña no puede estar vacía");
                }

                return _usuarioRepository.ValidarCredenciales(email, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales: {ex.Message}");
            }
        }

        public string CambiarContrasena(int usuarioId, string nuevaContrasena)
        {
            try
            {
                if (usuarioId <= 0)
                {
                    return "El ID del usuario debe ser mayor que cero";
                }

                if (string.IsNullOrEmpty(nuevaContrasena))
                {
                    return "La nueva contraseña no puede estar vacía";
                }

                if (nuevaContrasena.Length < 6)
                {
                    return "La contraseña debe tener al menos 6 caracteres";
                }

                // Verificar que el usuario exista
                var usuarioExistente = _usuarioRepository.BuscarPorId(usuarioId);
                if (usuarioExistente == null)
                {
                    return $"No se encontró un usuario con ID {usuarioId}";
                }

                // Cambiar la contraseña
                return _usuarioRepository.CambiarContrasena(usuarioId, nuevaContrasena);
            }
            catch (Exception ex)
            {
                return $"Error al cambiar contraseña: {ex.Message}";
            }
        }

        private string ValidarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                return "El usuario no puede ser nulo";
            }

            if (string.IsNullOrEmpty(usuario.Nombre))
            {
                return "El nombre no puede estar vacío";
            }

            if (string.IsNullOrEmpty(usuario.Apellido))
            {
                return "El apellido no puede estar vacío";
            }

            if (string.IsNullOrEmpty(usuario.Email))
            {
                return "El email no puede estar vacío";
            }

            // Validar formato de email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(usuario.Email, emailPattern))
            {
                return "El formato del email no es válido";
            }

            if (string.IsNullOrEmpty(usuario.Password))
            {
                return "La contraseña no puede estar vacía";
            }

            if (usuario.Password.Length < 6)
            {
                return "La contraseña debe tener al menos 6 caracteres";
            }

            return null;
        }
    }
}