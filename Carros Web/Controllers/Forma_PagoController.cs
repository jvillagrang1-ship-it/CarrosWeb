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
    public class Forma_PagoController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // GET: Forma_Pago
        public ActionResult Index()
        {
            return View(db.Forma_Pago.ToList());
        }

        // GET: Forma_Pago/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Forma_Pago forma_Pago = db.Forma_Pago.Find(id);
            if (forma_Pago == null)
            {
                return HttpNotFound();
            }
            return View(forma_Pago);
        }

        // GET: Forma_Pago/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Forma_Pago/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_forma_pago,descripcion")] Forma_Pago forma_Pago)
        {
            if (ModelState.IsValid)
            {
                db.Forma_Pago.Add(forma_Pago);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(forma_Pago);
        }

        // GET: Forma_Pago/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Forma_Pago forma_Pago = db.Forma_Pago.Find(id);
            if (forma_Pago == null)
            {
                return HttpNotFound();
            }
            return View(forma_Pago);
        }

        // POST: Forma_Pago/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_forma_pago,descripcion")] Forma_Pago forma_Pago)
        {
            if (ModelState.IsValid)
            {
                db.Entry(forma_Pago).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(forma_Pago);
        }

        // GET: Forma_Pago/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Forma_Pago forma_Pago = db.Forma_Pago.Find(id);
            if (forma_Pago == null)
            {
                return HttpNotFound();
            }
            return View(forma_Pago);
        }

        // POST: Forma_Pago/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Forma_Pago forma_Pago = db.Forma_Pago.Find(id);
            db.Forma_Pago.Remove(forma_Pago);
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
