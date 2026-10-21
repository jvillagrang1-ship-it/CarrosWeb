using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso]
    public class HomeController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        /// <summary>
        /// El inicio del sistema es directamente el catálogo de vehículos
        /// disponibles (la vitrina), no un menú de opciones: para eso ya
        /// está la barra superior. Si el usuario no tiene permiso para ver
        /// Vehículos, se le muestra una bienvenida simple en su lugar.
        /// </summary>
        public ActionResult Index()
        {
            if (!SesionActual.TienePermiso("Vehiculos"))
                return View();

            return View("~/Views/Vehiculos/Catalogo.cshtml", VehiculosController.ConsultarDisponibles(db));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
