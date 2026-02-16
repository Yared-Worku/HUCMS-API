using HUCMS.Models.HUCMS.MedicalCertificate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalCertificate
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetcertificateController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetcertificateController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{userId}")]
        public IActionResult Getcertificate(Guid userId)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<Getcertificate>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getcetrificateDiagnosisRelated", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new Getcertificate
                    {
                        application_number = reader["application_number"] != DBNull.Value ? reader["application_number"].ToString() : null,
                        diagnosis_code = reader["diagnosis_code"] != DBNull.Value ? (Guid)reader["diagnosis_code"] : null
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this UserId." });
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
