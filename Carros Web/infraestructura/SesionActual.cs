using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CarrosWeb.Infraestructura
{
    /// <summary>
    /// Envuelve la Session de ASP.NET para guardar/leer los datos del
    /// usuario logueado (equivalente, en esta versión sin OWIN/Identity, a
    /// las claims que usábamos en las otras versiones del sistema).
    /// </summary>
    public static class SesionActual
    {
        private static HttpSessionStateBase Session =>
            new HttpSessionStateWrapper(HttpContext.Current.Session);

        public static bool HaIniciadoSesion => Session["IdUsuario"] != null;

        public static int IdUsuario => (int)(Session["IdUsuario"] ?? 0);
        public static string NombreUsuario => (string)Session["NombreUsuario"];
        public static int IdRol => (int)(Session["IdRol"] ?? 0);
        public static string NombreRol => (string)Session["NombreRol"];
        public static int IdEmpresa => (int)(Session["IdEmpresa"] ?? 0);

        public static List<string> Permisos =>
            (Session["Permisos"] as List<string>) ?? new List<string>();

        public static bool TienePermiso(string modulo)
        {
            if (!HaIniciadoSesion) return false;
            if (NombreRol == "Administrador") return true;
            return Permisos.Contains(modulo);
        }

        public static void Iniciar(int idUsuario, string nombre, int idRol, string nombreRol,
                                    int idEmpresa, IEnumerable<string> permisos)
        {
            Session["IdUsuario"] = idUsuario;
            Session["NombreUsuario"] = nombre;
            Session["IdRol"] = idRol;
            Session["NombreRol"] = nombreRol;
            Session["IdEmpresa"] = idEmpresa;
            Session["Permisos"] = permisos.ToList();
        }

        public static void Cerrar()
        {
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }
    }
}
