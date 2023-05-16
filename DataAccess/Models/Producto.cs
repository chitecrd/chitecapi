using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Producto
    {
        public Int64 Sec { get; set; }
        public String Codigo { get; set; }
        public String CodBar { get; set; }
        public String Cod_Fab { get; set; }
        public String Comentario { get; set; }
        public String Descripcion { get; set; }
        public String UND { get; set; }
        public double Empaque { get; set; }
        public String Detalle { get; set; }
        public Boolean ITBIS { get; set; }
        public Double PorITBIS { get; set; }
        public Double Precio { get; set; }
        public Double PrecioOferta { get; set; }
        public Double Costo { get; set; }
        public Double PrecioLista { get; set; }
        public Double xMayor1 { get; set; }
        public Double xMayor2 { get; set; }
        public Double xMayor3 { get; set; }
        public Double PrecioMinimo { get; set; }
        public Double EnOferta { get; set; }
        public int AreaID { get; set; }
        public int DptoID { get; set; }
        public int GrupoID { get; set; }
        public String Area { get; set; }
        public String Dpto { get; set; }
        public String Grupo { get; set; }
        public String Foto { get; set; }
        public double Inventario { get; set; }
        public Boolean Pesado { get; set; }
        public double AddCantidad { get; set; }
        public double quantity { get; set; }
        public double Agregado { get; set; }

        public string DescripcionLarga { get; set; }
        public string CodigoJuntos { get; set; }
        public string CodigoJuntos1 { get; set; }

        public List<String> FotosAlterna { get; set; }
        public List<Producto> ProductosLink { get; set; }
        public Int64 Link { get; set; }

    }
}
