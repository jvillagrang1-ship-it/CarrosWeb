using Carros_Web.Infraestructura;
using Carros_Web.Models;
using Carros_Web.Negocio;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Usuarios")]
    public class UsuariosController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();
        private readonly UsuarioNegocio usuarioNegocio = new UsuarioNegocio();

        // GET: Usuarios
        public ActionResult Index()
        {
            var usuarios = usuarioNegocio.ListarPorEmpresa(SesionActual.IdEmpresa);
            ViewBag.Roles = db.Rols.Where(r => r.id_empresa == SesionActual.IdEmpresa).OrderBy(r => r.id_rol).ToList();
            ViewBag.IdUsuarioActual = SesionActual.IdUsuario;
            return View(usuarios);
        }

        // GET: Usuarios/Create
        public ActionResult Create()
        {
            ViewBag.id_rol = new SelectList(db.Rols.Where(r => r.id_empresa == SesionActual.IdEmpresa), "id_rol", "nombre_rol");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string nombre, string correo, string contrasena, int idRol)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
                ModelState.AddModelError("", "Todos los campos son obligatorios.");

            if (ModelState.IsValid)
            {
                var resultado = usuarioNegocio.CrearUsuario(nombre, correo, contrasena, idRol, SesionActual.IdEmpresa, SesionActual.IdUsuario);
                if (resultado.Exito)
                    TempData["MensajeExito"] = "Usuario creado correctamente.";
                else
                    TempData["MensajeError"] = resultado.Mensaje;
            }
            else
            {
                TempData["MensajeError"] = "Nombre, correo y contraseña son obligatorios.";
            }

            return RedirectToAction("Index");
        }

        // POST: Usuarios/CambiarActivo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarActivo(int id, bool activo)
        {
            if (id == SesionActual.IdUsuario)
            {
                TempData["MensajeError"] = "No puedes desactivar tu propia cuenta.";
                return RedirectToAction("Index");
            }

            usuarioNegocio.CambiarActivo(id, activo, SesionActual.IdUsuario, SesionActual.IdEmpresa);
            TempData["MensajeExito"] = activo ? "Usuario activado." : "Usuario desactivado.";
            return RedirectToAction("Index");
        }

        // GET: Usuarios/CambiarContrasena/5
        public ActionResult CambiarContrasena(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var usuario = db.Usuarios.FirstOrDefault(u => u.id_usuario == id && u.id_empresa == SesionActual.IdEmpresa);
            if (usuario == null) return HttpNotFound();
            return View(usuario);
        }

        // POST: Usuarios/CambiarContrasena/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena(int id, string nuevaContrasena)
        {
            if (string.IsNullOrWhiteSpace(nuevaContrasena) || nuevaContrasena.Length < 4)
            {
                TempData["MensajeError"] = "La nueva contraseña debe tener al menos 4 caracteres.";
                return RedirectToAction("Index");
            }

            usuarioNegocio.CambiarContrasena(id, nuevaContrasena, SesionActual.IdUsuario, SesionActual.IdEmpresa);
            TempData["MensajeExito"] = "Contraseña actualizada correctamente.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
