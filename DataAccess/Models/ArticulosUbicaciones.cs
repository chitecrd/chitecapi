using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    class ArticulosUbicaciones
    {
        public string id { get; set; }
        public string No_articulo { get; set; }
        public string Descripcion { get; set; }
        public string Ubicacion { get; set; }
        public string Codigo_Barras { get; set; }
        public string Tipo_ubicacion { get; set; }
       
        public bool IsNotInitialized =>
            id == null
            && No_articulo == null
            && Descripcion == null
            && Ubicacion == null
            && Codigo_Barras == null
            && Tipo_ubicacion == null;
    
     }
}
