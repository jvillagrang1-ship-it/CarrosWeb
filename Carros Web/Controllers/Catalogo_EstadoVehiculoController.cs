using Carros_Web.Models;
using Carros_Web.Infraestructura;
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
    public class Catalogo_EstadoVehiculoController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Catalogo_EstadoVehiculo
        public ActionResult Index()
        {
            return View(db.Catalogo_EstadoVehiculo.ToList());
        }

        // GET: Catalogo_EstadoVehiculo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_EstadoVehiculo catalogo_EstadoVehiculo = db.Catalogo_EstadoVehiculo.Find(id);
            if (catalogo_EstadoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_EstadoVehiculo);
        }

        // GET: Catalogo_EstadoVehiculo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Catalogo_EstadoVehiculo/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_estado,descripcion")] Catalogo_EstadoVehiculo catalogo_EstadoVehiculo)
        {
            if (ModelState.IsValid)
            {
                db.Catalogo_EstadoVehiculo.Add(catalogo_EstadoVehiculo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(catalogo_EstadoVehiculo);
        }

        // GET: Catalogo_EstadoVehiculo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_EstadoVehiculo catalogo_EstadoVehiculo = db.Catalogo_EstadoVehiculo.Find(id);
            if (catalogo_EstadoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_EstadoVehiculo);
        }

        // POST: Catalogo_EstadoVehiculo/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_estado,descripcion")] Catalogo_EstadoVehiculo catalogo_EstadoVehiculo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogo_EstadoVehiculo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(catalogo_EstadoVehiculo);
        }

        // GET: Catalogo_EstadoVehiculo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_EstadoVehiculo catalogo_EstadoVehiculo = db.Catalogo_EstadoVehiculo.Find(id);
            if (catalogo_EstadoVehiculo == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_EstadoVehiculo);
        }

        // POST: Catalogo_EstadoVehiculo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Catalogo_EstadoVehiculo catalogo_EstadoVehiculo = db.Catalogo_EstadoVehiculo.Find(id);
            db.Catalogo_EstadoVehiculo.Remove(catalogo_EstadoVehiculo);
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
