using Carros_Web.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Carros_Web.Negocio
{
    public class ResultadoLogin
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public Usuario Usuario { get; set; }
        public string NombreRol { get; set; }
        public List<string> Permisos { get; set; } = new List<string>();
    }

    public class ResultadoOperacion
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
    }

    public class UsuarioNegocio
    {
        /// <summary>Módulos/pantallas del sistema (catálogo de permisos).</summary>
        public static readonly string[] Modulos =
        {
            "Empresa", "Usuarios", "Bitacora", "Catalogos", "Mantenimientos",
            "Consignantes", "Vehiculos", "Clientes", "Ventas", "Reportes"
        };

        private static readonly string[] PermisosVendedor =
        {
            "Catalogos", "Mantenimientos", "Consignantes", "Vehiculos", "Clientes", "Ventas", "Reportes"
        };

        public bool ExisteAlgunUsuario()
        {
            using (var db = new ConcesionariaWEBEntities())
                return db.Usuarios.Any();
        }

        /// <summary>
        /// Garantiza que existan la empresa, los roles (Administrador/Vendedor)
        /// y el catálogo de permisos, asignando a cada rol los que le
        /// corresponden. Se llama antes de crear el primer usuario para que
        /// el sistema quede funcional sin tener que correr SQL a mano.
        /// </summary>
        private void AsegurarDatosBase(ConcesionariaWEBEntities db, out Empresa empresa, out Rol rolAdmin)
        {
            empresa = db.Empresas.FirstOrDefault();
            if (empresa == null)
            {
                empresa = new Empresa { nombre = "Mi Concesionaria", moneda = "GTQ", porcentaje_comision_default = 10m };
                db.Empresas.Add(empresa);
                db.SaveChanges();
            }

            rolAdmin = db.Rols.FirstOrDefault(r => r.nombre_rol == "Administrador");
            if (rolAdmin == null)
            {
                rolAdmin = new Rol { nombre_rol = "Administrador", id_empresa = empresa.id_empresa };
                db.Rols.Add(rolAdmin);
                db.SaveChanges();
            }

            var rolVendedor = db.Rols.FirstOrDefault(r => r.nombre_rol == "Vendedor");
            if (rolVendedor == null)
            {
                rolVendedor = new Rol { nombre_rol = "Vendedor", id_empresa = empresa.id_empresa };
                db.Rols.Add(rolVendedor);
                db.SaveChanges();
            }

            foreach (var nombreModulo in Modulos)
            {
                if (!db.Permisoes.Any(p => p.descripcion == nombreModulo))
                {
                    db.Permisoes.Add(new Permiso { descripcion = nombreModulo });
                }
            }
            db.SaveChanges();

            var permisos = db.Permisoes.ToList();

            foreach (var permiso in permisos)
            {
                if (!rolAdmin.Permisoes.Any(p => p.id_permiso == permiso.id_permiso))
                    rolAdmin.Permisoes.Add(permiso);
            }

            rolVendedor.Permisoes = rolVendedor.Permisoes ?? new List<Permiso>();
            foreach (var permiso in permisos.Where(p => PermisosVendedor.Contains(p.descripcion)))
            {
                if (!rolVendedor.Permisoes.Any(p => p.id_permiso == permiso.id_permiso))
                    rolVendedor.Permisoes.Add(permiso);
            }

            db.SaveChanges();
        }

        public ResultadoOperacion CrearAdminInicial(string nombre, string correo, string contrasenaPlano)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                if (db.Usuarios.Any())
                    return new ResultadoOperacion { Exito = false, Mensaje = "Ya existe al menos un usuario." };

                AsegurarDatosBase(db, out Empresa empresa, out Rol rolAdmin);

                string salt = SeguridadHelper.GenerarSalt();
                string hash = SeguridadHelper.CalcularHash(contrasenaPlano, salt);

                var usuario = new Usuario
                {
                    nombre = nombre,
                    correo = correo,
                    contraseña = hash,
                    salt = salt,
                    id_rol = rolAdmin.id_rol,
                    id_empresa = empresa.id_empresa,
                    activo = true
                };
                db.Usuarios.Add(usuario);
                db.SaveChanges();

                new BitacoraNegocio().Registrar(usuario.id_usuario, empresa.id_empresa, "INSERT", "Usuario",
                    usuario.id_usuario, null, $"Nombre={nombre}; Correo={correo}; Rol=Administrador");

                return new ResultadoOperacion { Exito = true };
            }
        }

        public ResultadoLogin IniciarSesion(string correo, string contrasenaPlano)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.correo == correo);

                if (usuario == null || string.IsNullOrEmpty(usuario.salt))
                {
                    new BitacoraNegocio().Registrar(0, 0, "LOGIN_FALLIDO", "Usuario", null, null, correo);
                    return new ResultadoLogin { Exito = false, Mensaje = "Correo o contraseña incorrectos." };
                }

                if (!usuario.activo)
                    return new ResultadoLogin { Exito = false, Mensaje = "Este usuario está inactivo." };

                if (!SeguridadHelper.Verificar(contrasenaPlano, usuario.salt, usuario.contraseña))
                {
                    new BitacoraNegocio().Registrar(usuario.id_usuario, usuario.id_empresa ?? 0, "LOGIN_FALLIDO", "Usuario", usuario.id_usuario);
                    return new ResultadoLogin { Exito = false, Mensaje = "Correo o contraseña incorrectos." };
                }

                string nombreRol = db.Rols.Where(r => r.id_rol == usuario.id_rol)
                                           .Select(r => r.nombre_rol)
                                           .FirstOrDefault() ?? "";

                var permisos = db.Rols.Where(r => r.id_rol == usuario.id_rol)
                                       .SelectMany(r => r.Permisoes.Select(p => p.descripcion))
                                       .ToList();

                new BitacoraNegocio().Registrar(usuario.id_usuario, usuario.id_empresa ?? 0, "LOGIN_EXITOSO", "Usuario", usuario.id_usuario);

                return new ResultadoLogin { Exito = true, Usuario = usuario, NombreRol = nombreRol, Permisos = permisos };
            }
        }

        public ResultadoOperacion CrearUsuario(string nombre, string correo, string contrasenaPlano, int idRol,
                                                int idEmpresa, int idUsuarioQueCrea)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                if (db.Usuarios.Any(u => u.correo == correo))
                    return new ResultadoOperacion { Exito = false, Mensaje = "Ya existe un usuario con ese correo." };

                string salt = SeguridadHelper.GenerarSalt();
                string hash = SeguridadHelper.CalcularHash(contrasenaPlano, salt);

                var usuario = new Usuario
                {
                    nombre = nombre,
                    correo = correo,
                    contraseña = hash,
                    salt = salt,
                    id_rol = idRol,
                    id_empresa = idEmpresa,
                    activo = true
                };
                db.Usuarios.Add(usuario);
                db.SaveChanges();

                new BitacoraNegocio().Registrar(idUsuarioQueCrea, idEmpresa, "INSERT", "Usuario", usuario.id_usuario,
                    null, $"Nombre={nombre}; Correo={correo}; id_rol={idRol}");

                return new ResultadoOperacion { Exito = true };
            }
        }

        public List<Usuario> ListarPorEmpresa(int idEmpresa)
        {
            using (var db = new ConcesionariaWEBEntities())
                return db.Usuarios.Include(u => u.Rol).Where(u => u.id_empresa == idEmpresa).OrderBy(u => u.nombre).ToList();
        }

        public void CambiarActivo(int idUsuario, bool activo, int idUsuarioQueModifica, int idEmpresa)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                var usuario = db.Usuarios.Find(idUsuario);
                if (usuario == null) return;
                usuario.activo = activo;
                db.SaveChanges();
            }
            new BitacoraNegocio().Registrar(idUsuarioQueModifica, idEmpresa, "UPDATE", "Usuario", idUsuario, null, $"activo={activo}");
        }

        public void CambiarContrasena(int idUsuario, string nuevaContrasenaPlano, int idUsuarioQueModifica, int idEmpresa)
        {
            string salt = SeguridadHelper.GenerarSalt();
            string hash = SeguridadHelper.CalcularHash(nuevaContrasenaPlano, salt);
            using (var db = new ConcesionariaWEBEntities())
            {
                var usuario = db.Usuarios.Find(idUsuario);
                if (usuario == null) return;
                usuario.salt = salt;
                usuario.contraseña = hash;
                db.SaveChanges();
            }
            new BitacoraNegocio().Registrar(idUsuarioQueModifica, idEmpresa, "UPDATE", "Usuario", idUsuario, null, "Contraseña actualizada");
        }
    }
}
