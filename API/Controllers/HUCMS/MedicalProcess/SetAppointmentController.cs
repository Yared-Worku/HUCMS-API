using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class SetAppointmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public SetAppointmentController(IConfiguration config)
        {
            _config = config;
        }
        [HttpPost]
        public IActionResult SetAppointment([FromBody] SetAppointment apoint)
        {
            if (apoint == null || apoint.UserId == Guid.Empty || apoint.diagnosisCode == Guid.Empty)
                return BadRequest("Invalid appointment data.");

            string connStr = _config.GetConnectionString("HU_DB");

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                using SqlCommand cmd = new("proc_SetAppointment", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.Add("@appointment_Code", SqlDbType.UniqueIdentifier)
        .Value = apoint.appointment_Code.HasValue
         ? apoint.appointment_Code.Value
         : DBNull.Value;

                cmd.Parameters.AddWithValue("@diagnosis_Code", apoint.diagnosisCode);
                cmd.Parameters.AddWithValue("@created_by", apoint.UserId);

                cmd.Parameters.AddWithValue("@created_date", DBNull.Value);

                cmd.Parameters.AddWithValue("@appointment_date",
                    DateOnly.Parse(apoint.appointment_date.ToString()));

                cmd.Parameters.AddWithValue("@appointment_reason",
                    apoint.appointment_reason ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = apoint.appointment_Code.HasValue
                ? "Appointment updated successfully"
                : "Appointment inserted successfully",
                    apoint.appointment_Code
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "Failed to process appointment",
                    Details = ex.Message
                });
            }
        }

    }
}
