using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetJsonDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetJsonDataController(IConfiguration config)
        {
            _config = config;
        }
        [HttpGet("{detailId}")]
        public IActionResult GetUserID(string detailId)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var id = new List<GetJsonData>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetUJsonData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@detailId", detailId);
            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id.Add(new GetJsonData
                    {
                        value = reader["value"] != DBNull.Value ? reader["value"].ToString() : null,
                    });
                }

                return Ok(id);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch jsondata",
                    Details = ex.Message
                });
            }
        }
    }
}
