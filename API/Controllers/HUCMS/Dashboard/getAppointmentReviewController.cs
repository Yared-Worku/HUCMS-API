using HUCMS.Models.HUCMS.Dashboard;
using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Dashboard
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getAppointmentReviewController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getAppointmentReviewController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{userId}")]
        public IActionResult getAppointment(Guid userId)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<getAppointmentReview>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getAppointmentReiew", conn)
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
                    results.Add(new getAppointmentReview
                    {
                        DoctorFName = reader["DoctorFName"] != DBNull.Value ? reader["DoctorFName"].ToString() : null,
                        appointment_date = reader["appointment_date"] != DBNull.Value ? DateOnly.FromDateTime((DateTime)reader["appointment_date"]): null,
                        DoctorLName = reader["DoctorLName"] != DBNull.Value ? reader["DoctorLName"].ToString() : null,
                        application_number = reader["application_number"] != DBNull.Value ? reader["application_number"].ToString() : null
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
