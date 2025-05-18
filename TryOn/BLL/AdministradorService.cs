using DAL;
using Entities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BLL
{
    public class AdministradorService : IService<Administrador>
    {
        private readonly AdministradorRepository _administradorRepository;
        private readonly UsuarioRepository _usuarioRepository;

        public AdministradorService()
        {
            _administradorRepository = new AdministradorRepository();
            _usuarioRepository = new UsuarioRepository();
        }

        public string Guardar(Administrador administrador)
        {
            try
            {
                // Validaciones
                var validacion = ValidarAdministrador(administrador);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                // Verificar si ya existe un usuario con el mismo email
                var usuarioExistente = _usuarioRepository.BuscarPorEmail(administrador.Email);
                if (usuarioExistente != null)
                {
                    return $"Ya existe un usuario con el email {administrador.Email}";
                }

                // Guardar el administrador
                return _administradorRepository.Guardar(administrador);
            }
            catch (Exception ex)
            {
                return $"Error al guardar el administrador: {ex.Message}";
            }
        }

        public List<Administrador> Consultar()
        {
            try
            {
                return _administradorRepository.Consultar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar administradores: {ex.Message}");
            }
        }

        public Administrador BuscarPorId(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del administrador debe ser mayor que cero");
                }

                return _administradorRepository.BuscarPorId(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar administrador: {ex.Message}");
            }
        }

        public Administrador BuscarPorEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("El email no puede estar vacío");
                }

                return _administradorRepository.BuscarPorEmail(email);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar administrador por email: {ex.Message}");
            }
        }

        public string Modificar(Administrador administrador)
        {
            try
            {
                // Validaciones
                var validacion = ValidarAdministrador(administrador);
                if (!string.IsNullOrEmpty(validacion))
                {
                    return validacion;
                }

                if (administrador.Id <= 0)
                {
                    return "El ID del administrador debe ser mayor que cero";
                }

                // Verificar que el administrador exista
                var administradorExistente = _administradorRepository.BuscarPorId(administrador.Id);
                if (administradorExistente == null)
                {
                    return $"No se encontró un administrador con ID {administrador.Id}";
                }

                // Verificar si el email ya está en uso por otro usuario
                var administradorPorEmail = _administradorRepository.BuscarPorEmail(administrador.Email);
                if (administradorPorEmail != null && administradorPorEmail.Id != administrador.Id)
                {
                    return $"El email {administrador.Email} ya está en uso por otro usuario";
                }

                // Modificar el administrador
                return _administradorRepository.Modificar(administrador);
            }
            catch (Exception ex)
            {
                return $"Error al modificar el administrador: {ex.Message}";
            }
        }

        public string Eliminar(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return "El ID del administrador debe ser mayor que cero";
                }

                // Verificar que el administrador exista
                var administradorExistente = _administradorRepository.BuscarPorId(id);
                if (administradorExistente == null)
                {
                    return $"No se encontró un administrador con ID {id}";
                }

                // Eliminar el administrador
                return _administradorRepository.Eliminar(id);
            }
            catch (Exception ex)
            {
                return $"Error al eliminar el administrador: {ex.Message}";
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

                return _administradorRepository.ValidarCredenciales(email, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales: {ex.Message}");
            }
        }

        private string ValidarAdministrador(Administrador administrador)
        {
            if (administrador == null)
            {
                return "El administrador no puede ser nulo";
            }

            if (string.IsNullOrEmpty(administrador.Nombre))
            {
                return "El nombre no puede estar vacío";
            }

            if (string.IsNullOrEmpty(administrador.Apellido))
            {
                return "El apellido no puede estar vacío";
            }

            if (string.IsNullOrEmpty(administrador.Email))
            {
                return "El email no puede estar vacío";
            }

            // Validar formato de email
            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(administrador.Email, emailPattern))
            {
                return "El formato del email no es válido";
            }

            if (string.IsNullOrEmpty(administrador.Contrasena))
            {
                return "La contraseña no puede estar vacía";
            }

            if (administrador.Contrasena.Length < 6)
            {
                return "La contraseña debe tener al menos 6 caracteres";
            }

            if (string.IsNullOrEmpty(administrador.Cargo))
            {
                return "El cargo no puede estar vacío";
            }

            if (string.IsNullOrEmpty(administrador.Departamento))
            {
                return "El departamento no puede estar vacío";
            }

            return null;
        }
    }
}