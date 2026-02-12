using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class PrescribeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PrescribeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Prescribe([FromBody] Prescribe prsc)
        {
            if (prsc == null || prsc.UserId == Guid.Empty || prsc.diagnosisCode == Guid.Empty)
                return BadRequest("Invalid prescription data.");

            string connStr = _config.GetConnectionString("HU_DB");

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                using SqlCommand cmd = new("proc_InsertPrescription", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@created_by", prsc.UserId);
                cmd.Parameters.AddWithValue("@diagnosis_Code", prsc.diagnosisCode);
                cmd.Parameters.AddWithValue("@RX", prsc.RX);
                cmd.ExecuteNonQuery();
                return Ok(new
                {
                    Message =
                 "prescription inserted successfully",
                    diagnosisCode = prsc.diagnosisCode
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to insert prescription data",
                    Details = ex.Message
                });
            }
        }
    }
}
