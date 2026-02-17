using HUCMS.Models.HUCMS;
using HUCMS.Models.HUCMS.MedicalCertificate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalCertificate
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class IssueCertificateController : ControllerBase
    {
        private readonly IConfiguration _config;

        public IssueCertificateController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult InsetrLabResult([FromBody] IssueCertificate IS)
        {
            if (IS == null || IS.UserId == Guid.Empty)
                return BadRequest("Invalid certificate data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
             
                string applicationNumber = IS.application_number; 
                Guid processDetailCode = Guid.Empty;
                Guid created_by = IS.UserId.Value;
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;

                processDetailCode = InsertApplicationProcessCertificateData(conn, IS.health_profetional_recomendation, IS.patient_condition, created_by, IS.processDetailCode);

                using (SqlCommand cmd = new("proc_getStartDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@todocode", IS.todocode);
                    cmd.Parameters.AddWithValue("@applicationNumber", IS.application_number);
                    object res = cmd.ExecuteScalar();
                    if (res == null || res == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(res);
                }
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);

                Updatetodolistdetailid(conn, processDetailCode, applicationNumber);
                TodolistUpdate(conn, processDetailCode, endDate, elapsedTimeHours);

                return Ok(new
                {
                    Message = "✅ certificate data updated successfully",
                    ProcessDetailCode = processDetailCode
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Database error occurred",
                    Details = ex.Message
                });
            }
        }

        private Guid InsertApplicationProcessCertificateData(SqlConnection conn, string? health_profetional_recomendation, string? patient_condition, Guid created_by, Guid? processDetailCode)
        {
            using SqlCommand cmd = new("proc_UpdateApplicationprocessCertificatedata", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            // OUTPUT parameter (also used as input value)
            SqlParameter detailParam = new("@detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.InputOutput,
                Value = processDetailCode ?? Guid.Empty
            };
            cmd.Parameters.Add(detailParam);
            cmd.Parameters.AddWithValue("@health_profetional_recomendation", health_profetional_recomendation ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@patient_condition", patient_condition);
            cmd.Parameters.AddWithValue("@created_by", created_by);
         
            cmd.ExecuteNonQuery();

            return (Guid)detailParam.Value;
        }

        // helper method to update to do list
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
        private void Updatetodolistdetailid(SqlConnection conn, Guid processDetailCode, string? application_number)
        {
            using SqlCommand cmd = new("proc_UpdateToDoListCertificate", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_detail_id", processDetailCode);
            cmd.Parameters.AddWithValue("@application_number", application_number);
            cmd.ExecuteNonQuery();
        }
    }
}
