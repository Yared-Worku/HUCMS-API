using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetDiagnosisController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetDiagnosisController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{application_detail_id}")]
        public IActionResult GetDiagnosis(Guid application_detail_id)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetDiagnosis>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getDiagnosisData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@application_detail_id", application_detail_id);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new GetDiagnosis
                    {
                        diagnosis_Code = reader["diagnosis_Code"] != DBNull.Value ? (Guid?)reader["diagnosis_Code"]: null, 
                        detail_diagnosis = reader["detail_diagnosis"] != DBNull.Value ? reader["detail_diagnosis"].ToString() : null,
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this application_detail_id." });
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
