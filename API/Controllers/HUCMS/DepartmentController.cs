using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;

namespace HU_api.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DepartmentController(IConfiguration config)
        {
            _config = config;
        }

       
        // ✅ Get All Departments
        [HttpGet]
        public IActionResult GetAllDepartments()
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var departments = new List<Department>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_GetAllDepartments", conn) 
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    departments.Add(new Department
                    {
                        depCode = reader["depCode"] != DBNull.Value ? reader["depCode"].ToString() : null,
                        depName = reader["depName"] != DBNull.Value ? reader["depName"].ToString() : null,
                        orgCode = reader["orgCode"] != DBNull.Value ? reader["orgCode"].ToString() : null,
                        created_by = reader["created_by"] != DBNull.Value ? Guid.Parse(reader["created_by"].ToString()) : null,
                        created_date = reader["created_date"] != DBNull.Value ? (DateTime?)reader["created_date"] : null,
                        is_active = reader["is_active"] != DBNull.Value ? (bool?)reader["is_active"] : null,
                        // is_published = reader["is_published"] != DBNull.Value ? (bool?)reader["is_published"] : null,
                    });
                }

                return Ok(departments);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Failed to fetch Departments", Details = ex.Message });
            }
        }
    }
}
