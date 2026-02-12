using HU_api.Entities.HU;
using HUCMS.Entities.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class studentdepartmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public studentdepartmentController(IConfiguration config)
        {
            _config = config;
        }
        // ✅ Get All students Department
        [HttpGet]
        public IActionResult GetAllDepartments()
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var departments = new List<studentdepartment>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetStudentsDepartment", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    departments.Add(new studentdepartment
                    {
                        depcode = reader["depcode"] != DBNull.Value ? reader["depcode"].ToString() : null,
                        depname = reader["depname"] != DBNull.Value ? reader["depname"].ToString() : null,
                      
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
