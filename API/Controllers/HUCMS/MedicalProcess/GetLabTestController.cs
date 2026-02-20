using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetLabTestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetLabTestController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{todocode}")]
        public IActionResult GetLabTest(Guid todocode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetLabTest>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getLabTestData", conn)
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
                    results.Add(new GetLabTest
                    {
                        FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : null,
                        LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : null,
                        lab_test = reader["lab_test"] != DBNull.Value ? reader["lab_test"].ToString() : null,
                        lab_result = reader["lab_result"] != DBNull.Value ? reader["lab_result"].ToString() : null,
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
