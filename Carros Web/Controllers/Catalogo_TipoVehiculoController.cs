
using Carros_Web.Infraestructura;
using Carros_Web.Models;
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
    public class Catalogo_TipoVehiculoController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Catalogo_TipoVehiculo
        public ActionResult Index()
        {
            return View(db.Catalogo_TipoVehiculo.ToList());
        }

        // GET: Catalogo_TipoVehiculo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoVehiculo catalogo_TipoVehiculo = db.Catalogo_TipoVehiculo.Find(id);
            if (catalogo_TipoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoVehiculo);
        }

        // GET: Catalogo_TipoVehiculo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Catalogo_TipoVehiculo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo,descripcion")] Catalogo_TipoVehiculo catalogo_TipoVehiculo)
        {
            if (ModelState.IsValid)
            {
                db.Catalogo_TipoVehiculo.Add(catalogo_TipoVehiculo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(catalogo_TipoVehiculo);
        }

        // GET: Catalogo_TipoVehiculo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoVehiculo catalogo_TipoVehiculo = db.Catalogo_TipoVehiculo.Find(id);
            if (catalogo_TipoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoVehiculo);
        }

        // POST: Catalogo_TipoVehiculo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tipo,descripcion")] Catalogo_TipoVehiculo catalogo_TipoVehiculo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogo_TipoVehiculo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(catalogo_TipoVehiculo);
        }

        // GET: Catalogo_TipoVehiculo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_TipoVehiculo catalogo_TipoVehiculo = db.Catalogo_TipoVehiculo.Find(id);
            if (catalogo_TipoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_TipoVehiculo);
        }

        // POST: Catalogo_TipoVehiculo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Catalogo_TipoVehiculo catalogo_TipoVehiculo = db.Catalogo_TipoVehiculo.Find(id);
            db.Catalogo_TipoVehiculo.Remove(catalogo_TipoVehiculo);
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
