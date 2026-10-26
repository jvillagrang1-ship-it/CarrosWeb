using Carros_Web.Infraestructura;
using Carros_Web.Models;
using Carros_Web.Negocio;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Consignantes")]
    public class ContratosController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Contratos
        public ActionResult Index(int? idConsignante)
        {
            var contratos = db.Contratoes.Include(c => c.Consignante)
                                          .Where(c => c.id_empresa == SesionActual.IdEmpresa);

            if (idConsignante.HasValue)
                contratos = contratos.Where(c => c.id_consignante == idConsignante);

            ViewBag.IdConsignante = idConsignante;
            return View(contratos.OrderByDescending(c => c.fecha_inicio).ToList());
        }

        // GET: Contratos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var contrato = db.Contratoes.Include(c => c.Consignante).Include(c => c.Vehiculoes)
                                         .FirstOrDefault(c => c.id_contrato == id && c.id_empresa == SesionActual.IdEmpresa);
            if (contrato == null) return HttpNotFound();
            return View(contrato);
        }

        // GET: Contratos/Create
        public ActionResult Create(int? idConsignante)
        {
            ViewBag.id_consignante = new SelectList(
                db.Consignantes.Where(c => c.id_empresa == SesionActual.IdEmpresa), "id_consignante", "nombre", idConsignante);

            var empresa = db.Empresas.Find(SesionActual.IdEmpresa);
            var contrato = new Contrato
            {
                id_consignante = idConsignante,
                fecha_inicio = DateTime.Today,
                fecha_fin = DateTime.Today.AddMonths(3),
                porcentaje_comision = empresa?.porcentaje_comision_default ?? 10m
            };
            return View(contrato);
        }

        // POST: Contratos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_consignante,precio_pactado,porcentaje_comision,fecha_inicio,fecha_fin")] Contrato contrato)
        {
            if (contrato.fecha_fin <= contrato.fecha_inicio)
                ModelState.AddModelError("fecha_fin", "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (ModelState.IsValid)
            {
                contrato.id_empresa = SesionActual.IdEmpresa;
                db.Contratoes.Add(contrato);
                db.SaveChanges();

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Contrato", contrato.id_contrato);

                TempData["MensajeExito"] = "Contrato de consignación creado.";
                return RedirectToAction("Index", new { idConsignante = contrato.id_consignante });
            }

            ViewBag.id_consignante = new SelectList(
                db.Consignantes.Where(c => c.id_empresa == SesionActual.IdEmpresa), "id_consignante", "nombre", contrato.id_consignante);
            return View(contrato);
        }

        // GET: Contratos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var contrato = db.Contratoes.FirstOrDefault(c => c.id_contrato == id && c.id_empresa == SesionActual.IdEmpresa);
            if (contrato == null) return HttpNotFound();

            ViewBag.id_consignante = new SelectList(
                db.Consignantes.Where(c => c.id_empresa == SesionActual.IdEmpresa), "id_consignante", "nombre", contrato.id_consignante);
            return View(contrato);
        }

        // POST: Contratos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_contrato,id_consignante,precio_pactado,porcentaje_comision,fecha_inicio,fecha_fin")] Contrato contrato)
        {
            var existente = db.Contratoes.FirstOrDefault(c => c.id_contrato == contrato.id_contrato && c.id_empresa == SesionActual.IdEmpresa);
            if (existente == null) return HttpNotFound();

            if (contrato.fecha_fin <= contrato.fecha_inicio)
                ModelState.AddModelError("fecha_fin", "La fecha de fin debe ser posterior a la fecha de inicio.");

            if (ModelState.IsValid)
            {
                existente.id_consignante = contrato.id_consignante;
                existente.precio_pactado = contrato.precio_pactado;
                existente.porcentaje_comision = contrato.porcentaje_comision;
                existente.fecha_inicio = contrato.fecha_inicio;
                existente.fecha_fin = contrato.fecha_fin;
                db.SaveChanges();

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Contrato", contrato.id_contrato);

                TempData["MensajeExito"] = "Contrato actualizado.";
                return RedirectToAction("Index", new { idConsignante = existente.id_consignante });
            }

            ViewBag.id_consignante = new SelectList(
                db.Consignantes.Where(c => c.id_empresa == SesionActual.IdEmpresa), "id_consignante", "nombre", contrato.id_consignante);
            return View(contrato);
        }

        // GET: Contratos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var contrato = db.Contratoes.Include(c => c.Consignante)
                                         .FirstOrDefault(c => c.id_contrato == id && c.id_empresa == SesionActual.IdEmpresa);
            if (contrato == null) return HttpNotFound();
            return View(contrato);
        }

        // POST: Contratos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var contrato = db.Contratoes.FirstOrDefault(c => c.id_contrato == id && c.id_empresa == SesionActual.IdEmpresa);
            if (contrato == null) return HttpNotFound();

            if (contrato.Vehiculoes.Any())
            {
                TempData["MensajeError"] = "No se puede eliminar: el contrato tiene vehículos asociados.";
                return RedirectToAction("Index", new { idConsignante = contrato.id_consignante });
            }

            db.Contratoes.Remove(contrato);
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "DELETE", "Contrato", id);

            TempData["MensajeExito"] = "Contrato eliminado.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
