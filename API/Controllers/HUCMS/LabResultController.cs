using Azure.Core;
using BPM.Entities.HU;
using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class LabResultController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LabResultController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult InsetrLabResult([FromBody] LabResult result)
        {
            if (result == null || result.UserId == Guid.Empty)
                return BadRequest("Invalid lab result data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                // Declare required variables
                Guid diagnosisCode = Guid.Empty;
                string applicationNumber = result.application_number; // Sent from UI
                Guid processDetailCode = Guid.Empty;
                Guid labCode = Guid.Empty;
                Guid labtechnician_Code = result.UserId.Value;
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;

                //Fetch diagnosisCode
                labCode = GetLabCode(conn, applicationNumber);

                if (labCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "Laboratory request not found for the given application number."
                    });
                }

                processDetailCode = InsertApplicationProcessLabResultData(conn, result.lab_result, labtechnician_Code, labCode);

                using (SqlCommand cmd = new("proc_getStartDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@todocode", result.todocode);
                    cmd.Parameters.AddWithValue("@applicationNumber", result.application_number);
                    object res = cmd.ExecuteScalar();
                    if (res == null || res == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(res);
                }
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);

                TodolistUpdate(conn, processDetailCode, endDate, elapsedTimeHours);
               
                return Ok(new
                {
                    Message = "✅ Lab reslt data inserted successfully",
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

        //Helper method to fetch labCode
        private Guid GetLabCode(SqlConnection conn, string? applicationNumber)
        {
            using SqlCommand cmd = new("proc_getLabCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber);

            SqlParameter outputParam = new("@lab_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return outputParam.Value != DBNull.Value
                ? (Guid)outputParam.Value
                : Guid.Empty;
        }

        private Guid InsertApplicationProcessLabResultData(SqlConnection conn, string? lab_result, Guid? labtechnician_Code,Guid labCode)
        {
            using SqlCommand cmd = new("proc_InsertLabResult", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@lab_result", lab_result ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@lab_Code", labCode);
            cmd.Parameters.AddWithValue("@labtechnician_Code", labtechnician_Code);

            SqlParameter outputParam = new("@detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        // helper method to update to do list
        private void TodolistUpdate(SqlConnection conn, Guid processDetailCode, DateTime endDate, Decimal elapsedTimeHours)
        {
            using SqlCommand cmd = new("proc_UpdateToDoListLabTest", conn)
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
