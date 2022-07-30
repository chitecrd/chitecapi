namespace DataAccess.Models
{
    public class InventoryStatistic
    {
        public int cantidad_articulos { get; set; }
        public int cantidad_articulos_no_existen_tienda { get; set; }
        public decimal porciento_cantidad_articulos { get; set; }
        public decimal costo_total { get; set; }
        public decimal costo_total_no_existe_tienda { get; set; }
        public decimal porciento_costo_total { get; set; }
        public decimal precio_total { get; set; }
        public decimal precio_total_no_existe_tienda { get; set; }
        public decimal porciento_precio_total { get; set; }
    }
}
