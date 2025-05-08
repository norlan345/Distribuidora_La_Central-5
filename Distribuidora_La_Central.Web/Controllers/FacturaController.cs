using Distribuidora_La_Central.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

namespace Distribuidora_La_Central.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturaController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public FacturaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllFacturas")]
        public string GetFacturas()
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Factura;", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Factura> facturaList = new List<Factura>();
            Response response = new Response();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Factura factura = new Factura();
                    factura.codigoFactura = Convert.ToInt32(dt.Rows[i]["codigoFactura"]);
                    factura.codigoCliente = Convert.ToInt32(dt.Rows[i]["codigoCliente"]);
                    factura.fecha = Convert.ToDateTime(dt.Rows[i]["fecha"]);
                    factura.totalFactura = Convert.ToDecimal(dt.Rows[i]["totalFactura"]);
                    factura.saldo = Convert.ToDecimal(dt.Rows[i]["saldo"]);
                    factura.tipo = Convert.ToString(dt.Rows[i]["tipo"]);
                    facturaList.Add(factura);
                }
            }

            if (facturaList.Count > 0)
            {
                return JsonConvert.SerializeObject(facturaList);
            }
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "No data found";
                return JsonConvert.SerializeObject(response);
            }
        }
    }
}