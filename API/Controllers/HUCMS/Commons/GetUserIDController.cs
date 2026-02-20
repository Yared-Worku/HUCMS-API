using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;


namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetUserIDController : ControllerBase
    {
            private readonly IConfiguration _config;

            public GetUserIDController(IConfiguration config)
            {
                _config = config;
            }
            [HttpGet("{username}")]
        public IActionResult GetUserID(string username)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var id = new List<UserId>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetUserID", conn)
            {
                CommandType = CommandType.StoredProcedure 
            };
            cmd.Parameters.AddWithValue("@Username", username);
            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    id.Add(new UserId
                    {
                        userid = reader["userID"] != DBNull.Value ? reader["userID"].ToString() : null,
                    });
                }

                return Ok(id);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch userid",
                    Details = ex.Message
                });
            }
        }
    }
}
