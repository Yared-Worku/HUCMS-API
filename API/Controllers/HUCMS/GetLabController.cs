using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetLabController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetLabController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{diagnosisCode}")]
        public IActionResult GetLab(Guid diagnosisCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetLab>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getLabData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@diagnosis_Code", diagnosisCode);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new GetLab
                    {
                        lab_test = reader["lab_test"] != DBNull.Value ? reader["lab_test"].ToString() : null,
                        lab_result = reader["lab_result"] != DBNull.Value ? reader["lab_result"].ToString() : null,
                        lab_Code = reader["lab_Code"] != DBNull.Value ? (Guid?)reader["lab_Code"] : null,
                        detail_code = reader["detail_code"] != DBNull.Value ? (Guid?)reader["detail_code"] : null
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this diagnosisCode." });
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
