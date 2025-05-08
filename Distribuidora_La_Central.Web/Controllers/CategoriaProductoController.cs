using Distribuidora_La_Central.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Text.Json.Serialization;


[Route("api/[controller]")]
[ApiController]
public class CategoriaProductoController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public CategoriaProductoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("obtener-todas-categorias")]
    public IActionResult ObtenerTodasCategorias()
    {
        try
        {
            using SqlConnection con = new(_configuration.GetConnectionString("DefaultConnection"));
            SqlDataAdapter da = new("SELECT * FROM CategoriaProducto", con);
            DataTable dt = new DataTable();
            da.Fill(dt);

            List<CategoriaProducto> categoriaList = new();

            foreach (DataRow row in dt.Rows)
            {
                categoriaList.Add(new CategoriaProducto
                {
                    idCategoria = Convert.ToInt32(row["idCategoria"]),
                    nombre = row["nombre"].ToString(),
                    descripcion = row["descripcion"].ToString()
                });
            }

            return Ok(categoriaList);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                StatusCode = 500,
                Message = "Error al obtener categorías",
                Error = ex.Message
            });
        }
    }
}