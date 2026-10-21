using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Reportes")]
    public class ReportesController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // El módulo abre directamente en el inventario; los demás reportes están en las pestañas.
        public ActionResult Index()
        {
            return RedirectToAction("Inventario");
        }

        // GET: Reportes/Inventario
        public ActionResult Inventario()
        {
            var vehiculos = db.Vehiculoes
                .Include(v => v.Catalogo_EstadoVehiculo)
                .Include(v => v.Catalogo_Marca)
                .Include(v => v.Catalogo_TipoVehiculo)
                .Include(v => v.Contrato.Consignante)
                .Where(v => v.id_empresa == SesionActual.IdEmpresa
                         && v.Catalogo_EstadoVehiculo.descripcion == "Disponible"
                         && !v.Ventas.Any())
                .ToList();

            RegistrarGeneracion("Inventario");
            return View(vehiculos);
        }

        // GET: Reportes/VentasComisiones
        public ActionResult VentasComisiones(DateTime? desde, DateTime? hasta)
        {
            var ventas = db.Ventas.Include(v => v.Vehiculo).Include(v => v.Cliente)
                                   .Where(v => v.id_empresa == SesionActual.IdEmpresa);

            if (desde.HasValue)
                ventas = ventas.Where(v => v.fecha >= desde.Value);
            if (hasta.HasValue)
            {
                var hastaFin = hasta.Value.Date.AddDays(1);
                ventas = ventas.Where(v => v.fecha < hastaFin);
            }

            var lista = ventas.OrderByDescending(v => v.fecha).ToList();

            ViewBag.TotalVentas = lista.Sum(v => v.precio_final);
            ViewBag.TotalComisiones = lista.Sum(v => v.comision);
            ViewBag.TotalLiquidado = lista.Sum(v => v.liquidacion_consignante);
            ViewBag.desde = desde;
            ViewBag.hasta = hasta;

            RegistrarGeneracion("VentasComisiones");
            return View(lista);
        }

        // GET: Reportes/Liquidaciones
        public ActionResult Liquidaciones()
        {
            var liquidaciones = db.Liquidacions
                .Include(l => l.Venta.Vehiculo.Contrato.Consignante)
                .Include(l => l.Venta.Vehiculo.Catalogo_Marca)
                .Where(l => l.id_empresa == SesionActual.IdEmpresa && l.estado == "Pendiente")
                .OrderBy(l => l.id_liquidacion)
                .ToList();

            ViewBag.TotalPendiente = liquidaciones.Where(l => l.estado == "Pendiente").Sum(l => l.monto_liquidado ?? 0);
            ViewBag.TotalPagado = liquidaciones.Where(l => l.estado == "Pagada").Sum(l => l.monto_liquidado ?? 0);

            RegistrarGeneracion("Liquidaciones");
            return View(liquidaciones);
        }

        // GET: Reportes/MantenimientosCobrados
        // Servicios de taller ya cobrados (vehículos del inventario y externos).
        public ActionResult MantenimientosCobrados(DateTime? desde, DateTime? hasta)
        {
            var servicios = db.Mantenimientoes
                .Include(m => m.Vehiculo.Catalogo_Marca)
                .Include(m => m.Catalogo_TipoMantenimiento)
                .Include(m => m.Forma_Pago)
                .Include(m => m.Usuario)
                .Where(m => m.id_empresa == SesionActual.IdEmpresa && m.pagado);

            if (desde.HasValue)
                servicios = servicios.Where(m => m.fecha_pago >= desde.Value);
            if (hasta.HasValue)
            {
                var hastaFin = hasta.Value.Date.AddDays(1);
                servicios = servicios.Where(m => m.fecha_pago < hastaFin);
            }

            var lista = servicios.OrderByDescending(m => m.fecha_pago).ToList();

            ViewBag.TotalCobrado = lista.Sum(m => m.costo ?? 0m);
            ViewBag.desde = desde;
            ViewBag.hasta = hasta;

            RegistrarGeneracion("MantenimientosCobrados");
            return View(lista);
        }

        private void RegistrarGeneracion(string tipoReporte)
        {
            db.Reportes.Add(new Reporte
            {
                tipo_reporte = tipoReporte,
                fecha_generacion = DateTime.Now,
                id_usuario = SesionActual.IdUsuario,
                id_empresa = SesionActual.IdEmpresa
            });
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
