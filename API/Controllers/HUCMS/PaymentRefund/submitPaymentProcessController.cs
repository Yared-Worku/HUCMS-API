using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class submitPaymentProcessController : ControllerBase
    {
        private readonly IConfiguration _config;

        public submitPaymentProcessController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult SubmitPaymentProcess([FromBody] submitPaymentProcess spp)
        {
            if (spp == null)
            {
                return BadRequest(new { Error = "Invalid payload." });
            }

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid applicationCode = Guid.Empty;
                Guid processDetailCode = Guid.Empty;
                Guid pr_Code = Guid.Empty;
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;
                using (SqlCommand cmdsd = new("proc_getStartDate", conn))
                {
                    cmdsd.CommandType = CommandType.StoredProcedure;
                    cmdsd.Parameters.AddWithValue("@todocode", spp.todocode);
                    cmdsd.Parameters.AddWithValue("@applicationNumber", spp.application_number);
                    object res = cmdsd.ExecuteScalar();
                    if (res == null || res == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(res);
                }
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);

                applicationCode = GetApplicationCode(conn, spp.application_number);
                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new { Error = "Application not found for the given application number." });
                }

                if (spp.ProcessDetailCode.GetValueOrDefault() == Guid.Empty)
                {
                    if (!spp.tasks_task_code.HasValue || spp.tasks_task_code.Value == Guid.Empty)
                    {
                        return BadRequest(new { Error = "tasks_task_code is required to generate a new ProcessDetailCode." });
                    }

                    processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, spp.tasks_task_code.Value);
                }
                else
                {
                    processDetailCode = spp.ProcessDetailCode.Value;
                }

                pr_Code = GetPrCodeFinance(conn, spp.todocode.Value);
                if (pr_Code == Guid.Empty)
                {
                    return NotFound(new { Error = "Financial reference (pr_code) not found." });
                }

                using SqlCommand cmd = new("proc_SubmitPaymentProcess", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@method_code", spp.method_code.HasValue ? spp.method_code.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@pr_code", pr_Code);
                cmd.Parameters.AddWithValue("@process_detail_code", processDetailCode);

                cmd.ExecuteNonQuery();
                UpdateTodoDetailId(conn, spp.application_number, processDetailCode);
                return Ok(new { Message = "Payment process submitted successfully." });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Database execution failed",
                    Details = ex.Message
                });
            }
        }
        private Guid GetPrCodeFinance(SqlConnection conn, Guid todocode)
        {
            using SqlCommand cmd = new("proc_GetPrCodeCh", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@todo_code", todocode);

            var result = cmd.ExecuteScalar();

            return result != DBNull.Value && result != null ? (Guid)result : Guid.Empty;
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
