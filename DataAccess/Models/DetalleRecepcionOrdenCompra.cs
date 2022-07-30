using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class DetalleRecepcionOrdenCompra
    {
        [JsonIgnore]
        public int id { get; set; }
        [JsonIgnore]
        public int id_recepcion_orden_compra { get; set; }
        public string no_articulo { get; set; }
        public float cantidad { get; set; }
        public string id_ubicacion { get; set; }

        [JsonIgnore]
        public bool IsNotInitialized =>
            id == 0
            && id_recepcion_orden_compra == 0
            && no_articulo == null
            && cantidad == 0
            && id_ubicacion == null;
    }
}
