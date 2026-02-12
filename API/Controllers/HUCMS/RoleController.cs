using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HU_api.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IConfiguration _config;

        public RoleController(IConfiguration config)
        {
            _config = config;
        }

      
        [HttpGet]
        public IActionResult GetAllRoles()
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var Roles = new List<Role>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("GetAllRoles", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Roles.Add(NewMethod(reader));
                }

                return Ok(Roles);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch roles",
                    Details = ex.Message
                });
            }

            static Role NewMethod(SqlDataReader reader)
            {
                return new Role
                {
                    RoleId = reader["RoleId"] != DBNull.Value ? reader["RoleId"].ToString() : null,
                    RoleName = reader["RoleName"] != DBNull.Value ? reader["RoleName"].ToString() : null,
                    CreatedOnDate = reader["CreatedOnDate"]?.ToString(),
                };
            }
        }
      
    }
}
