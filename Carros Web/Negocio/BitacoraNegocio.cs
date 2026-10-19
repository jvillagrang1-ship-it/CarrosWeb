using Carros_Web.Models;
using CarrosWeb.Models;
using System;

namespace CarrosWeb.Negocio
{
    public class BitacoraNegocio
    {
        public void Registrar(int idUsuario, int idEmpresa, string accion, string entidadAfectada,
                               int? idRegistro = null, string valorAnterior = null, string valorNuevo = null)
        {
            using (var db = new ConcesionariaWEBEntities())
            {
                db.Bitacoras.Add(new Bitacora
                {
                    id_usuario = idUsuario == 0 ? (int?)null : idUsuario,
                    id_empresa = idEmpresa == 0 ? (int?)null : idEmpresa,
                    accion = accion,
                    entidad_afectada = entidadAfectada,
                    id_registro = idRegistro,
                    valor_anterior = valorAnterior,
                    valor_nuevo = valorNuevo,
                    fecha_hora = DateTime.Now
                });
                db.SaveChanges();
            }
        }
    }
}
