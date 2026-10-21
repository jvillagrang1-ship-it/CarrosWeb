using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Empresa")]
    public class EmpresaController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        public ActionResult Index()
        {
            var empresa = db.Empresas.FirstOrDefault();
            return View(empresa);
        }

        public ActionResult Editar()
        {
            var empresa = db.Empresas.FirstOrDefault();
            if (empresa == null)
                empresa = new Empresa { nombre = "Mi Concesionaria", moneda = "GTQ", porcentaje_comision_default = 10m };
            return View(empresa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Empresa modelo, HttpPostedFileBase archivoLogo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var empresa = db.Empresas.Find(modelo.id_empresa);
            if (empresa == null)
            {
                empresa = modelo;
                db.Empresas.Add(empresa);
            }
            else
            {
                empresa.nombre = modelo.nombre;
                empresa.moneda = modelo.moneda;
                empresa.porcentaje_comision_default = modelo.porcentaje_comision_default;
            }

            if (archivoLogo != null && archivoLogo.ContentLength > 0)
            {
                string carpeta = Server.MapPath("~/Uploads/empresa");
                Directory.CreateDirectory(carpeta);
                string nombreArchivo = $"logo_{DateTime.Now.Ticks}{Path.GetExtension(archivoLogo.FileName)}";
                archivoLogo.SaveAs(Path.Combine(carpeta, nombreArchivo));
                empresa.logotipo = "/Uploads/empresa/" + nombreArchivo;
            }

            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, empresa.id_empresa, "UPDATE", "Empresa", empresa.id_empresa);

            TempData["MensajeExito"] = "Datos de la empresa actualizados.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
