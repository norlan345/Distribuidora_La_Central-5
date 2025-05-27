using Distribuidora_La_Central.Web.Models;
using Microsoft.AspNetCore.Http;
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
        private readonly IConfiguration _configuration;

        public FacturaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("GetAllFacturas")]
        public IActionResult GetAllFacturas()
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Factura", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            List<Factura> facturas = new List<Factura>();

            foreach (DataRow row in dt.Rows)
            {
                facturas.Add(new Factura
                {
                    codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                    codigoCliente = Convert.ToInt32(row["codigoCliente"]),
                    fecha = Convert.ToDateTime(row["fecha"]),
                    totalFactura = Convert.ToDecimal(row["totalFactura"]),
                    saldo = Convert.ToDecimal(row["saldo"]),
                    tipo = row["tipo"].ToString(),
                  
                });
            }

            return Ok(facturas);
        }

        [HttpGet("GetFacturaPorCodigo/{codigo}")]
        public IActionResult GetFacturaPorCodigo(int codigo)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Factura WHERE codigoFactura = @codigo", con);
            da.SelectCommand.Parameters.AddWithValue("@codigo", codigo);

            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count == 0)
                return NotFound();

            DataRow row = dt.Rows[0];

            var factura = new Factura
            {
                codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                codigoCliente = Convert.ToInt32(row["codigoCliente"]),
                fecha = Convert.ToDateTime(row["fecha"]),
                totalFactura = Convert.ToDecimal(row["totalFactura"]),
                saldo = Convert.ToDecimal(row["saldo"]),
                tipo = row["tipo"].ToString(),
              
            };

            return Ok(factura);
        }

        [HttpPost("AgregarFactura")]
        public IActionResult AgregarFactura([FromBody] FacturaConDetalles facturaConDetalles)
        {
            try
            {
                // Validar el modelo recibido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                con.Open();
                using SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    // Validar tipo de factura
                    if (facturaConDetalles.Factura.tipo != "Contado" && facturaConDetalles.Factura.tipo != "Crédito")
                    {
                        return BadRequest(new { error = "Tipo de factura no válido. Debe ser 'Contado' o 'Crédito'" });
                    }

                    // 1. Insertar la factura
                    string queryFactura = @"INSERT INTO Factura 
                                (codigoCliente, fecha, totalFactura, saldo, tipo, estado)
                                OUTPUT INSERTED.codigoFactura
                                VALUES (@codigoCliente, @fecha, @totalFactura, @saldo, @tipo, @estado)";

                    int codigoFactura;
                    using (SqlCommand cmd = new SqlCommand(queryFactura, con, transaction))
                    {
                        cmd.Parameters.AddWithValue("@codigoCliente", facturaConDetalles.Factura.codigoCliente);
                        cmd.Parameters.AddWithValue("@fecha", facturaConDetalles.Factura.fecha);
                        cmd.Parameters.AddWithValue("@totalFactura", facturaConDetalles.Factura.totalFactura);

                        // Establecer saldo inicial
                        decimal saldoInicial = facturaConDetalles.Factura.tipo == "Contado" ? 0 : facturaConDetalles.Factura.totalFactura;
                        cmd.Parameters.AddWithValue("@saldo", saldoInicial);

                        cmd.Parameters.AddWithValue("@tipo", facturaConDetalles.Factura.tipo);

                        // Estado de la factura
                        string estadoFactura = facturaConDetalles.Factura.tipo == "Contado" ? "Pagado" : "Pendiente";
                        cmd.Parameters.AddWithValue("@estado", estadoFactura);

                        codigoFactura = (int)cmd.ExecuteScalar();
                    }

                    // 2. Insertar los detalles de la factura
                    string queryDetalle = @"INSERT INTO DetalleFactura 
                                (codigoFactura, codigoProducto, cantidad, precioUnitario, subtotal)
                                VALUES (@codigoFactura, @codigoProducto, @cantidad, @precioUnitario, @subtotal)";

                    foreach (var detalle in facturaConDetalles.Detalles)
                    {
                        using SqlCommand cmd = new SqlCommand(queryDetalle, con, transaction);
                        cmd.Parameters.AddWithValue("@codigoFactura", codigoFactura);
                        cmd.Parameters.AddWithValue("@codigoProducto", detalle.codigoProducto);
                        cmd.Parameters.AddWithValue("@cantidad", detalle.cantidad);
                        cmd.Parameters.AddWithValue("@precioUnitario", detalle.precioUnitario);
                        cmd.Parameters.AddWithValue("@subtotal", detalle.cantidad * detalle.precioUnitario);

                        cmd.ExecuteNonQuery();
                    }

                    // 3. Si es crédito, insertar en la tabla de créditos
                    if (facturaConDetalles.Factura.tipo == "Crédito")
                    {
                        // Calcular fecha de vencimiento (30 días después como ejemplo)
                        DateTime fechaVencimiento = facturaConDetalles.Factura.fecha.AddDays(30);

                        string queryCredito = @"INSERT INTO Credito 
                                    (codigoFactura, fechaInicial, fechaFinal, saldoMaximo, estado)
                                    VALUES 
                                    (@codigoFactura, @fechaInicial, @fechaFinal, @saldoMaximo, @estado)";

                        using SqlCommand cmd = new SqlCommand(queryCredito, con, transaction);
                        cmd.Parameters.AddWithValue("@codigoFactura", codigoFactura);
                        cmd.Parameters.AddWithValue("@fechaInicial", facturaConDetalles.Factura.fecha);
                        cmd.Parameters.AddWithValue("@fechaFinal", fechaVencimiento);
                        cmd.Parameters.AddWithValue("@saldoMaximo", facturaConDetalles.Factura.totalFactura);
                        cmd.Parameters.AddWithValue("@estado", "Activo"); // Estado inicial para créditos nuevos

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Ok(new
                    {
                        codigoFactura,
                        message = $"Factura registrada exitosamente como {facturaConDetalles.Factura.tipo}"
                    });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, new
                    {
                        error = "Error al registrar la factura",
                        details = ex.Message,
                        innerException = ex.InnerException?.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error de conexión con la base de datos",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("EliminarFactura/{id}")]
        public IActionResult EliminarFactura(int id)
        {
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = "DELETE FROM Factura WHERE codigoFactura = @id";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@id", id);
            con.Open();
            int rows = cmd.ExecuteNonQuery();
            return Ok(rows > 0);
        }

        [HttpPut("ActualizarFactura/{id}")]
        public IActionResult ActualizarFactura(int id, [FromBody] Factura factura)
        {
            // Set default estado if not provided
          
            using SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            string query = @"UPDATE Factura SET 
                                codigoCliente = @codigoCliente,
                                fecha = @fecha,
                                totalFactura = @totalFactura,
                                saldo = @saldo,
                                tipo = @tipo,
                                estado = @estado
                             WHERE codigoFactura = @codigoFactura";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@codigoFactura", id);
            cmd.Parameters.AddWithValue("@codigoCliente", factura.codigoCliente);
            cmd.Parameters.AddWithValue("@fecha", factura.fecha);
            cmd.Parameters.AddWithValue("@totalFactura", factura.totalFactura);
            cmd.Parameters.AddWithValue("@saldo", factura.saldo);
            cmd.Parameters.AddWithValue("@tipo", factura.tipo);
          

            con.Open();
            int rows = cmd.ExecuteNonQuery();
            if (rows > 0)
                return Ok(new { message = "Factura actualizada correctamente" });
            else
                return NotFound(new { message = "Factura no encontrada" });
        }

        [HttpGet("GetFacturasPendientes")]
        public IActionResult GetFacturasPendientes()
        {
            using (SqlConnection con = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = @"SELECT * FROM Factura 
                        WHERE saldo > 0 
                        AND (estado IS NULL OR estado != 'Pagado')";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<Factura> facturas = new List<Factura>();

                foreach (DataRow row in dt.Rows)
                {
                    facturas.Add(new Factura
                    {
                        codigoFactura = Convert.ToInt32(row["codigoFactura"]),
                        totalFactura = Convert.ToDecimal(row["totalFactura"]),
                        saldo = Convert.ToDecimal(row["saldo"]),
                       
                    });
                }

                return Ok(facturas);
            }
        }

        public class FacturaConDetalles
        {
            public Factura Factura { get; set; }
            public List<DetalleFactura> Detalles { get; set; }
        }
    }
}