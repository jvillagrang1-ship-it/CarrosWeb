using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
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
    [RequierePermiso("Catalogos")]
    public class Catalogo_TipoMantenimientoController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Catalogo_TipoMantenimiento
        public ActionResult Index()
        {
            return View(db.Catalogo_TipoMantenimiento.ToList());
        }

        // GET: Catalogo_TipoMantenimiento/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoMantenimiento catalogo_TipoMantenimiento = db.Catalogo_TipoMantenimiento.Find(id);
            if (catalogo_TipoMantenimiento == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoMantenimiento);
        }

        // GET: Catalogo_TipoMantenimiento/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Catalogo_TipoMantenimiento/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo_mantenimiento,descripcion")] Catalogo_TipoMantenimiento catalogo_TipoMantenimiento)
        {
            if (ModelState.IsValid)
            {
                db.Catalogo_TipoMantenimiento.Add(catalogo_TipoMantenimiento);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(catalogo_TipoMantenimiento);
        }

        // GET: Catalogo_TipoMantenimiento/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoMantenimiento catalogo_TipoMantenimiento = db.Catalogo_TipoMantenimiento.Find(id);
            if (catalogo_TipoMantenimiento == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoMantenimiento);
        }

        // POST: Catalogo_TipoMantenimiento/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tipo_mantenimiento,descripcion")] Catalogo_TipoMantenimiento catalogo_TipoMantenimiento)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogo_TipoMantenimiento).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(catalogo_TipoMantenimiento);
        }

        // GET: Catalogo_TipoMantenimiento/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoMantenimiento catalogo_TipoMantenimiento = db.Catalogo_TipoMantenimiento.Find(id);
            if (catalogo_TipoMantenimiento == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoMantenimiento);
        }

        // POST: Catalogo_TipoMantenimiento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Catalogo_TipoMantenimiento catalogo_TipoMantenimiento = db.Catalogo_TipoMantenimiento.Find(id);
            db.Catalogo_TipoMantenimiento.Remove(catalogo_TipoMantenimiento);
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
