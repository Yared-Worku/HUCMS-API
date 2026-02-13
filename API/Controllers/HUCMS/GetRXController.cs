using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetRXController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetRXController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{todocode}")]
        public IActionResult GetRX(Guid todocode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetRX>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getRX", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@to_do_code", todocode);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new GetRX
                    {
                        FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : null,
                        LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : null,
                        rX = reader["RX"] != DBNull.Value ? reader["RX"].ToString() : null,
                        detail_code = reader["detail_code"] != DBNull.Value ? (Guid)reader["detail_code"] : null
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this todocode." });
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch data",
                    Details = ex.Message
                });
            }
        }
    }
}
