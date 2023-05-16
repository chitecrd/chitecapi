using chitecapi.Responses;
using DataAccess;
using DataAccess.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace chitecapi.Controllers
{
    public class Consulta_AzureController : ApiController
    {
        /// <summary>
        /// Obtine un producto
        /// </summary>
        /// <param name="codigo" example="7460103900141">Codigo</param>
        /// <param name="Nivel" example="4">Nivel</param>
        /// <param name="Factor" example="1">Factor</param>
        /// <remarks>
        /// Se utiliza para obtener un producto, generado con un formato JSON:
        /// 
        ///     Example Result:
        ///     {
        ///         "producto": 
        ///                   {
        ///                      "sec": 10774,
        ///                      "codigo": "7460103900141",
        ///                      "codBar": "N10774",
        ///                      "cod_Fab": "INV.PELO",
        ///                      "comentario": null,
        ///                      "descripcion": "INVITACIONES CUMPLEAÑOS 10PCS OSIRIS ESPECIALES",
        ///                      "und": "UND",
        ///                      "empaque": 1,
        ///                      "detalle": "",
        ///                      "itbis": true,
        ///                      "porITBIS": 0.152542372881356,
        ///                      "precio": 60,
        ///                      "precioOferta": 60,
        ///                      "costo": 31.388,
        ///                      "xMayor1": 58,
        ///                      "xMayor2": 55.1,
        ///                      "xMayor3": 52.2,
        ///                      "precioMinimo": 49.3,
        ///                      "enOferta": 0,
        ///                      "areaID": 39,
        ///                      "dptoID": 23,
        ///                      "grupoID": 0,
        ///                      "area": "FIESTAS Y CUMPLEAÑOS",
        ///                      "dpto": "FIESTAS TEMATICAS",
        ///                      "grupo": null,
        ///                      "foto": "https://www.casamichelrd.com/imagenes\\23\\7460103900141.jpg",
        ///                      "inventario": 40,
        ///                      "pesado": false,
        ///                      "addCantidad": 1,
        ///                      "quantity": 1,
        ///                      "agregado": 0,
        ///                      "descripcionLarga": "",
        ///                      "codigoJuntos": "0",
        ///                      "codigoJuntos1": "0",
        ///                      "fotosAlterna": [
        ///                       "https://www.casamichelrd.com/imagenes\\23\\7460103900141.jpg"
        ///                      ],
        ///                      "productosLink": null,
        ///                      "link": 0
        ///                    }
        ///                 
        ///     }
        /// </remarks>
        [HttpGet]
        public async Task<IHttpActionResult> Get(string codigo)
        {
            string endpoint = ConfigurationManager.AppSettings["consultaazure"];
            
            var producto = new Producto();

            HttpClient http = new HttpClient();

            string url = endpoint + codigo;

            string data = await http.GetStringAsync(endpoint + codigo);

            producto = JsonConvert.DeserializeObject<Producto>(data);

            return Json(producto);
        }
    }
}



