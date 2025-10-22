﻿using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class UsuarioAccesoRepositories
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }
        public UsuarioAccesoRepositories()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de movimientos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar tabla: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<UsuariosAccesoModel> GetAllUsuarios()
        {
            try
            {
                return Connection.Table<UsuariosAccesoModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al obtener usuarios: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<UsuariosAccesoModel>();
            }
        }

        public UsuariosAccesoModel GetUsuarioByNombreUsuario(string nombreUsuario)
        {
            try
            {
                var usuario = Connection.Table<UsuariosAccesoModel>()
                    .FirstOrDefault(u => u.Usuario == nombreUsuario);

                if (usuario != null)
                {
                    StatusMessage = "Usuario encontrado";
                }
                else
                {
                    StatusMessage = "Usuario no encontrado";
                }

                return usuario;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al buscar usuario: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public bool ValidarCredenciales(string nombreUsuario, string password)
        {
            try
            {
                var usuario = GetUsuarioByNombreUsuario(nombreUsuario);

                if (usuario == null)
                {
                    StatusMessage = "Usuario no encontrado";
                    return false;
                }

                // Comparar la contraseña
                if (usuario.Password == password)
                {
                    StatusMessage = "Credenciales válidas";
                    return true;
                }
                else
                {
                    StatusMessage = "Contraseña incorrecta";
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al validar credenciales: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool InsertarUsuario(UsuariosAccesoModel usuario)
        {
            try
            {
                if (usuario == null)
                {
                    StatusMessage = "El usuario no puede ser nulo";
                    return false;
                }

                int result = Connection.Insert(usuario);

                if (result > 0)
                {
                    StatusMessage = $"Usuario insertado correctamente. ID: {result}";
                    System.Diagnostics.Debug.WriteLine(StatusMessage);
                    return true;
                }
                else
                {
                    StatusMessage = "Error al insertar usuario";
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar usuario: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool ActualizarUsuario(UsuariosAccesoModel usuario)
        {
            try
            {
                if (usuario == null)
                {
                    StatusMessage = "El usuario no puede ser nulo";
                    return false;
                }

                int result = Connection.Update(usuario);

                if (result > 0)
                {
                    StatusMessage = "Usuario actualizado correctamente";
                    return true;
                }
                else
                {
                    StatusMessage = "Error al actualizar usuario";
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar usuario: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
        public bool EliminarUsuario(string idUsuario)
        {
            try
            {
                var usuario = Connection.Find<UsuariosAccesoModel>(idUsuario);

                if (usuario == null)
                {
                    StatusMessage = "Usuario no encontrado";
                    return false;
                }

                int result = Connection.Delete(usuario);

                if (result > 0)
                {
                    StatusMessage = "Usuario eliminado correctamente";
                    return true;
                }
                else
                {
                    StatusMessage = "Error al eliminar usuario";
                    return false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al eliminar usuario: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }
    }
}
