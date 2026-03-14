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
            conn.Open();
            try
            {
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;
                using (SqlCommand cmdsd = new("proc_getStartDate", conn))
                {
                    cmdsd.CommandType = CommandType.StoredProcedure;
                    cmdsd.Parameters.AddWithValue("@todocode", rf.todocode);
                    cmdsd.Parameters.AddWithValue("@applicationNumber", rf.application_number);
                    object res = cmdsd.ExecuteScalar();
                    if (res == null || res == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(res);
                }
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);


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
                TodolistUpdate(conn, rf.processDetailCode.Value, endDate, elapsedTimeHours);
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
        private void TodolistUpdate(SqlConnection conn, Guid processDetailCode, DateTime endDate, Decimal elapsedTimeHours)
        {
            using SqlCommand cmd = new("proc_UpdateToDoListCertificateToClosed", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_detail_id", processDetailCode);
            cmd.Parameters.AddWithValue("@end_date", endDate);
            cmd.Parameters.AddWithValue("@elapsed_time_hours", elapsedTimeHours);
            cmd.ExecuteNonQuery();
        }

    }
}
