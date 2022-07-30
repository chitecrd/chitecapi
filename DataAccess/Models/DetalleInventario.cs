using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class DetalleInventario
    {
        public string id { get; set; }
        public string id_terminal { get; set; }
        public string no_detalleInv { get; set; }
        public string no_articulo { get; set; }
        public string codigo_barra { get; set; }
        public string alterno1 { get; set; }
        public string alterno2 { get; set; }
        public string alterno3 { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string costo { get; set; }
        public string costo_total { get; set; }
        public string precio { get; set; }
        public string precio_total { get; set; }
        public string id_ubicacion { get; set; }
        public string cod_alterno { get; set; }
        public string fecha_registro { get; set; }
        public string fecha_modificacion { get; set; }
        public string id_usuario_registro { get; set; }
        public string id_usuario_modificacion { get; set; }
        public string id_auditor { get; set; }
        public string id_tipo_auditoria { get; set; }
        public string pre_conteo { get; set; }
        public string cantidad_auditada { get; set; }
        public string diferencia { get; set; }
        public string porcentaje_diferencia { get; set; }
        public string id_tipo_error { get; set; }
        public string notas { get; set; }
        public string estado { get; set; }
        public string codigo_capturado { get; set; }
        public string id_almacen { get; set; }
        [JsonIgnore]
        public bool IsNotInitialized =>
            id == null
            && id_terminal == null
            && no_detalleInv == null
            && no_articulo == null
            && codigo_barra == null
            && alterno1 == null
            && alterno2 == null
            && alterno3 == null
            && descripcion == null
            && cantidad == null
            && costo == null
            && costo_total == null
            && precio == null
            && precio_total == null
            && id_ubicacion == null
            && cod_alterno == null
            && fecha_registro == null
            && fecha_modificacion == null
            && id_usuario_registro == null
            && id_usuario_modificacion == null
            && id_auditor == null
            && id_tipo_auditoria == null
            && pre_conteo == null
            && cantidad_auditada == null
            && diferencia == null
            && porcentaje_diferencia == null
            && id_tipo_error == null
            && notas == null
            && estado == null
            && codigo_capturado == null;
    }
}
