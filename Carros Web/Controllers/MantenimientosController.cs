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
    [RequierePermiso("Mantenimientos")]
    public class MantenimientosController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        public static readonly string[] Estados = { "En proceso", "En revisión", "Finalizado" };

        // GET: Mantenimientos
        public ActionResult Index(string estado)
        {
            var mantenimientos = db.Mantenimientoes
                .Include(m => m.Catalogo_TipoMantenimiento)
                .Include(m => m.Taller)
                .Include(m => m.Vehiculo)
                .Where(m => m.id_empresa == SesionActual.IdEmpresa);

            if (!string.IsNullOrWhiteSpace(estado))
                mantenimientos = mantenimientos.Where(m => m.estado == estado);

            ViewBag.estado = estado;
            ViewBag.Estados = Estados;

            return View(mantenimientos.OrderByDescending(m => m.fecha).ToList());
        }

        // GET: Mantenimientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mantenimiento = db.Mantenimientoes
                .Include(m => m.Catalogo_TipoMantenimiento)
                .Include(m => m.Taller)
                .Include(m => m.Vehiculo)
                .Include(m => m.Forma_Pago)
                .Include(m => m.Usuario)
                .FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);

            if (mantenimiento == null) return HttpNotFound();

            ViewBag.Estados = Estados;
            ViewBag.id_forma_pago = new SelectList(db.Forma_Pago, "id_forma_pago", "descripcion");
            return View(mantenimiento);
        }

        // GET: Mantenimientos/Create
        public ActionResult Create()
        {
            CargarListas(null);
            return View(new Mantenimiento { fecha = DateTime.Today });
        }

        // POST: Mantenimientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_vehiculo,placa_ingresada,marca_modelo_ingresado,propietario_nombre,propietario_telefono,id_tipo_mantenimiento,costo,id_taller,fecha,observaciones")] Mantenimiento mantenimiento)
        {
            if (ModelState.IsValid)
            {
                mantenimiento.id_empresa = SesionActual.IdEmpresa;
                mantenimiento.estado = Estados[0];
                mantenimiento.pagado = false;
                db.Mantenimientoes.Add(mantenimiento);
                db.SaveChanges();

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Mantenimiento", mantenimiento.id_mantenimiento);

                TempData["MensajeExito"] = "Mantenimiento ingresado.";
                return RedirectToAction("Details", new { id = mantenimiento.id_mantenimiento });
            }

            CargarListas(mantenimiento);
            return View(mantenimiento);
        }

        // GET: Mantenimientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();

            CargarListas(mantenimiento);
            return View(mantenimiento);
        }

        // POST: Mantenimientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_mantenimiento,id_vehiculo,placa_ingresada,marca_modelo_ingresado,propietario_nombre,propietario_telefono,id_tipo_mantenimiento,costo,id_taller,fecha,observaciones")] Mantenimiento mantenimiento)
        {
            var existente = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == mantenimiento.id_mantenimiento && m.id_empresa == SesionActual.IdEmpresa);
            if (existente == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                existente.id_vehiculo = mantenimiento.id_vehiculo;
                existente.placa_ingresada = mantenimiento.placa_ingresada;
                existente.marca_modelo_ingresado = mantenimiento.marca_modelo_ingresado;
                existente.propietario_nombre = mantenimiento.propietario_nombre;
                existente.propietario_telefono = mantenimiento.propietario_telefono;
                existente.id_tipo_mantenimiento = mantenimiento.id_tipo_mantenimiento;
                existente.costo = mantenimiento.costo;
                existente.id_taller = mantenimiento.id_taller;
                existente.fecha = mantenimiento.fecha;
                existente.observaciones = mantenimiento.observaciones;
                db.SaveChanges();

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Mantenimiento", existente.id_mantenimiento);

                TempData["MensajeExito"] = "Mantenimiento actualizado.";
                return RedirectToAction("Details", new { id = existente.id_mantenimiento });
            }

            CargarListas(mantenimiento);
            return View(mantenimiento);
        }

        // POST: Mantenimientos/CambiarEstado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int id, string nuevoEstado)
        {
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();

            if (!Estados.Contains(nuevoEstado))
            {
                TempData["MensajeError"] = "Estado no válido.";
                return RedirectToAction("Details", new { id });
            }

            string anterior = mantenimiento.estado;
            mantenimiento.estado = nuevoEstado;
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Mantenimiento", id, anterior, nuevoEstado);

            TempData["MensajeExito"] = $"Estado cambiado a '{nuevoEstado}'.";
            return RedirectToAction("Details", new { id });
        }

        // GET: Mantenimientos/Cobrar/5
        public ActionResult Cobrar(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();

            if (mantenimiento.estado != "Finalizado")
            {
                TempData["MensajeError"] = "Solo se puede cobrar un mantenimiento finalizado.";
                return RedirectToAction("Details", new { id });
            }

            ViewBag.id_forma_pago = new SelectList(db.Forma_Pago, "id_forma_pago", "descripcion");
            return View(mantenimiento);
        }

        // POST: Mantenimientos/Cobrar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cobrar(int id, int idFormaPago)
        {
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();

            if (mantenimiento.estado != "Finalizado")
            {
                TempData["MensajeError"] = "Solo se puede cobrar un mantenimiento finalizado.";
                return RedirectToAction("Details", new { id });
            }
            if (mantenimiento.pagado)
            {
                TempData["MensajeError"] = "Este mantenimiento ya fue cobrado.";
                return RedirectToAction("Details", new { id });
            }

            mantenimiento.pagado = true;
            mantenimiento.id_forma_pago = idFormaPago;
            mantenimiento.fecha_pago = DateTime.Now;
            mantenimiento.id_usuario_cobro = SesionActual.IdUsuario;
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Mantenimiento", id, null, "Cobrado");

            TempData["MensajeExito"] = "Cobro registrado.";
            return RedirectToAction("Comprobante", new { id });
        }

        // GET: Mantenimientos/Comprobante/5  (solo si el servicio ya fue cobrado)
        public ActionResult Comprobante(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var mantenimiento = db.Mantenimientoes
                .Include(m => m.Catalogo_TipoMantenimiento)
                .Include(m => m.Taller)
                .Include(m => m.Vehiculo.Catalogo_Marca)
                .Include(m => m.Forma_Pago)
                .Include(m => m.Usuario)
                .FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);

            if (mantenimiento == null || !mantenimiento.pagado) return HttpNotFound();
            return View(mantenimiento);
        }

        // GET: Mantenimientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();
            return View(mantenimiento);
        }

        // POST: Mantenimientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var mantenimiento = db.Mantenimientoes.FirstOrDefault(m => m.id_mantenimiento == id && m.id_empresa == SesionActual.IdEmpresa);
            if (mantenimiento == null) return HttpNotFound();

            db.Mantenimientoes.Remove(mantenimiento);
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "DELETE", "Mantenimiento", id);

            TempData["MensajeExito"] = "Mantenimiento eliminado.";
            return RedirectToAction("Index");
        }

        private void CargarListas(Mantenimiento mantenimiento)
        {
            ViewBag.id_tipo_mantenimiento = new SelectList(db.Catalogo_TipoMantenimiento, "id_tipo_mantenimiento", "descripcion", mantenimiento?.id_tipo_mantenimiento);
            ViewBag.id_taller = new SelectList(db.Tallers.Where(t => t.id_empresa == SesionActual.IdEmpresa), "id_taller", "nombre", mantenimiento?.id_taller);
            ViewBag.id_vehiculo = new SelectList(db.Vehiculoes.Where(v => v.id_empresa == SesionActual.IdEmpresa), "id_vehiculo", "placa", mantenimiento?.id_vehiculo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
