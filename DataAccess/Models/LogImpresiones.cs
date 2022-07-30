using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class LogImpresiones
    {
        public string id { get; set; }
        public string id_Terminal { get; set; }
        public string Barcode { get; set; }
        public string cantidad_impresiones { get; set; }
        public string id_usuario { get; set; }
        public string No_articulo { get; set; }
        public string Fecha { get; set; }

        public bool IsNotInitialized =>
            id == null
            && id_Terminal == null
            && Barcode == null
            && cantidad_impresiones == null
            && id_usuario == null
            && No_articulo == null
            && Fecha == null;
    }
}
