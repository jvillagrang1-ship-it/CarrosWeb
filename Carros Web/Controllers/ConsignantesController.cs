
using Carros_Web.Infraestructura;
using Carros_Web.Models;
using Carros_Web.Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Consignantes")]
    public class ConsignantesController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Consignantes
        public ActionResult Index()
        {
            int idEmpresa = SesionActual.IdEmpresa;
            ViewBag.Contratos = db.Contratoes.Include(c => c.Consignante)
                                              .Where(c => c.id_empresa == idEmpresa)
                                              .OrderByDescending(c => c.fecha_fin).ToList();
            return View(db.Consignantes.Where(c => c.id_empresa == idEmpresa).OrderBy(c => c.nombre).ToList());
        }

        // GET: Consignantes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consignante consignante = db.Consignantes.Find(id);
            if (consignante == null)
            {
                return HttpNotFound();
            }
            return View(consignante);
        }

        // GET: Consignantes/Create
        public ActionResult Create()
        {
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre");
            return View();
        }

        // POST: Consignantes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_consignante,nombre,DPI,telefono,direccion,id_empresa")] Consignante consignante)
        {
            consignante.id_empresa = SesionActual.IdEmpresa;
            if (ModelState.IsValid)
            {
                db.Consignantes.Add(consignante);
                db.SaveChanges();
                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Consignante", consignante.id_consignante, null, consignante.nombre);
                TempData["MensajeExito"] = "Consignante registrado.";
                return RedirectToAction("Index");
            }

            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", consignante.id_empresa);
            return View(consignante);
        }

        // GET: Consignantes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consignante consignante = db.Consignantes.Find(id);
            if (consignante == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", consignante.id_empresa);
            return View(consignante);
        }

        // POST: Consignantes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_consignante,nombre,DPI,telefono,direccion,id_empresa")] Consignante consignante)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consignante).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", consignante.id_empresa);
            return View(consignante);
        }

        // GET: Consignantes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consignante consignante = db.Consignantes.Find(id);
            if (consignante == null)
            {
                return HttpNotFound();
            }
            return View(consignante);
        }

        // POST: Consignantes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Consignante consignante = db.Consignantes.Find(id);
            db.Consignantes.Remove(consignante);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
