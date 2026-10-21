using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    /// <summary>
    /// Pantalla única de catálogos (tipos de vehículo, marcas, estados, tipos de
    /// mantenimiento y formas de pago): ver, agregar y eliminar valores sin salir de la página.
    /// Solo el Administrador puede agregar o eliminar.
    /// </summary>
    [RequierePermiso("Catalogos")]
    public class CatalogosController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        public ActionResult Index()
        {
            ViewBag.TiposVehiculo = db.Catalogo_TipoVehiculo.OrderBy(x => x.descripcion).ToList()
                .Select(x => new Tuple<int, string>(x.id_tipo, x.descripcion)).ToList();
            ViewBag.Marcas = db.Catalogo_Marca.OrderBy(x => x.descripcion).ToList()
                .Select(x => new Tuple<int, string>(x.id_marca, x.descripcion)).ToList();
            ViewBag.Estados = db.Catalogo_EstadoVehiculo.OrderBy(x => x.descripcion).ToList()
                .Select(x => new Tuple<int, string>(x.id_estado, x.descripcion)).ToList();
            ViewBag.TiposMantenimiento = db.Catalogo_TipoMantenimiento.OrderBy(x => x.descripcion).ToList()
                .Select(x => new Tuple<int, string>(x.id_tipo_mantenimiento, x.descripcion)).ToList();
            ViewBag.FormasPago = db.Forma_Pago.OrderBy(x => x.descripcion).ToList()
                .Select(x => new Tuple<int, string>(x.id_forma_pago, x.descripcion)).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(string clave, string descripcion)
        {
            if (SesionActual.NombreRol != "Administrador")
                return RedirectToAction("AccesoDenegado", "Cuenta");

            descripcion = (descripcion ?? "").Trim();
            if (descripcion.Length == 0)
            {
                TempData["MensajeError"] = "Escribe una descripción.";
                return RedirectToAction("Index");
            }

            int id;
            switch (clave)
            {
                case "tipovehiculo":
                    var tv = new Catalogo_TipoVehiculo { descripcion = descripcion };
                    db.Catalogo_TipoVehiculo.Add(tv); db.SaveChanges(); id = tv.id_tipo; break;
                case "marca":
                    var ma = new Catalogo_Marca { descripcion = descripcion };
                    db.Catalogo_Marca.Add(ma); db.SaveChanges(); id = ma.id_marca; break;
                case "estado":
                    var es = new Catalogo_EstadoVehiculo { descripcion = descripcion };
                    db.Catalogo_EstadoVehiculo.Add(es); db.SaveChanges(); id = es.id_estado; break;
                case "tipomantenimiento":
                    var tm = new Catalogo_TipoMantenimiento { descripcion = descripcion };
                    db.Catalogo_TipoMantenimiento.Add(tm); db.SaveChanges(); id = tm.id_tipo_mantenimiento; break;
                case "formapago":
                    var fp = new Forma_Pago { descripcion = descripcion };
                    db.Forma_Pago.Add(fp); db.SaveChanges(); id = fp.id_forma_pago; break;
                default:
                    return HttpNotFound();
            }

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Catalogo_" + clave, id, null, descripcion);
            TempData["MensajeExito"] = "Valor agregado al catálogo.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(string clave, int id)
        {
            if (SesionActual.NombreRol != "Administrador")
                return RedirectToAction("AccesoDenegado", "Cuenta");

            try
            {
                switch (clave)
                {
                    case "tipovehiculo": db.Catalogo_TipoVehiculo.Remove(db.Catalogo_TipoVehiculo.Find(id)); break;
                    case "marca": db.Catalogo_Marca.Remove(db.Catalogo_Marca.Find(id)); break;
                    case "estado": db.Catalogo_EstadoVehiculo.Remove(db.Catalogo_EstadoVehiculo.Find(id)); break;
                    case "tipomantenimiento": db.Catalogo_TipoMantenimiento.Remove(db.Catalogo_TipoMantenimiento.Find(id)); break;
                    case "formapago": db.Forma_Pago.Remove(db.Forma_Pago.Find(id)); break;
                    default: return HttpNotFound();
                }
                db.SaveChanges();
                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "DELETE", "Catalogo_" + clave, id);
                TempData["MensajeExito"] = "Valor eliminado.";
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "No se puede eliminar: ya está en uso en vehículos, mantenimientos o ventas.";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
