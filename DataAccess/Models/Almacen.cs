namespace DataAccess.Models
{
    public class Almacen
    {
        public string id { get; set; }
        public string descripcion { get; set; }
        public string cod_alterno { get; set; }
        public string estado { get; set; }
        public bool IsNotInitialized =>
            id == null
            && cod_alterno == null
            && descripcion == null
            && estado == null;
    }
}
