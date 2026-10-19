using System.Web.Mvc;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;

namespace CarrosWeb.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UsuarioNegocio usuarioNegocio = new UsuarioNegocio();

        [HttpGet]
        public ActionResult Login()
        {
            if (SesionActual.HaIniciadoSesion)
                return RedirectToAction("Index", "Home");

            if (!usuarioNegocio.ExisteAlgunUsuario())
                return RedirectToAction("CrearAdminInicial");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = usuarioNegocio.IniciarSesion(modelo.Correo, modelo.Contrasena);
            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(modelo);
            }

            var usuario = resultado.Usuario;
            SesionActual.Iniciar(usuario.id_usuario, usuario.nombre, usuario.id_rol ?? 0,
                resultado.NombreRol, usuario.id_empresa ?? 0, resultado.Permisos);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            SesionActual.Cerrar();
            return RedirectToAction("Login");
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }

        [HttpGet]
        public ActionResult CrearAdminInicial()
        {
            if (usuarioNegocio.ExisteAlgunUsuario())
                return RedirectToAction("Login");

            return View(new CrearAdminInicialViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearAdminInicial(CrearAdminInicialViewModel modelo)
        {
            if (usuarioNegocio.ExisteAlgunUsuario())
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = usuarioNegocio.CrearAdminInicial(modelo.Nombre, modelo.Correo, modelo.Contrasena);
            if (!resultado.Exito)
            {
                ModelState.AddModelError("", resultado.Mensaje);
                return View(modelo);
            }

            TempData["MensajeExito"] = "Usuario administrador creado. Ya puedes iniciar sesión.";
            return RedirectToAction("Login");
        }
    }
}
