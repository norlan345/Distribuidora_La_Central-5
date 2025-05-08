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
    public class AbonoController : ControllerBase
    {
        public readonly IConfiguration _configuration;
        public AbonoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllAbonos")]
        public string GetAbonos()
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection").ToString());
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Abono;", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Abono> abonoList = new List<Abono>();
            Response response = new Response();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Abono abono = new Abono();
                    abono.idAbono = Convert.ToInt32(dt.Rows[i]["idAbono"]);
                    abono.codigoFactura = Convert.ToInt32(dt.Rows[i]["codigoFactura"]);
                    abono.montoAbono = Convert.ToDecimal(dt.Rows[i]["montoAbono"]);
                    abono.fechaAbono = Convert.ToDateTime(dt.Rows[i]["fechaAbono"]);
                    abonoList.Add(abono);
                }
            }
                
            if (abonoList.Count > 0)
            {
                return JsonConvert.SerializeObject(abonoList);
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