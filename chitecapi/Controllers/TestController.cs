using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Net.Http.Headers;

namespace chitecapi.Controllers
{
    public class TestController : ChitecApiController
    {
        
        // GET api/test
        public IEnumerable<string> Get()
        {
            return new string[] { "Hellow1", "Hellow2" };
        }


        // GET api/test/{value}
        [HttpGet]
        public IHttpActionResult Get(String id)
        {
            string connetionString;
            SqlConnection conection;
            connetionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            conection = new SqlConnection(connetionString);
            conection.Open();

            String sql = ConfigurationManager.AppSettings["consulta"];

            if (sql == null)
            {
                sql = "select * from productos where barCode = @item";
            }
           
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, conection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("@item", id);
            DataTable table = new DataTable();
            dataAdapter.Fill(table);


            conection.Close();



            return Json(table);
        }

      /*  // GET api/Image/{value}
        [HttpGet]
        public HttpResponseMessage Image(String id)
        {
            string connetionString;
            SqlConnection conection;
            connetionString = ConfigurationManager.ConnectionStrings["dbconnection"].ConnectionString;
            conection = new SqlConnection(connetionString);
            conection.Open();

            String sql = ConfigurationManager.AppSettings["consultaimagen"];

            if (sql == null)
            {
                sql = "select f_url_foto from articulos where codigo_barra = @item";
            }

            SqlDataAdapter dataAdapter = new SqlDataAdapter(sql, conection);
            dataAdapter.SelectCommand.Parameters.AddWithValue("@item", id);
            DataTable table = new DataTable();
            dataAdapter.Fill(table);

            
            String urlfoto ="";
            foreach  (DataRow row in table.Rows)
            {
                urlfoto = row[0].ToString();
            }
            conection.Close();


            var response = new HttpResponseMessage();
            if (!urlfoto.Equals(""))
            {
                response.Content = new StringContent($"<div><img src='{urlfoto}' ></ div>");
            }
            else
            {
                response.Content = new StringContent($"<div>The file dont have picture</ div>");
            }
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;

        }
      */
    }


}
