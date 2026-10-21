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
    public class Catalogo_MarcaController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Catalogo_Marca
        public ActionResult Index()
        {
            return View(db.Catalogo_Marca.ToList());
        }

        // GET: Catalogo_Marca/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_Marca catalogo_Marca = db.Catalogo_Marca.Find(id);
            if (catalogo_Marca == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_Marca);
        }

        // GET: Catalogo_Marca/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Catalogo_Marca/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_marca,descripcion")] Catalogo_Marca catalogo_Marca)
        {
            if (ModelState.IsValid)
            {
                db.Catalogo_Marca.Add(catalogo_Marca);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(catalogo_Marca);
        }

        // GET: Catalogo_Marca/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_Marca catalogo_Marca = db.Catalogo_Marca.Find(id);
            if (catalogo_Marca == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_Marca);
        }

        // POST: Catalogo_Marca/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_marca,descripcion")] Catalogo_Marca catalogo_Marca)
        {
            if (ModelState.IsValid)
            {
                db.Entry(catalogo_Marca).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(catalogo_Marca);
        }

        // GET: Catalogo_Marca/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Catalogo_Marca catalogo_Marca = db.Catalogo_Marca.Find(id);
            if (catalogo_Marca == null)
            {
                return HttpNotFound();
            }
            return View(catalogo_Marca);
        }

        // POST: Catalogo_Marca/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Catalogo_Marca catalogo_Marca = db.Catalogo_Marca.Find(id);
            db.Catalogo_Marca.Remove(catalogo_Marca);
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
