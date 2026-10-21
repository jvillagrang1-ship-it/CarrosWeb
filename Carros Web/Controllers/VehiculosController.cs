using Carros_Web.Models;
using CarrosWeb.Infraestructura;
using CarrosWeb.Models;
using CarrosWeb.Negocio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace CarrosWeb.Controllers
{
    [RequierePermiso("Vehiculos")]
    public class VehiculosController : Controller
    {
        private ConcesionariaWEBEntities db = new ConcesionariaWEBEntities();

        // Extensiones y tamaño permitidos para las fotos de vehículos.
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const int TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Vehículos disponibles para vender (con fotos y contrato, para la vitrina).
        /// Lo comparten el catálogo y la página de inicio.
        /// </summary>
        public static List<Vehiculo> ConsultarDisponibles(ConcesionariaWEBEntities contexto)
        {
            int idEmpresa = SesionActual.IdEmpresa;
            return contexto.Vehiculoes
                .Include(v => v.Catalogo_EstadoVehiculo)
                .Include(v => v.Catalogo_Marca)
                .Include(v => v.Catalogo_TipoVehiculo)
                .Include(v => v.Contrato)
                .Include(v => v.VehiculoFotoes)
                .Where(v => v.id_empresa == idEmpresa
                         && v.Catalogo_EstadoVehiculo.descripcion == "Disponible"
                         && !v.Ventas.Any())
                .OrderByDescending(v => v.id_vehiculo)
                .ToList();
        }

        // GET: Vehiculos/Catalogo
        // Vista tipo vitrina (tarjetas con foto) de los vehículos disponibles,
        // pensada para que el vendedor arranque la venta desde ahí con un clic.
        public ActionResult Catalogo()
        {
            return View(ConsultarDisponibles(db));
        }

        // GET: Vehiculos
        public ActionResult Index(string estado, string placa)
        {
            var vehiculos = db.Vehiculoes
                .Include(v => v.VehiculoFotoes)
                .Include(v => v.Catalogo_EstadoVehiculo)
                .Include(v => v.Catalogo_Marca)
                .Include(v => v.Catalogo_TipoVehiculo)
                .Include(v => v.Contrato.Consignante)
                .Where(v => v.id_empresa == SesionActual.IdEmpresa);

            if (!string.IsNullOrWhiteSpace(estado))
                vehiculos = vehiculos.Where(v => v.Catalogo_EstadoVehiculo.descripcion == estado);

            if (!string.IsNullOrWhiteSpace(placa))
                vehiculos = vehiculos.Where(v => v.placa.Contains(placa));

            ViewBag.estado = estado;
            ViewBag.placa = placa;

            return View(vehiculos.OrderByDescending(v => v.id_vehiculo).ToList());
        }

        // GET: Vehiculos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var vehiculo = db.Vehiculoes
                .Include(v => v.Catalogo_EstadoVehiculo)
                .Include(v => v.Catalogo_Marca)
                .Include(v => v.Catalogo_TipoVehiculo)
                .Include(v => v.Contrato.Consignante)
                .Include(v => v.VehiculoFotoes)
                .Include(v => v.Mantenimientoes)
                .FirstOrDefault(v => v.id_vehiculo == id && v.id_empresa == SesionActual.IdEmpresa);

            if (vehiculo == null) return HttpNotFound();

            // Estados a los que se puede cambiar a mano ("Vendido" solo se asigna al registrar una venta).
            ViewBag.Estados = db.Catalogo_EstadoVehiculo.Where(e => e.descripcion != "Vendido").ToList();
            return View(vehiculo);
        }

        // POST: Vehiculos/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(int idVehiculo, int idEstado)
        {
            var vehiculo = db.Vehiculoes.Include(v => v.Catalogo_EstadoVehiculo)
                                         .FirstOrDefault(v => v.id_vehiculo == idVehiculo && v.id_empresa == SesionActual.IdEmpresa);
            if (vehiculo == null) return HttpNotFound();

            var nuevo = db.Catalogo_EstadoVehiculo.Find(idEstado);
            if (nuevo == null || nuevo.descripcion == "Vendido")
            {
                TempData["MensajeError"] = "Estado no válido: \"Vendido\" solo se asigna al registrar una venta.";
                return RedirectToAction("Details", new { id = idVehiculo });
            }
            if (vehiculo.Catalogo_EstadoVehiculo?.descripcion == "Vendido")
            {
                TempData["MensajeError"] = "El vehículo ya fue vendido; su estado no se puede cambiar.";
                return RedirectToAction("Details", new { id = idVehiculo });
            }

            string anterior = vehiculo.Catalogo_EstadoVehiculo?.descripcion;
            vehiculo.id_estado = nuevo.id_estado;
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Vehiculo", idVehiculo, anterior, nuevo.descripcion);

            TempData["MensajeExito"] = $"Estado cambiado a '{nuevo.descripcion}'.";
            return RedirectToAction("Details", new { id = idVehiculo });
        }

        // POST: Vehiculos/SubirFoto
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubirFoto(int idVehiculo, IEnumerable<HttpPostedFileBase> fotos)
        {
            var vehiculo = db.Vehiculoes.FirstOrDefault(v => v.id_vehiculo == idVehiculo && v.id_empresa == SesionActual.IdEmpresa);
            if (vehiculo == null) return HttpNotFound();

            if (fotos == null || !fotos.Any(f => f != null && f.ContentLength > 0))
            {
                TempData["MensajeError"] = "Selecciona al menos una imagen.";
                return RedirectToAction("Details", new { id = idVehiculo });
            }

            int subidas = GuardarFotos(idVehiculo, fotos);
            if (subidas > 0)
                TempData["MensajeExito"] = $"{subidas} foto(s) subida(s) correctamente.";

            return RedirectToAction("Details", new { id = idVehiculo });
        }

        // GET: Vehiculos/Create
        public ActionResult Create()
        {
            CargarListas(null);
            return View();
        }

        // POST: Vehiculos/Create
        // Registra el vehículo y, si se indican los datos, el contrato de consignación en el mismo paso.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo,id_marca,modelo,año,placa,kilometraje,id_estado,id_contrato")] Vehiculo vehiculo,
                                    IEnumerable<HttpPostedFileBase> fotos,
                                    int? idConsignante, decimal? precioPactado, decimal? porcentajeComision,
                                    DateTime? fechaInicio, DateTime? fechaFin)
        {
            Contrato contratoNuevo = null;
            if (idConsignante.HasValue)
            {
                if (!precioPactado.HasValue || !porcentajeComision.HasValue || !fechaInicio.HasValue || !fechaFin.HasValue)
                    ModelState.AddModelError("", "Para crear el contrato indica precio pactado, comisión y vigencia.");
                else if (fechaFin.Value <= fechaInicio.Value)
                    ModelState.AddModelError("", "La fecha de fin del contrato debe ser posterior a la de inicio.");
                else
                    contratoNuevo = new Contrato
                    {
                        id_consignante = idConsignante,
                        precio_pactado = precioPactado.Value,
                        porcentaje_comision = porcentajeComision.Value,
                        fecha_inicio = fechaInicio.Value,
                        fecha_fin = fechaFin.Value,
                        id_empresa = SesionActual.IdEmpresa
                    };
            }

            if (ModelState.IsValid)
            {
                if (contratoNuevo != null)
                {
                    db.Contratoes.Add(contratoNuevo);
                    db.SaveChanges();
                    vehiculo.id_contrato = contratoNuevo.id_contrato;
                    new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Contrato", contratoNuevo.id_contrato,
                        null, $"Consignante={idConsignante}; Precio={precioPactado}; Comision={porcentajeComision}");
                }

                // Un vehículo nuevo queda "Disponible" salvo que se indique otro estado.
                if (!vehiculo.id_estado.HasValue)
                    vehiculo.id_estado = db.Catalogo_EstadoVehiculo.Where(e => e.descripcion == "Disponible")
                                                                   .Select(e => (int?)e.id_estado).FirstOrDefault();

                vehiculo.id_empresa = SesionActual.IdEmpresa;
                db.Vehiculoes.Add(vehiculo);
                db.SaveChanges();

                GuardarFotos(vehiculo.id_vehiculo, fotos);

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "INSERT", "Vehiculo", vehiculo.id_vehiculo,
                    null, $"Placa={vehiculo.placa}; Modelo={vehiculo.modelo}");

                TempData["MensajeExito"] = "Vehículo registrado en consignación correctamente.";
                return RedirectToAction("Details", new { id = vehiculo.id_vehiculo });
            }

            CargarListas(vehiculo);
            return View(vehiculo);
        }

        // GET: Vehiculos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var vehiculo = db.Vehiculoes.Include(v => v.VehiculoFotoes)
                                         .FirstOrDefault(v => v.id_vehiculo == id && v.id_empresa == SesionActual.IdEmpresa);
            if (vehiculo == null) return HttpNotFound();

            CargarListas(vehiculo);
            return View(vehiculo);
        }

        // POST: Vehiculos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_vehiculo,id_tipo,id_marca,modelo,año,placa,kilometraje,id_estado,id_contrato")] Vehiculo vehiculo,
                                  IEnumerable<HttpPostedFileBase> fotos)
        {
            var existente = db.Vehiculoes.FirstOrDefault(v => v.id_vehiculo == vehiculo.id_vehiculo && v.id_empresa == SesionActual.IdEmpresa);
            if (existente == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                existente.id_tipo = vehiculo.id_tipo;
                existente.id_marca = vehiculo.id_marca;
                existente.modelo = vehiculo.modelo;
                existente.año = vehiculo.año;
                existente.placa = vehiculo.placa;
                existente.kilometraje = vehiculo.kilometraje;
                existente.id_estado = vehiculo.id_estado;
                existente.id_contrato = vehiculo.id_contrato;
                db.SaveChanges();

                GuardarFotos(existente.id_vehiculo, fotos);

                new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "UPDATE", "Vehiculo", existente.id_vehiculo);

                TempData["MensajeExito"] = "Vehículo actualizado.";
                return RedirectToAction("Details", new { id = existente.id_vehiculo });
            }

            CargarListas(vehiculo);
            return View(vehiculo);
        }

        // POST: Vehiculos/EliminarFoto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarFoto(int idFoto, int idVehiculo, string volverA)
        {
            var foto = db.VehiculoFotoes.FirstOrDefault(f => f.id_foto == idFoto && f.id_vehiculo == idVehiculo
                                                              && f.Vehiculo.id_empresa == SesionActual.IdEmpresa);
            if (foto != null)
            {
                try
                {
                    string ruta = Server.MapPath("~" + foto.ruta_archivo);
                    if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
                }
                catch { /* si el archivo ya no existe, continuamos */ }

                db.VehiculoFotoes.Remove(foto);
                db.SaveChanges();
            }
            return RedirectToAction(volverA == "Details" ? "Details" : "Edit", new { id = idVehiculo });
        }

        // GET: Vehiculos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var vehiculo = db.Vehiculoes.Include(v => v.Catalogo_Marca)
                                         .FirstOrDefault(v => v.id_vehiculo == id && v.id_empresa == SesionActual.IdEmpresa);
            if (vehiculo == null) return HttpNotFound();
            return View(vehiculo);
        }

        // POST: Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var vehiculo = db.Vehiculoes.Include(v => v.VehiculoFotoes).Include(v => v.Ventas)
                                         .FirstOrDefault(v => v.id_vehiculo == id && v.id_empresa == SesionActual.IdEmpresa);
            if (vehiculo == null) return HttpNotFound();

            if (vehiculo.Ventas.Any())
            {
                TempData["MensajeError"] = "No se puede eliminar: el vehículo ya tiene una venta registrada.";
                return RedirectToAction("Index");
            }

            foreach (var foto in vehiculo.VehiculoFotoes.ToList())
            {
                try
                {
                    string ruta = Server.MapPath("~" + foto.ruta_archivo);
                    if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
                }
                catch { }
                db.VehiculoFotoes.Remove(foto);
            }

            db.Vehiculoes.Remove(vehiculo);
            db.SaveChanges();

            new BitacoraNegocio().Registrar(SesionActual.IdUsuario, SesionActual.IdEmpresa, "DELETE", "Vehiculo", id);

            TempData["MensajeExito"] = "Vehículo eliminado.";
            return RedirectToAction("Index");
        }

        private void CargarListas(Vehiculo vehiculo)
        {
            ViewBag.id_tipo = new SelectList(db.Catalogo_TipoVehiculo, "id_tipo", "descripcion", vehiculo?.id_tipo);
            ViewBag.id_marca = new SelectList(db.Catalogo_Marca, "id_marca", "descripcion", vehiculo?.id_marca);
            ViewBag.id_estado = new SelectList(db.Catalogo_EstadoVehiculo, "id_estado", "descripcion", vehiculo?.id_estado);

            var contratos = db.Contratoes.Include(c => c.Consignante)
                                          .Where(c => c.id_empresa == SesionActual.IdEmpresa)
                                          .ToList()
                                          .Select(c => new { c.id_contrato, Texto = $"#{c.id_contrato} - {c.Consignante?.nombre} (hasta {c.fecha_fin:dd/MM/yyyy})" });
            ViewBag.id_contrato = new SelectList(contratos, "id_contrato", "Texto", vehiculo?.id_contrato);

            ViewBag.Consignantes = db.Consignantes.Where(c => c.id_empresa == SesionActual.IdEmpresa)
                                                   .OrderBy(c => c.nombre).ToList();
        }

        private int GuardarFotos(int idVehiculo, IEnumerable<HttpPostedFileBase> fotos)
        {
            if (fotos == null) return 0;

            string carpeta = Server.MapPath($"~/Uploads/vehiculos/{idVehiculo}");
            int guardadas = 0;
            foreach (var archivo in fotos.Where(f => f != null && f.ContentLength > 0))
            {
                string extension = Path.GetExtension(archivo.FileName ?? "").ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(extension))
                {
                    TempData["MensajeError"] = $"'{archivo.FileName}' no es una imagen válida (usa jpg, png o webp).";
                    continue;
                }
                if (archivo.ContentLength > TamanoMaximoBytes)
                {
                    TempData["MensajeError"] = $"'{archivo.FileName}' pesa más de 5 MB.";
                    continue;
                }

                Directory.CreateDirectory(carpeta);
                string nombreArchivo = $"{Guid.NewGuid()}{extension}";
                archivo.SaveAs(Path.Combine(carpeta, nombreArchivo));

                db.VehiculoFotoes.Add(new VehiculoFoto
                {
                    id_vehiculo = idVehiculo,
                    ruta_archivo = $"/Uploads/vehiculos/{idVehiculo}/{nombreArchivo}",
                    fecha_subida = DateTime.Now
                });
                guardadas++;
            }
            db.SaveChanges();
            return guardadas;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
