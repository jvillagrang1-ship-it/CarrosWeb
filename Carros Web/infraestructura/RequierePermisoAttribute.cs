using System.Web.Mvc;
using System.Web.Routing;

namespace Carros_Web.Infraestructura
{
    /// <summary>
    /// Filtro de autorización basado en Session (en vez de cookies/claims):
    /// exige que haya una sesión iniciada y, si se indica un módulo, que el
    /// rol tenga ese permiso asignado (Administrador siempre pasa, como red
    /// de seguridad para no bloquearse a sí mismo).
    /// Uso: [RequierePermiso] solo exige sesión iniciada.
    ///      [RequierePermiso("Vehiculos")] exige además ese permiso.
    /// </summary>
    public class RequierePermisoAttribute : ActionFilterAttribute
    {
        private readonly string modulo;

        public RequierePermisoAttribute(string modulo = null)
        {
            this.modulo = modulo;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!SesionActual.HaIniciadoSesion)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Cuenta", action = "Login" }));
                return;
            }

            if (modulo != null && !SesionActual.TienePermiso(modulo))
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Cuenta", action = "AccesoDenegado" }));
            }
        }
    }
}
