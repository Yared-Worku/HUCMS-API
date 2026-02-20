using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class ReferalDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ReferalDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult referal([FromBody] ReferalData rf)
        {
            if (rf == null || rf.UserId == Guid.Empty || rf.diagnosisCode == Guid.Empty)
                return BadRequest("Invalid referal data.");

            string connStr = _config.GetConnectionString("HU_DB");

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                using SqlCommand cmd = new("proc_InsertReferalData", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@ref_Code", SqlDbType.UniqueIdentifier).Value = rf.refCode.HasValue? rf.refCode.Value: DBNull.Value;
                cmd.Parameters.AddWithValue("@created_by", rf.UserId);
                cmd.Parameters.AddWithValue("@diagnosis_Code", rf.diagnosisCode);
                cmd.Parameters.AddWithValue("@vital_sign", rf.vitalSign);
                cmd.Parameters.AddWithValue("@physical_examination", rf.physicalExamination);
                cmd.Parameters.AddWithValue("@reason_for_referal", rf.referalReason);

                cmd.ExecuteNonQuery();
                return Ok(new
                {
                    Message = rf.refCode.HasValue
                ? "refer updated successfully"
                : "refer inserted successfully",
                    ref_Code = rf.refCode
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to insert referal data",
                    Details = ex.Message
                });
            }
        }

    }
}
