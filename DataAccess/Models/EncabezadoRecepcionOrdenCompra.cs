using Newtonsoft.Json;
using System;

namespace DataAccess.Models
{
    public class EncabezadoRecepcionOrdenCompra
    {
        [JsonIgnore]
        public int id { get; set; }
        public string fecha { get; set; }
        public string id_orden_compra { get; set; }
        [JsonIgnore]
        public string fecha_registro { get; set; }
        public string id_contenedor { get; set; }
        public string numero_factura { get; set; }
        public string comentario { get; set; }
        public int id_usuario { get; set; }

        [JsonIgnore]
        public bool IsNotInitialized =>
            id == 0
            && fecha == null
            && id_orden_compra == null
            && fecha_registro == null
            && id_contenedor == null
            && numero_factura == null
            && comentario == null
            && id_usuario == 0;
    }
}
