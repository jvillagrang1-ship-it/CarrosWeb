using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Bitacora")]
    public class BitacoraController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Bitacora
        public ActionResult Index(DateTime? desde, DateTime? hasta, int? idUsuario, string entidad, string accion)
        {
            var registros = db.Bitacoras.Include(b => b.Usuario)
                                          .Where(b => b.id_empresa == SesionActual.IdEmpresa);

            if (desde.HasValue)
                registros = registros.Where(b => b.fecha_hora >= desde.Value);

            if (hasta.HasValue)
            {
                var hastaFin = hasta.Value.Date.AddDays(1);
                registros = registros.Where(b => b.fecha_hora < hastaFin);
            }

            if (idUsuario.HasValue)
                registros = registros.Where(b => b.id_usuario == idUsuario);

            if (!string.IsNullOrWhiteSpace(entidad))
                registros = registros.Where(b => b.entidad_afectada == entidad);

            if (!string.IsNullOrWhiteSpace(accion))
                registros = registros.Where(b => b.accion == accion);

            ViewBag.id_usuario = new SelectList(db.Usuarios.Where(u => u.id_empresa == SesionActual.IdEmpresa), "id_usuario", "nombre", idUsuario);
            ViewBag.entidades = db.Bitacoras.Where(b => b.id_empresa == SesionActual.IdEmpresa)
                                             .Select(b => b.entidad_afectada).Distinct().OrderBy(e => e).ToList();
            ViewBag.desde = desde;
            ViewBag.hasta = hasta;
            ViewBag.entidad = entidad;
            ViewBag.accion = accion;
            ViewBag.acciones = new[] { "INSERT", "UPDATE", "DELETE", "LOGIN" };

            return View(registros.OrderByDescending(b => b.fecha_hora).Take(500).ToList());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
