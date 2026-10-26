using Carros_Web.Models;
using System;
using System.Data.Entity;
using System.Linq;

namespace Carros_Web.Negocio
{
    public class ResultadoVenta
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int IdVenta { get; set; }
    }

    public class VentaNegocio
    {
        /// <summary>
        /// Registra la venta de un vehículo: calcula la comisión de la
        /// concesionaria a partir del contrato de consignación (o el
        /// porcentaje por defecto de la empresa si el vehículo no tiene
        /// contrato), marca el vehículo como vendido y genera la
        /// liquidación pendiente para el consignante.
        /// </summary>
        public ResultadoVenta Registrar(int idVehiculo, int idCliente, decimal precioFinal, int idFormaPago,
                                         int idEmpresa, int idUsuario)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                var vehiculo = db.Vehiculoes.Include(v => v.Contrato)
                                             .Include(v => v.Ventas)
                                             .FirstOrDefault(v => v.id_vehiculo == idVehiculo && v.id_empresa == idEmpresa);

                if (vehiculo == null)
                    return new ResultadoVenta { Exito = false, Mensaje = "Vehículo no encontrado." };

                if (vehiculo.Ventas.Any())
                    return new ResultadoVenta { Exito = false, Mensaje = "Este vehículo ya tiene una venta registrada." };

                decimal porcentaje = vehiculo.Contrato?.porcentaje_comision
                                     ?? db.Empresas.Find(idEmpresa)?.porcentaje_comision_default
                                     ?? 10m;

                decimal comision = Math.Round(precioFinal * porcentaje / 100m, 2);
                decimal liquidacion = precioFinal - comision;

                var venta = new Venta
                {
                    id_vehiculo = idVehiculo,
                    id_cliente = idCliente,
                    precio_final = precioFinal,
                    id_forma_pago = idFormaPago,
                    comision = comision,
                    liquidacion_consignante = liquidacion,
                    fecha = DateTime.Now,
                    id_empresa = idEmpresa
                };
                db.Ventas.Add(venta);
                db.SaveChanges();

                var estadoVendido = db.Catalogo_EstadoVehiculo.FirstOrDefault(e => e.descripcion == "Vendido");
                if (estadoVendido != null)
                {
                    vehiculo.id_estado = estadoVendido.id_estado;
                }

                if (vehiculo.Contrato != null)
                {
                    db.Liquidacions.Add(new Liquidacion
                    {
                        id_venta = venta.id_venta,
                        monto_liquidado = liquidacion,
                        fecha_pago = null,
                        estado = "Pendiente",
                        id_empresa = idEmpresa
                    });
                }

                db.SaveChanges();

                new BitacoraNegocio().Registrar(idUsuario, idEmpresa, "INSERT", "Venta", venta.id_venta,
                    null, $"Vehiculo={idVehiculo}; Precio={precioFinal}; Comision={comision}");

                return new ResultadoVenta { Exito = true, IdVenta = venta.id_venta };
            }
        }
    }
}
