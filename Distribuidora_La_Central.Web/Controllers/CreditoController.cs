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
    public class CreditoController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public CreditoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllCreditos")]
        public string GetCreditos()
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Credito;", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Credito> creditoList = new List<Credito>();
            Response response = new Response();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Credito credito = new Credito();
                    credito.idCredito = Convert.ToInt32(dt.Rows[i]["idCredito"]);
                    credito.codigoFactura = Convert.ToInt32(dt.Rows[i]["codigoFactura"]);
                    credito.fechaInicial = Convert.ToDateTime(dt.Rows[i]["fechaInicial"]);
                    credito.fechaFinal = Convert.ToDateTime(dt.Rows[i]["fechaFinal"]);
                    credito.saldoMaximo = Convert.ToDecimal(dt.Rows[i]["saldoMaximo"]);
                    credito.estado = Convert.ToString(dt.Rows[i]["estado"]);
                    creditoList.Add(credito);
                }
            }

            if (creditoList.Count > 0)
            {
                return JsonConvert.SerializeObject(creditoList);
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