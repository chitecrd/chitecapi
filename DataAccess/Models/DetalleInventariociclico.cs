using Newtonsoft.Json;
using System;

namespace DataAccess.Models
{
    public class DetalleInventariociclico
    {
	
        public string id { get; set; }
        public string No_interno { get; set; }
        public string No_Conteo { get; set; }
        public string No_detalleinv_ciclico { get; set; }
        public string Nombre_Conteo { get; set; }
        public string Tipo_Conteo { get; set; }
        public string No_Articulo { get; set; }
        public string Codigo_Barra { get; set; }
        public string Alterno1 { get; set; }
        public string Alterno2 { get; set; }
        public string Alterno3 { get; set; }
        public string Descripcion { get; set; }
        public string Existencia { get; set; }
        public string Cantidad_Transito { get; set; }
        public string Unidad_Medida { get; set; }
        public string Costo { get; set; }
        public string Precio { get; set; }
        public string Marca { get; set; }
        public string id_Familia_Productos { get; set; }
        public string id_Clasificacion_inventario { get; set; }
        public string Fecha_asignada_ciclico { get; set; }
        public string estado { get; set; }
        public string Id_Terminal { get; set; }
        public string Wharehouse_Existencia { get; set; }
        public string Diferencia { get; set; }
        public string Diferencia_porcentual { get; set; }
        public string Cantidad { get; set; }
        public string Id_Usuario_Registro { get; set; }
        public string Fecha_Registro { get; set; }
        public string Cantidad_Contada { get; set; }
        public string Observaciones { get; set; }
        public string Costing_Code { get; set; }
        public string Wharehouse { get; set; }
        public string ubicacion { get; set; }
        public string Fecha_Conteo { get; set; }
        public string Hora_Conteo { get; set; }
        [JsonIgnore]
        public bool IsNotInitialized =>
            id == null
            && No_interno == null
            && No_Conteo == null
            && Nombre_Conteo == null
            && No_detalleinv_ciclico == null
            && Tipo_Conteo == null
            && No_Articulo == null
            && Codigo_Barra == null
            && Alterno1 == null
            && Alterno2 == null
            && Alterno3 == null
            && Descripcion == null
            && Existencia == null
            && Cantidad_Transito == null
            && Unidad_Medida == null
            && Costo == null
            && Precio == null
            && Marca == null
            && id_Familia_Productos == null
            && id_Clasificacion_inventario == null
            && Fecha_asignada_ciclico == null
            && estado == null
            && Id_Terminal == null
            && Wharehouse_Existencia == null
            && Diferencia == null
            && Diferencia_porcentual == null
            && Cantidad == null
            && Id_Usuario_Registro == null
            && Fecha_Registro == null
            && Cantidad_Contada == null
            && Observaciones == null
            && Costing_Code == null
            && Wharehouse == null
            && ubicacion == null
            && Fecha_Conteo == null
            && Hora_Conteo == null;
    }
}
