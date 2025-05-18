using Distribuidora_La_Central.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Distribuidora_La_Central.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreditoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CreditoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: api/Credito/GetAllCreditos
        [HttpGet("GetAllCreditos")]
        public async Task<IActionResult> GetAllCreditos()
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT * FROM Credito";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();

                await Task.Run(() => da.Fill(dt));

                var creditos = new System.Collections.Generic.List<Credito>();

                foreach (DataRow row in dt.Rows)
                {
                    creditos.Add(new Credito
                    {
                        idCredito = Convert.ToInt32(row["idCredito"]),
                        codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                        fechaInicial = Convert.ToDateTime(row["fechaInicial"]),
                        fechaFinal = Convert.ToDateTime(row["fechaFinal"]),
                        saldoMaximo = Convert.ToDecimal(row["saldoMaximo"]),
                        estado = row["estado"].ToString()
                    });
                }

                return Ok(creditos);
            }
        }

        // GET: api/Credito/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCredito(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT * FROM Credito WHERE idCredito = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                await con.OpenAsync();
                SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (reader.Read())
                {
                    var credito = new Credito
                    {
                        idCredito = Convert.ToInt32(reader["idCredito"]),
                        codigoFactura = Convert.ToInt32(reader["codigoFactura"]),
                        fechaInicial = Convert.ToDateTime(reader["fechaInicial"]),
                        fechaFinal = Convert.ToDateTime(reader["fechaFinal"]),
                        saldoMaximo = Convert.ToDecimal(reader["saldoMaximo"]),
                        estado = reader["estado"].ToString()
                    };

                    return Ok(credito);
                }

                return NotFound();
            }
        }

        // POST: api/Credito
        [HttpPost]
        public async Task<IActionResult> CreateCredito([FromBody] Credito credito)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"INSERT INTO Credito 
                                (codigoFactura, fechaInicial, fechaFinal, saldoMaximo, estado) 
                                VALUES (@CodigoFactura, @FechaInicial, @FechaFinal, @SaldoMaximo, @Estado);
                                SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CodigoFactura", credito.codigoFactura);
                cmd.Parameters.AddWithValue("@FechaInicial", credito.fechaInicial);
                cmd.Parameters.AddWithValue("@FechaFinal", credito.fechaFinal);
                cmd.Parameters.AddWithValue("@SaldoMaximo", credito.saldoMaximo);
                cmd.Parameters.AddWithValue("@Estado", credito.estado ?? "Activo");

                await con.OpenAsync();
                int newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                credito.idCredito = newId;
                return CreatedAtAction(nameof(GetCredito), new { id = newId }, credito);
            }
        }

        // PUT: api/Credito/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCredito(int id, [FromBody] Credito credito)
        {
            if (id != credito.idCredito)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"UPDATE Credito SET 
                                codigoFactura = @CodigoFactura, 
                                fechaInicial = @FechaInicial, 
                                fechaFinal = @FechaFinal, 
                                saldoMaximo = @SaldoMaximo, 
                                estado = @Estado 
                                WHERE idCredito = @Id";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@CodigoFactura", credito.codigoFactura);
                cmd.Parameters.AddWithValue("@FechaInicial", credito.fechaInicial);
                cmd.Parameters.AddWithValue("@FechaFinal", credito.fechaFinal);
                cmd.Parameters.AddWithValue("@SaldoMaximo", credito.saldoMaximo);
                cmd.Parameters.AddWithValue("@Estado", credito.estado);

                await con.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        // DELETE: api/Credito/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCredito(int id)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "DELETE FROM Credito WHERE idCredito = @Id";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Id", id);

                await con.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
        }

        // GET: api/Credito/GetCreditosActivos
        [HttpGet("GetCreditosActivos")]
        public async Task<IActionResult> GetCreditosActivos()
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT * FROM Credito WHERE estado = 'Activo'";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();

                await Task.Run(() => da.Fill(dt));

                var creditos = new System.Collections.Generic.List<Credito>();

                foreach (DataRow row in dt.Rows)
                {
                    creditos.Add(new Credito
                    {
                        idCredito = Convert.ToInt32(row["idCredito"]),
                        codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                        fechaInicial = Convert.ToDateTime(row["fechaInicial"]),
                        fechaFinal = Convert.ToDateTime(row["fechaFinal"]),
                        saldoMaximo = Convert.ToDecimal(row["saldoMaximo"]),
                        estado = row["estado"].ToString()
                    });
                }

                return Ok(creditos);
            }
        }

        // GET: api/Credito/GetCreditosPorFactura/5
        [HttpGet("GetCreditosPorFactura/{facturaId}")]
        public async Task<IActionResult> GetCreditosPorFactura(int facturaId)
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT * FROM Credito WHERE codigoFactura = @FacturaId";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FacturaId", facturaId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();

                await Task.Run(() => da.Fill(dt));

                var creditos = new System.Collections.Generic.List<Credito>();

                foreach (DataRow row in dt.Rows)
                {
                    creditos.Add(new Credito
                    {
                        idCredito = Convert.ToInt32(row["idCredito"]),
                        codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                        fechaInicial = Convert.ToDateTime(row["fechaInicial"]),
                        fechaFinal = Convert.ToDateTime(row["fechaFinal"]),
                        saldoMaximo = Convert.ToDecimal(row["saldoMaximo"]),
                        estado = row["estado"].ToString()
                    });
                }

                return Ok(creditos);
            }
        }
    }
}