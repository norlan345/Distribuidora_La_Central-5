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
    public class DetalleFacturaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public DetalleFacturaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("obtener-por-factura/{idFactura}")]
        public string ObtenerDetallesPorFactura(int idFactura)
        {
            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataAdapter da = new(
                @"SELECT df.*, p.descripcion as nombreProducto 
                  FROM DetalleFactura df
                  INNER JOIN Producto p ON df.idProducto = p.idProducto
                  WHERE df.idFactura = @idFactura", con);

            da.SelectCommand.Parameters.AddWithValue("@idFactura", idFactura);

            DataTable dt = new();
            da.Fill(dt);

            List<DetalleFactura> detalles = new();
            Response response = new();

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DetalleFactura detalle = new()
                    {
                        idDetalle = Convert.ToInt32(dt.Rows[i]["idDetalle"]),
                        codigoFactura = Convert.ToInt32(dt.Rows[i]["idFactura"]),
                        codigoProducto = Convert.ToInt32(dt.Rows[i]["idProducto"]),
                     
                        cantidad = Convert.ToInt32(dt.Rows[i]["cantidad"]),
                        precioUnitario = Convert.ToDecimal(dt.Rows[i]["precioUnitario"]),
                        subtotal = Convert.ToDecimal(dt.Rows[i]["subtotal"])
                    };
                    detalles.Add(detalle);
                }
            }

            if (detalles.Count > 0)
                return JsonConvert.SerializeObject(detalles);
            else
            {
                response.StatusCode = 100;
                response.ErrorMessage = "No se encontraron detalles para esta factura.";
                return JsonConvert.SerializeObject(response);
            }
        }
    }
}