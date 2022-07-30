namespace DataAccess.Models
{
    public class UsuarioAuditoria
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string usuario { get; set; }
        public string password { get; set; }
        public string estado { get; set; }
        public string tipo_usuario { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public bool IsNotInitialized =>
            id == null
            && nombre == null
            && apellido == null
            && usuario == null
            && password == null
            && estado == null
            && tipo_usuario == null
            && created_at == null
            && updated_at == null;

    }
}
