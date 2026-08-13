using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class DepCodeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepCodeController(IConfiguration config)
        {
            _config = config;
        }

        // ✅ Get Departments by User ID
        // Route matches React: api/HU/DepCode/{userid}
        [HttpGet("{userid}")]
        public IActionResult GetDepCodes(Guid userid)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var departments = new List<DepCode>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_getDepcode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            // Add the parameter that the React app is sending 
            cmd.Parameters.AddWithValue("@UserID", userid);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    departments.Add(new DepCode
                    {
                        depCode = reader["SDP_ID"] != DBNull.Value ? reader["SDP_ID"].ToString() : null,
                        orgCode = reader["orgCode"] != DBNull.Value ? reader["orgCode"].ToString() : null,
                    });
                }

                return Ok(departments);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ SQL Error", Details = ex.Message });
            }
        }
    }
}