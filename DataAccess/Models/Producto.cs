using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Producto
    {
        public int sec { get; set; }
        public string codigo { get; set; }
        public string codBar { get; set; }
        public string cod_Fab { get; set; }
        public object comentario { get; set; }
        public string descripcion { get; set; }
        public string und { get; set; }
        public int empaque { get; set; }
        public string detalle { get; set; }
        public bool itbis { get; set; }
        public double porITBIS { get; set; }
        public int precio { get; set; }
        public int precioOferta { get; set; }
        public double costo { get; set; }
        public int precioLista { get; set; }
        public int xMayor1 { get; set; }
        public double xMayor2 { get; set; }
        public double xMayor3 { get; set; }
        public double precioMinimo { get; set; }
        public int enOferta { get; set; }
        public int areaID { get; set; }
        public int dptoID { get; set; }
        public int grupoID { get; set; }
        public string area { get; set; }
        public string dpto { get; set; }
        public object grupo { get; set; }
        public string foto { get; set; }
        public int inventario { get; set; }
        public bool pesado { get; set; }
        public int addCantidad { get; set; }
        public int quantity { get; set; }
        public int agregado { get; set; }
        public string descripcionLarga { get; set; }
        public string codigoJuntos { get; set; }
        public string codigoJuntos1 { get; set; }
        public List<string> fotosAlterna { get; set; }
        public object productosLink { get; set; }
        public int link { get; set; }
    }
}
