using HUCMS.Models.HUCMS.MedicalProcess;
using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class CHRefundTaskDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CHRefundTaskDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult InsertTaskData([FromBody] CHRefundTaskData chr)
        {
            if (chr == null || chr.services_service_code == Guid.Empty)
                return BadRequest("Invalid task data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                // Declare required variables
                Guid applicationCode = Guid.Empty;
                string applicationNumber = chr.application_number; 
                Guid processDetailCode = Guid.Empty;
                Guid processDetailCodeOld = Guid.Empty;
                Guid pr_Code = Guid.Empty;
                Guid created_by = chr.UserId.Value;

                // Fetch application_code via stored procedure
                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "Application not found for the given application number."
                    });
                }
                processDetailCodeOld = GetProcessDetailCode(conn, applicationCode);
                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "detail_code not found for the given application_code."
                    });
                }

                // Continue insertion process
                //processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, dtd.tasks_task_code.Value);
                if (chr.process_detail_code.HasValue && chr.process_detail_code != Guid.Empty)
                {
                    processDetailCode = chr.process_detail_code.Value;
                    pr_Code = UpdateRefundDataHelper(conn, chr.amount_inWord, chr.amount_inDigit, processDetailCode);
                }
                else{
                    processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, chr.tasks_task_code.Value );
                    pr_Code = InsertApplicationProcessPaymentRefundData(conn, chr.amount_inWord, chr.amount_inDigit, processDetailCode, processDetailCodeOld);
                }
                // Update TodoDetailId using new helper method
                UpdateTodoDetailId(conn, applicationNumber, processDetailCode);

                return Ok(new
                {
                    Message = "✅ payment refund Task data inserted successfully",
                    ProcessDetailCode = processDetailCode,
                    pr_Code
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
        [HttpPut]
        public IActionResult UpdataApplicationProcessPaymentRefundData([FromBody] CHRefundTaskData chr)
        {
            if (chr == null || chr.services_service_code == Guid.Empty || !chr.process_detail_code.HasValue)
                return BadRequest("Invalid refund data for update");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid pr_code = UpdateRefundDataHelper(conn, chr.amount_inWord, chr.amount_inDigit, chr.process_detail_code.Value);
                return Ok(new
                {
                    Message = "✅ payment refund updated successfully",
                    pr_code = pr_code
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error occurred", Details = ex.Message });
            }
        }
        private Guid UpdateRefundDataHelper(SqlConnection conn, string amount_inWord, float? amount_inDigit, Guid processDetailCode)
        {
            using SqlCommand cmd = new("proc_UpdateCHRefundValidationData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@amount_in_word", amount_inWord ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@amount_in_number", amount_inDigit ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@detail_code", processDetailCode);
            SqlParameter outputParam = new("@pr_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);
            cmd.ExecuteNonQuery();
            return outputParam.Value != DBNull.Value ? (Guid)outputParam.Value : Guid.Empty;
        }

        //Helper method to fetch application_code using stored procedure
        private Guid GetApplicationCode(SqlConnection conn, string applicationNumber)
        {
            using SqlCommand cmd = new("proc_getApplicationCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);

            SqlParameter outputParam = new("@application_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return outputParam.Value != DBNull.Value ? (Guid)outputParam.Value : Guid.Empty;
        }

        private Guid GetProcessDetailCode(SqlConnection conn, Guid applicationCode)
        {
            using SqlCommand cmd2 = new("proc_GetProcessDetail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);

            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd2.Parameters.Add(outputParam);

            cmd2.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }
        private Guid InsertApplicationProcessDetail(SqlConnection conn, Guid applicationCode, Guid tasksTaskCode)
        {
            using SqlCommand cmd2 = new("proc_InsertApplicationProcessDetail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);
            cmd2.Parameters.AddWithValue("@tasks_task_code", tasksTaskCode);

            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd2.Parameters.Add(outputParam);

            cmd2.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }
        private Guid InsertApplicationProcessPaymentRefundData(SqlConnection conn, string amount_inWord, float? amount_inDigit, Guid? applicationProcessDetailsProcessDetailCode, Guid? processDetailCodeOld)
        {
            using SqlCommand cmd = new("proc_InsertCHRefundValidationData", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@amount_in_word", amount_inWord ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@amount_in_number", amount_inDigit);
            cmd.Parameters.AddWithValue("@processDetailCodeOld", processDetailCodeOld.Value);
            if (applicationProcessDetailsProcessDetailCode.HasValue)
                cmd.Parameters.AddWithValue("@application_process_details_code", applicationProcessDetailsProcessDetailCode.Value);
            else
                cmd.Parameters.AddWithValue("@application_process_details_code", DBNull.Value);

            SqlParameter outputParam = new("@pr_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);
             
            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        // helper method to update Todo Detail ID
        private void UpdateTodoDetailId(SqlConnection conn, string applicationNumber, Guid processDetailCode)
        {
            using SqlCommand cmd = new("proc_updateTodoDetailId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@process_detail_code", processDetailCode);

            cmd.ExecuteNonQuery();
        }
    }
}
