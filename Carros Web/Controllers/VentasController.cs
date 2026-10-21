using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Ventas")]
    public class VentasController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();
        private readonly VentaNegocio ventaNegocio = new VentaNegocio();

        // GET: Ventas
        public ActionResult Index()
        {
            var ventas = db.Ventas
                .Include(v => v.Vehiculo)
                .Include(v => v.Cliente)
                .Include(v => v.Forma_Pago)
                .Where(v => v.id_empresa == SesionActual.IdEmpresa)
                .OrderByDescending(v => v.fecha);

            return View(ventas.ToList());
        }

        // GET: Ventas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var venta = db.Ventas
                .Include(v => v.Vehiculo.Contrato.Consignante)
                .Include(v => v.Cliente)
                .Include(v => v.Forma_Pago)
                .Include(v => v.Liquidacions)
                .FirstOrDefault(v => v.id_venta == id && v.id_empresa == SesionActual.IdEmpresa);

            if (venta == null) return HttpNotFound();
            return View(venta);
        }

        // GET: Ventas/Create
        // idVehiculo permite llegar desde el catálogo con el vehículo ya elegido.
        public ActionResult Create(int? idVehiculo)
        {
            CargarListas(idVehiculo, null, null);
            return View();
        }

        // GET: Ventas/Comprobante/5  (versión imprimible de la venta)
        public ActionResult Comprobante(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var venta = db.Ventas
                .Include(v => v.Vehiculo.Catalogo_Marca)
                .Include(v => v.Vehiculo.Contrato.Consignante)
                .Include(v => v.Cliente)
                .Include(v => v.Forma_Pago)
                .FirstOrDefault(v => v.id_venta == id && v.id_empresa == SesionActual.IdEmpresa);

            if (venta == null) return HttpNotFound();
            return View(venta);
        }

        // POST: Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int idVehiculo, int idCliente, decimal precioFinal, int idFormaPago)
        {
            if (precioFinal <= 0)
                ModelState.AddModelError("", "El precio final debe ser mayor a cero.");

            if (ModelState.IsValid)
            {
                var resultado = ventaNegocio.Registrar(idVehiculo, idCliente, precioFinal, idFormaPago,
                    SesionActual.IdEmpresa, SesionActual.IdUsuario);

                if (!resultado.Exito)
                {
                    ModelState.AddModelError("", resultado.Mensaje);
                }
                else
                {
                    TempData["MensajeExito"] = "Venta registrada correctamente.";
                    return RedirectToAction("Comprobante", new { id = resultado.IdVenta });
                }
            }

            CargarListas(idVehiculo, idCliente, idFormaPago);
            return View();
        }

        private void CargarListas(int? idVehiculo, int? idCliente, int? idFormaPago)
        {
            var vehiculosDisponibles = db.Vehiculoes
                .Where(v => v.id_empresa == SesionActual.IdEmpresa && !v.Ventas.Any())
                .ToList()
                .Select(v => new { v.id_vehiculo, Texto = $"{v.placa} - {v.modelo}" });

            ViewBag.id_vehiculo = new SelectList(vehiculosDisponibles, "id_vehiculo", "Texto", idVehiculo);
            ViewBag.id_cliente = new SelectList(db.Clientes.Where(c => c.id_empresa == SesionActual.IdEmpresa), "id_cliente", "nombre", idCliente);
            ViewBag.id_forma_pago = new SelectList(db.Forma_Pago, "id_forma_pago", "descripcion", idFormaPago);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
