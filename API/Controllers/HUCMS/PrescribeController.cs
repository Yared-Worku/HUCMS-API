using BPM.Entities.HU;
using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
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
            try
            {
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;
                Guid next_task;
                Guid applicationCode = Guid.Empty;
                Guid processDetailCode = Guid.Empty;
                string applicationNumber = prsc.application_number;
                using SqlConnection conn = new(connStr);
                conn.Open();
                // Fetch application_code via stored procedure
                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "Application not found for the given application number."
                    });
                }
                // Get start_date
                using (SqlCommand cmd = new("proc_getStartDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@todocode", prsc.todocode);
                    cmd.Parameters.AddWithValue("@applicationNumber", prsc.application_number);
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(result);
                }
                //Get nexttask
                using (SqlCommand cmd = new("proc_getNextTask", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@task_code", prsc.tasks_task_code);

                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return NotFound(new { Message = "next task not found." });

                    next_task = (Guid)result;
                }

                // Calculate elapsed hours
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);
                processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, prsc.tasks_task_code.Value);
                using SqlCommand InsertCmd = new("proc_InsertPrescription", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                InsertCmd.Parameters.AddWithValue("@created_by", prsc.UserId);
                InsertCmd.Parameters.AddWithValue("@diagnosis_Code", prsc.diagnosisCode);
                InsertCmd.Parameters.AddWithValue("@RX", prsc.RX);
                InsertCmd.Parameters.AddWithValue("@detail_code", processDetailCode);
                InsertCmd.ExecuteNonQuery();

                // Update old row
                using (SqlCommand updateCmd = new("proc_updateTodoListTask", conn))
                {
                    updateCmd.CommandType = CommandType.StoredProcedure;
                    updateCmd.Parameters.AddWithValue("@todocode", prsc.todocode);
                    updateCmd.Parameters.AddWithValue("@end_date", endDate);
                    updateCmd.Parameters.AddWithValue("@elapsed_time_hours", elapsedTimeHours);
                    updateCmd.Parameters.AddWithValue("@application_detail_id", (object?)prsc.ProcessDetailCode ?? DBNull.Value);
                    updateCmd.ExecuteNonQuery();
                }
                // Insert new row
                Guid newToDoCode = Guid.NewGuid();
                using (SqlCommand insertCmd = new("proc_insertToDolist", conn))
                {
                    insertCmd.CommandType = CommandType.StoredProcedure;
                    //insertCmd.Parameters.AddWithValue("@old_todocode", todocode);
                    insertCmd.Parameters.AddWithValue("@new_todocode", newToDoCode);
                    insertCmd.Parameters.AddWithValue("@new_start_date", DateTime.Now);
                    insertCmd.Parameters.AddWithValue("@jumpfrom", (object?)prsc.todocode ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@tasks_task_code", next_task);
                    insertCmd.Parameters.AddWithValue("@userid", "00000000-0000-0000-0000-000000000000");
                    insertCmd.Parameters.AddWithValue("@organization_code", (object?)prsc.organization_code ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@application_number", (object?)prsc.application_number ?? DBNull.Value);
                    insertCmd.ExecuteNonQuery();
                }

                // Update TodoDetailId 
                UpdateTodoDetailId(conn, newToDoCode, processDetailCode);
                return Ok(new
                {
                    Message = "prescription inserted successfully " +
                    " ✅ ToDo updated and new ToDo created",
                    diagnosisCode = prsc.diagnosisCode,
                    CompletedToDoCode = prsc.todocode,
                    CompletedEndDate = endDate,
                    CompletedElapsedHours = elapsedTimeHours,
                    NewToDoCode = newToDoCode
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
     //Helper method to fetch application_code 
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
        // helper method to update Todo Detail ID
        private void UpdateTodoDetailId(SqlConnection conn, Guid newToDoCode, Guid processDetailCode)
        {
            using SqlCommand cmd = new("proc_updateTodoDetailIdForPrescription", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@to_do_code", newToDoCode );
            cmd.Parameters.AddWithValue("@detail_code", processDetailCode);

            cmd.ExecuteNonQuery();
        }
    }
}