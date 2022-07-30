namespace DataAccess.Models
{
    public class Clasificacion
    {
        public string id { get; set; }
        public string codigo_unico { get; set; }
        public string descripcion { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool IsNotInitialized =>
            id == null
            && codigo_unico == null
            && descripcion == null
            && created_at == null
            && updated_at == null;
    }
}
