using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Mantenimientos")]
    public class TallersController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Tallers
        public ActionResult Index()
        {
            var tallers = db.Tallers.Include(t => t.Empresa);
            return View(tallers.ToList());
        }

        // GET: Tallers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Taller taller = db.Tallers.Find(id);
            if (taller == null)
            {
                return HttpNotFound();
            }
            return View(taller);
        }

        // GET: Tallers/Create
        public ActionResult Create()
        {
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre");
            return View();
        }

        // POST: Tallers/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_taller,nombre,direccion,telefono,id_empresa")] Taller taller)
        {
            if (ModelState.IsValid)
            {
                db.Tallers.Add(taller);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", taller.id_empresa);
            return View(taller);
        }

        // GET: Tallers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Taller taller = db.Tallers.Find(id);
            if (taller == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", taller.id_empresa);
            return View(taller);
        }

        // POST: Tallers/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_taller,nombre,direccion,telefono,id_empresa")] Taller taller)
        {
            if (ModelState.IsValid)
            {
                db.Entry(taller).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_empresa = new SelectList(db.Empresas, "id_empresa", "nombre", taller.id_empresa);
            return View(taller);
        }

        // GET: Tallers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Taller taller = db.Tallers.Find(id);
            if (taller == null)
            {
                return HttpNotFound();
            }
            return View(taller);
        }

        // POST: Tallers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Taller taller = db.Tallers.Find(id);
            db.Tallers.Remove(taller);
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
