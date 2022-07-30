using DataAccess;
using DataAccess.Models;
using FluentScheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace chitecapi.Jobs
{
    public class DetalleInventarioJob : GenericJob, IJob, IRegisteredObject
    {
        private readonly object _lock = new object();
        private bool shuttingDown;

        public void Execute()
        {
            try
            {
                lock (_lock)
                {
                    if (shuttingDown || IsDisabled(ConfigurationManager.AppSettings["detalle_inventario_job_inhabilitado"]))
                    {
                        return;
                    }

                    var db = $"{ConfigurationManager.AppSettings["default_db"]}";

                    if (ConfigurationManager.ConnectionStrings[db] == null)
                    {
                        RegisterJobSuccess(false, $"La base de datos {db} no existe.");
                    }

                    var dbAccess = new SqlServerDataAccess(ConfigurationManager.ConnectionStrings[db].ConnectionString);

                    var pendingAssets = GetPendingAssets(dbAccess);
                    if (pendingAssets.Any())
                    {
                        InsertPendingAssets(pendingAssets, dbAccess);
                        //ChangePendingStatus(pendingAssets, dbAccess);
                    }

                    RegisterJobSuccess(true);
                }
            }
            catch (Exception exception)
            {
                RegisterJobSuccess(false, exception.GetBaseException().Message);
            }
            finally
            {
                HostingEnvironment.UnregisterObject(this);
            }
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                shuttingDown = true;
            }

            HostingEnvironment.UnregisterObject(this);
        }

        private IEnumerable<DetalleInventarioTransitorio> GetPendingAssets(SqlServerDataAccess dbAccess)
        {
            var sql =
                ConfigurationManager.AppSettings["buscar_detalle_inventario_transitorio"];

            return dbAccess.LoadData<DetalleInventarioTransitorio, dynamic>(sql, new { });
        }

        private async void InsertPendingAssets(IEnumerable<DetalleInventarioTransitorio> pendingAssets, SqlServerDataAccess dbAccess)
        {
            foreach (var asset in pendingAssets)
            {

                var articulo = await GetArticulo(asset.no_articulo, dbAccess);

                if (articulo == null)
                {
                    continue;
                }

                var codigoAlterno = await GetCodigoAlterno(asset.id_ubicacion, dbAccess);



                var fullDetalleInventario = SetFullDetalleInventario(asset, articulo, codigoAlterno);



                if ( await IsUniqueDetalleInventario(
                       fullDetalleInventario.id_terminal,
                       fullDetalleInventario.no_detalleInv,
                       dbAccess))
                {
                    InsertDetalleInventario(fullDetalleInventario, dbAccess);
                }
                else
                {
                    EditDuplicatedDetalleInventario(fullDetalleInventario, dbAccess);
                }

                var parameters = new
                {
                    id = asset.id
                };
                var sql = ConfigurationManager.AppSettings["editar_detalle_inventario_transitorio"];

                dbAccess.SaveData(sql, parameters);

                /*
                var parameters = new
                {
                    id_terminal = asset.id_terminal,
                    no_detalleInv = asset.no_detalleInv,
                    no_articulo = asset.no_articulo,
                    codigo_barra = asset.codigo_barra,
                    alterno1 = asset.alterno1,
                    alterno2 = asset.alterno2,
                    alterno3 = asset.alterno3,
                    descripcion = asset.descripcion,
                    cantidad = asset.cantidad,
                    costo = asset.costo,
                    costo_total = asset.costo_total,
                    precio = asset.precio,
                    precio_total = asset.precio_total,
                    id_ubicacion = asset.id_ubicacion,
                    cod_alterno = asset.cod_alterno,
                    fecha_registro = asset.fecha_registro,
                    fecha_modificacion = asset.fecha_modificacion,
                    id_usuario_registro = asset.id_usuario_registro,
                    id_usuario_modificacion = asset.id_usuario_modificacion,
                    codigo_capturado = asset.codigo_capturado,
                    id_auditor = asset.id_auditor,
                    id_tipo_auditoria = asset.id_tipo_auditoria,
                    pre_conteo = asset.pre_conteo,
                    cantidad_auditada = asset.cantidad_auditada,
                    diferencia = asset.diferencia,
                    porcentaje_diferencia = asset.porcentaje_diferencia,
                    id_tipo_error = asset.id_tipo_error,
                    notas = asset.notas,
                    estado = asset.estado,
                    created_at = asset.created_at,
                    updated_at = asset.updated_at
                };
                var sql = ConfigurationManager.AppSettings["crear_detalle_inventario"];

                dbAccess.SaveData(sql, parameters);
                */
            }

        }

        private async Task<bool> IsUniqueDetalleInventario(
         string terminalId, string detalleInventarioNumber, SqlServerDataAccess dbAccess)
        {
            var parameters = new { id_terminal = terminalId, no_detalleInv = detalleInventarioNumber };
            var sql =
                ConfigurationManager.AppSettings["buscar_detalle_inventario_por_terminal_y_numero"];

            var duplicateData = await dbAccess.LoadFirstOrDefaultAsync<DetalleInventario, dynamic>(sql, parameters);

            return duplicateData == null;
        }

        private void EditDuplicatedDetalleInventario(DetalleInventarioTransitorio detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                id_almacen = detalleInventario.id_almacen,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                estado = 1,
                updated_at = DateTime.Now
            };
            var sql = ConfigurationManager.AppSettings["editar_duplicado_detalle_inventario"];

            dbAccess.SaveData(sql, parameters);
        }

        private void InsertDetalleInventario(DetalleInventarioTransitorio detalleInventario, SqlServerDataAccess dbAccess)
        {
            var parameters = new
            {
                id_terminal = detalleInventario.id_terminal,
                no_detalleInv = detalleInventario.no_detalleInv,
                no_articulo = detalleInventario.no_articulo,
                codigo_barra = detalleInventario.codigo_barra,
                alterno1 = detalleInventario.alterno1,
                alterno2 = detalleInventario.alterno2,
                alterno3 = detalleInventario.alterno3,
                descripcion = detalleInventario.descripcion,
                cantidad = detalleInventario.cantidad,
                costo = detalleInventario.costo,
                costo_total = detalleInventario.costo_total,
                precio = detalleInventario.precio,
                precio_total = detalleInventario.precio_total,
                id_ubicacion = detalleInventario.id_ubicacion,
                id_almacen = detalleInventario.id_almacen,
                cod_alterno = detalleInventario.cod_alterno,
                fecha_registro = detalleInventario.fecha_registro,
                fecha_modificacion = detalleInventario.fecha_modificacion,
                id_usuario_registro = detalleInventario.id_usuario_registro,
                id_usuario_modificacion = detalleInventario.id_usuario_modificacion,
                codigo_capturado = detalleInventario.codigo_capturado,
                id_auditor = detalleInventario.id_auditor,
                id_tipo_auditoria = detalleInventario.id_tipo_auditoria,
                pre_conteo = detalleInventario.pre_conteo,
                cantidad_auditada = detalleInventario.cantidad_auditada,
                diferencia = detalleInventario.diferencia,
                porcentaje_diferencia = detalleInventario.porcentaje_diferencia,
                id_tipo_error = detalleInventario.id_tipo_error,
                notas = detalleInventario.notas,
                estado = 1,
                created_at = DateTime.Now,
                updated_at = DateTime.Now,
                generado = 0,
                ultimo_mensaje_error = string.Empty
            };
            var sql = ConfigurationManager.AppSettings["crear_detalle_inventario"];

            dbAccess.SaveData(sql, parameters);
        }


        private async Task<Articulo> GetArticulo(
            string articuloNumber, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_articulo_por_numero"];

            return await dbAccess.LoadFirstOrDefaultAsync<Articulo, dynamic>(
                    sql, new { no_articulo = articuloNumber });
        }


        private async Task<string> GetCodigoAlterno(string ubicacionId, SqlServerDataAccess dbAccess)
        {
            var sql = ConfigurationManager.AppSettings["buscar_ubicacion_por_id"];

            var ubicacion =
                await dbAccess.LoadFirstOrDefaultAsync<Ubicacion, dynamic>(
                    sql, new { id = ubicacionId });

            return ubicacion == null ?
                string.Empty
                : string.IsNullOrEmpty(ubicacion.cod_alterno) ?
                    string.Empty
                    : ubicacion.cod_alterno;
        }


        private DetalleInventarioTransitorio SetFullDetalleInventario(
    DetalleInventarioTransitorio partialDetalleInventario, Articulo articulo, string codigoAlterno)
        {
            float.TryParse(articulo.costo, out var articuloCosto);
            float.TryParse(articulo.precio, out var articuloPrecio);
            float.TryParse(partialDetalleInventario.cantidad, out var articuloCantidad);

            partialDetalleInventario.costo_total = (articuloCosto * articuloCantidad).ToString();

            partialDetalleInventario.precio_total = (articuloPrecio * articuloCantidad).ToString();

            partialDetalleInventario.codigo_barra = string.IsNullOrEmpty(partialDetalleInventario.codigo_barra)
                ? articulo.codigo_barra : partialDetalleInventario.codigo_barra;

            partialDetalleInventario.id_auditor =
                (string.IsNullOrEmpty(partialDetalleInventario.id_auditor)
                ? 0 : int.Parse(partialDetalleInventario.id_auditor)).ToString();

            partialDetalleInventario.id_tipo_auditoria =
                (string.IsNullOrEmpty(partialDetalleInventario.id_tipo_auditoria)
                ? 0 : int.Parse(partialDetalleInventario.id_tipo_auditoria)).ToString();

            partialDetalleInventario.pre_conteo =
                (string.IsNullOrEmpty(partialDetalleInventario.pre_conteo)
                ? 0 : int.Parse(partialDetalleInventario.pre_conteo)).ToString();

            partialDetalleInventario.cantidad_auditada =
                (string.IsNullOrEmpty(partialDetalleInventario.cantidad_auditada)
                    ? 0 : int.Parse(partialDetalleInventario.cantidad_auditada)).ToString();

            partialDetalleInventario.diferencia =
                (string.IsNullOrEmpty(partialDetalleInventario.diferencia)
                    ? 0 : int.Parse(partialDetalleInventario.diferencia)).ToString();

            partialDetalleInventario.porcentaje_diferencia =
                (string.IsNullOrEmpty(partialDetalleInventario.porcentaje_diferencia)
                    ? 0 : int.Parse(partialDetalleInventario.porcentaje_diferencia)).ToString();

            partialDetalleInventario.id_tipo_error =
                (string.IsNullOrEmpty(partialDetalleInventario.id_tipo_error)
                    ? 0 : int.Parse(partialDetalleInventario.id_tipo_error)).ToString();

            partialDetalleInventario.notas = string.IsNullOrEmpty(partialDetalleInventario.notas)
                ? string.Empty : partialDetalleInventario.notas;

            partialDetalleInventario.codigo_capturado =
                string.IsNullOrEmpty(partialDetalleInventario.codigo_capturado)
                ? string.Empty : partialDetalleInventario.codigo_capturado;

            partialDetalleInventario.alterno1 = articulo.alterno1;
            partialDetalleInventario.alterno2 = articulo.alterno2;
            partialDetalleInventario.alterno3 = articulo.alterno3;
            partialDetalleInventario.descripcion = articulo.descripcion;
            partialDetalleInventario.costo = articulo.costo;
            partialDetalleInventario.precio = articulo.precio;
            partialDetalleInventario.cod_alterno = codigoAlterno;

            return partialDetalleInventario;
        }



        private void ChangePendingStatus(IEnumerable<DetalleInventarioTransitorio> pendingAssets, SqlServerDataAccess dbAccess)
        {
            foreach (var asset in pendingAssets)
            {
                var parameters = new
                {
                    id = asset.id
                };
                var sql = ConfigurationManager.AppSettings["editar_detalle_inventario_transitorio"];

                dbAccess.SaveData(sql, parameters);
            }
        }
    }
}