using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Usuarios")]
    public class RolesController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Roles
        public ActionResult Index()
        {
            // La pantalla de permisos abre en el primer rol; los demás roles son pestañas.
            var primero = db.Rols.Where(r => r.id_empresa == SesionActual.IdEmpresa).OrderBy(r => r.id_rol).FirstOrDefault();
            if (primero == null) return RedirectToAction("Create");
            return RedirectToAction("Permisos", new { id = primero.id_rol });
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string nombreRol)
        {
            if (string.IsNullOrWhiteSpace(nombreRol))
            {
                ModelState.AddModelError("", "El nombre del rol es obligatorio.");
                return View();
            }

            var rol = new Rol { nombre_rol = nombreRol, id_empresa = SesionActual.IdEmpresa };
            db.Rols.Add(rol);
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Rol", rol.id_rol, null, nombreRol);

            TempData["MensajeExito"] = "Rol creado.";
            return RedirectToAction("Permisos", new { id = rol.id_rol });
        }

        // GET: Roles/Permisos/5
        public ActionResult Permisos(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var rol = db.Rols.FirstOrDefault(r => r.id_rol == id && r.id_empresa == SesionActual.IdEmpresa);
            if (rol == null) return HttpNotFound();

            foreach (var modulo in UsuarioNegocio.Modulos)
            {
                if (!db.Permisoes.Any(p => p.descripcion == modulo))
                    db.Permisoes.Add(new Permiso { descripcion = modulo });
            }
            db.SaveChanges();

            ViewBag.Roles = db.Rols.Where(r => r.id_empresa == SesionActual.IdEmpresa).OrderBy(r => r.id_rol).ToList();
            ViewBag.Modulos = UsuarioNegocio.Modulos;
            ViewBag.PermisosAsignados = rol.Permisoes.Select(p => p.descripcion).ToList();
            return View(rol);
        }

        // POST: Roles/Permisos/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Permisos(int id, string[] modulosSeleccionados)
        {
            var rol = db.Rols.FirstOrDefault(r => r.id_rol == id && r.id_empresa == SesionActual.IdEmpresa);
            if (rol == null) return HttpNotFound();

            modulosSeleccionados = modulosSeleccionados ?? new string[0];
            var permisos = db.Permisoes.ToList();

            foreach (var permiso in permisos)
            {
                bool debeTener = modulosSeleccionados.Contains(permiso.descripcion);
                bool yaLoTiene = rol.Permisoes.Any(p => p.id_permiso == permiso.id_permiso);

                if (debeTener && !yaLoTiene)
                    rol.Permisoes.Add(permiso);
                else if (!debeTener && yaLoTiene)
                    rol.Permisoes.Remove(permiso);
            }
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Rol_Permiso", id,
                null, string.Join(",", modulosSeleccionados));

            TempData["MensajeExito"] = "Permisos del rol actualizados.";
            return RedirectToAction("Permisos", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
