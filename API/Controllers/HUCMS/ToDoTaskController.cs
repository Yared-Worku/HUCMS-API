using BPM.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class ToDoTaskController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ToDoTaskController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult CompleteAndCreateNext([FromBody] ToDoTask todo)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;
                Guid goto_task;
                using SqlConnection conn = new(connStr);
                conn.Open();

                // Get start_date
                using (SqlCommand cmd = new("proc_getStartDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@todocode", todo.todocode);
                    cmd.Parameters.AddWithValue("@applicationNumber", todo.application_number);
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(result);
                }
                //Get gototask
                using (SqlCommand cmd = new("proc_getGoToTask", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@task_rules_code", todo.task_rules_code);

                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return NotFound(new { Message = "goto task not found." });

                    goto_task = (Guid)result;
                }

                // Calculate elapsed hours
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);

                // Update old row
                using (SqlCommand updateCmd = new("proc_updateTodoListTask", conn))
                {
                    updateCmd.CommandType = CommandType.StoredProcedure;
                    updateCmd.Parameters.AddWithValue("@todocode", todo.todocode);
                    updateCmd.Parameters.AddWithValue("@end_date", endDate);
                    updateCmd.Parameters.AddWithValue("@elapsed_time_hours", elapsedTimeHours);
                    updateCmd.Parameters.AddWithValue("@application_detail_id", (object?)todo.ProcessDetailCode ?? DBNull.Value);
                    //updateCmd.Parameters.AddWithValue("@tasks_task_code", (object?)todo.tasks_task_code ?? DBNull.Value);

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
                    insertCmd.Parameters.AddWithValue("@jumpfrom", (object?)todo.todocode ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@tasks_task_code", goto_task);
                    insertCmd.Parameters.AddWithValue("@userid", "00000000-0000-0000-0000-000000000000");
                    insertCmd.Parameters.AddWithValue("@organization_code", (object?)todo.organization_code ?? DBNull.Value);
                    insertCmd.Parameters.AddWithValue("@application_number", (object?)todo.application_number ?? DBNull.Value);
                    insertCmd.ExecuteNonQuery();
                }

                return Ok(new
                {
                    Message = "✅ ToDo updated and new ToDo created",
                    CompletedToDoCode = todo.todocode,
                    CompletedEndDate = endDate,
                    CompletedElapsedHours = elapsedTimeHours,
                    NewToDoCode = newToDoCode
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "Database error occurred", Details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "Unexpected error occurred", Details = ex.Message });
            }
        }
    }
}
