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
        private readonly IConfiguration _configuration;
        public AbonoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("GetAllAbonos")]
        public string GetAbonos()
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Abono;", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Abono> abonoList = new List<Abono>();
            Response response = new Response();

            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    Abono abono = new Abono
                    {
                        idAbono = Convert.ToInt32(row["idAbono"]),
                        codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                        montoAbono = Convert.ToDecimal(row["montoAbono"]),
                        fechaAbono = Convert.ToDateTime(row["fechaAbono"])
                    };
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
                response.ErrorMessage = "No se encontraron abonos.";
                return JsonConvert.SerializeObject(response);
            }
        }

        [HttpPost("registrar-abono")]
        public IActionResult RegistrarAbono([FromBody] Abono abono)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // 1. Obtener información actual de la factura
                        SqlCommand cmdGetFactura = new SqlCommand(
                            "SELECT saldo FROM Factura WHERE codigoFactura = @codigoFactura",
                            con, transaction);
                        cmdGetFactura.Parameters.AddWithValue("@codigoFactura", abono.codigoFactura);

                        decimal saldoActual = (decimal)cmdGetFactura.ExecuteScalar();

                        // 2. Validar el monto del abono
                        if (abono.montoAbono > saldoActual)
                        {
                            transaction.Rollback();
                            return BadRequest("El monto del abono excede el saldo pendiente");
                        }

                        // 3. Registrar el abono
                        SqlCommand cmdInsertAbono = new SqlCommand(
                            @"INSERT INTO Abono (codigoFactura, montoAbono, fechaAbono) 
                      VALUES (@codigoFactura, @montoAbono, @fechaAbono)",
                            con, transaction);
                        cmdInsertAbono.Parameters.AddWithValue("@codigoFactura", abono.codigoFactura);
                        cmdInsertAbono.Parameters.AddWithValue("@montoAbono", abono.montoAbono);
                        cmdInsertAbono.Parameters.AddWithValue("@fechaAbono", abono.fechaAbono);
                        cmdInsertAbono.ExecuteNonQuery();

                        // 4. Actualizar el saldo de la factura
                        decimal nuevoSaldo = saldoActual - abono.montoAbono;
                        string nuevoEstado = nuevoSaldo <= 0 ? "Pagado" : "Pendiente";

                        SqlCommand cmdUpdateFactura = new SqlCommand(
                            "UPDATE Factura SET saldo = @saldo, estado = @estado WHERE codigoFactura = @codigoFactura",
                            con, transaction);
                        cmdUpdateFactura.Parameters.AddWithValue("@saldo", nuevoSaldo);
                        cmdUpdateFactura.Parameters.AddWithValue("@estado", nuevoEstado);
                        cmdUpdateFactura.Parameters.AddWithValue("@codigoFactura", abono.codigoFactura);
                        cmdUpdateFactura.ExecuteNonQuery();

                        transaction.Commit();

                        return Ok(new
                        {
                            success = true,
                            message = "Abono registrado exitosamente",
                            nuevoSaldo,
                            nuevoEstado
                        });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return StatusCode(500, $"Error al registrar el abono: {ex.Message}");
                    }
                
                
                
                
                
                }
           
            
            
            }
        
        
        
        
        
        
        }


        [HttpPut("actualizar-abono/{idAbono}")]
        public IActionResult ActualizarAbono(int idAbono, [FromBody] Abono abono)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            SqlDataAdapter checkAbono = new SqlDataAdapter("SELECT * FROM Abono WHERE idAbono = @idAbono", con);
            checkAbono.SelectCommand.Parameters.AddWithValue("@idAbono", idAbono);
            DataTable dt = new DataTable();
            checkAbono.Fill(dt);

            if (dt.Rows.Count == 0)
                return NotFound("No se encontró el abono a actualizar.");

            SqlCommand cmd = new SqlCommand(@"UPDATE Abono 
                                               SET codigoFactura = @codigoFactura,
                                                   montoAbono = @montoAbono,
                                                   fechaAbono = @fechaAbono
                                             WHERE idAbono = @idAbono", con);

            cmd.Parameters.AddWithValue("@codigoFactura", abono.codigoFactura);
            cmd.Parameters.AddWithValue("@montoAbono", abono.montoAbono);
            cmd.Parameters.AddWithValue("@fechaAbono", abono.fechaAbono);
            cmd.Parameters.AddWithValue("@idAbono", idAbono);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
                return Ok("Abono actualizado exitosamente.");
            else
                return StatusCode(500, "Error al actualizar el abono.");
        }

        [HttpDelete("eliminar-abono/{idAbono}")]
        public IActionResult EliminarAbono(int idAbono)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

            SqlDataAdapter checkAbono = new SqlDataAdapter("SELECT * FROM Abono WHERE idAbono = @idAbono", con);
            checkAbono.SelectCommand.Parameters.AddWithValue("@idAbono", idAbono);
            DataTable dt = new DataTable();
            checkAbono.Fill(dt);

            if (dt.Rows.Count == 0)
                return NotFound("No se encontró el abono a eliminar.");

            SqlCommand cmd = new SqlCommand("DELETE FROM Abono WHERE idAbono = @idAbono", con);
            cmd.Parameters.AddWithValue("@idAbono", idAbono);

            con.Open();
            int i = cmd.ExecuteNonQuery();
            con.Close();

            if (i > 0)
                return Ok("Abono eliminado exitosamente.");
            else
                return StatusCode(500, "Error al eliminar el abono.");
        }
    }
}
