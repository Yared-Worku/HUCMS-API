using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetReferController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetReferController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{diagnosisCode}")]
        public IActionResult GetAppointment(Guid diagnosisCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetRefer>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getAReferalData", conn)
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
                    results.Add(new GetRefer
                    {
                        vitalSign = reader["vital_sign"] != DBNull.Value ? reader["vital_sign"].ToString() : null,
                        physicalExamination = reader["physical_examination"] != DBNull.Value ? reader["physical_examination"].ToString() : null,
                        referalReason = reader["reason_for_referal"] != DBNull.Value ? reader["reason_for_referal"].ToString() : null,
                        ref_Code = reader["ref_Code"] != DBNull.Value ? (Guid?)reader["ref_Code"] : null,
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
