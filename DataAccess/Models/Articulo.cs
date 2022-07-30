namespace DataAccess.Models
{
    public class Articulo
    {
        public string id { get; set; }
        public string no_articulo { get; set; }
        public string codigo_barra { get; set; }
        public string alterno1 { get; set; }
        public string alterno2 { get; set; }
        public string alterno3 { get; set; }
        public string descripcion { get; set; }
        public string existencia { get; set; }
        public string unidad_medida { get; set; }
        public string costo { get; set; }
        public string precio { get; set; }
        public string referencia { get; set; }
        public string marca { get; set; }
        public string familia { get; set; }
        public bool IsNotInitialized =>
            id == null
            && no_articulo == null
            && codigo_barra == null
            && alterno1 == null
            && alterno2 == null
            && alterno3 == null
            && descripcion == null
            && existencia == null
            && unidad_medida == null
            && costo == null
            && precio == null
            && referencia == null
            && marca == null
            && familia == null;
    }
}
