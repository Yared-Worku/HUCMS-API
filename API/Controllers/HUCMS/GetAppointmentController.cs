using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetAppointmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetAppointmentController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{diagnosisCode}")]
        public IActionResult GetAppointment(Guid diagnosisCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetAppointment>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getAppointmentData", conn)
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
                    results.Add(new GetAppointment
                    {
                        appointment_date = reader["appointment_date"] != DBNull.Value ? DateOnly.FromDateTime((DateTime)reader["appointment_date"]) : null,

                        appointment_reason = reader["appointment_reason"] != DBNull.Value ? reader["appointment_reason"].ToString() : null,
                        appointment_Code = reader["appointment_Code"] != DBNull.Value ? (Guid?)reader["appointment_Code"] : null,
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
