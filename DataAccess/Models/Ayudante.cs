using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class Ayudante
    {
        [JsonIgnore]
        public string id { get; set; }
        public string codigo_unico { get; set; }
        public string nombre { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        [JsonIgnore]
        public bool IsNotInitialized =>
            id == null
            && codigo_unico == null
            && nombre == null
            && created_at == null
            && updated_at == null;
    }
}
