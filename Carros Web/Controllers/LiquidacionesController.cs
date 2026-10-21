using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CarrosWeb.Infraestructura;
using CarrosWeb.Negocio;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Ventas")]
    public class LiquidacionesController : Controller
    {
        private Models.ConcesionariaWEBEntities db = new Models.ConcesionariaWEBEntities();

        // GET: Liquidaciones
        public ActionResult Index()
        {
            var liquidaciones = db.Liquidacions
                .Include(l => l.Venta.Vehiculo.Contrato.Consignante)
                .Where(l => l.id_empresa == SesionActual.IdEmpresa)
                .OrderBy(l => l.estado == "Pendiente" ? 0 : 1)
                .ThenByDescending(l => l.id_liquidacion);

            return View(liquidaciones.ToList());
        }

        // POST: Liquidaciones/MarcarPagada/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarPagada(int id)
        {
            // Pagar a un consignante es una decisión del Administrador.
            if (SesionActual.NombreRol != "Administrador")
                return RedirectToAction("AccesoDenegado", "Cuenta");

            var liquidacion = db.Liquidacions.FirstOrDefault(l => l.id_liquidacion == id && l.id_empresa == SesionActual.IdEmpresa);
            if (liquidacion == null) return HttpNotFound();

            if (liquidacion.estado == "Pagada")
            {
                TempData["MensajeError"] = "Esta liquidación ya estaba pagada.";
                return RedirectToAction("Index");
            }

            liquidacion.estado = "Pagada";
            liquidacion.fecha_pago = DateTime.Now;
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Liquidacion", id, "Pendiente", "Pagada");

            TempData["MensajeExito"] = "Liquidación al consignante marcada como pagada.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
