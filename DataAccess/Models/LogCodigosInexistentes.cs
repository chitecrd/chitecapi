using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class LogCodigosInexistentes
    {
        public string id { get; set; }
        public string id_Terminal { get; set; }
        public string Barcode { get; set; }
        public string id_almacen { get; set; }
        public string id_usuario { get; set; }
        public string id_ubicacion { get; set; }
        public string Fecha { get; set; }

        public bool IsNotInitialized =>
            id == null
            && id_Terminal == null
            && Barcode == null
            && id_almacen == null
            && id_usuario == null
            && id_ubicacion == null
            && Fecha == null;
    }
}
