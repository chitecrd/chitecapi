using Newtonsoft.Json;

namespace DataAccess.Models
{
    public class Asignacion
    {
        [JsonIgnore]
        public string id { get; set; }
        public string codigo_unico { get; set; }
        public string cod_alterno { get; set; }
        public string descripcion { get; set; }
        public string id_usuario { get; set; }
        public string id_ayudante { get; set; }
        public string nombre_usuario { get; set; }
        public string nombre_ayudante { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        [JsonIgnore]
        public bool IsNotInitialized =>
            id == null
            && codigo_unico == null
            && cod_alterno == null
            && descripcion == null
            && id_usuario == null
            && id_ayudante == null
            && nombre_usuario == null
            && nombre_ayudante == null
            && created_at == null
            && updated_at == null;
    }
}
