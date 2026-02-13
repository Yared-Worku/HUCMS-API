using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class DispenseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DispenseController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult Dispense([FromBody] Dispense dis)
        {
            if (dis == null || dis.UserId == Guid.Empty || dis.todocode == Guid.Empty)
                return BadRequest("Invalid prescription data.");

            string connStr = _config.GetConnectionString("HU_DB");
            try
            {
                DateTime startDate;
                DateTime endDate = DateTime.Now;
                decimal elapsedTimeHours;
                using SqlConnection conn = new(connStr);
                conn.Open();
                // Get start_date
                using (SqlCommand cmd = new("proc_getStartDate", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@todocode", dis.todocode);
                    cmd.Parameters.AddWithValue("@applicationNumber", dis.application_number);
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        return NotFound(new { Message = "Start date not found for given ToDoCode." });

                    startDate = Convert.ToDateTime(result);
                }
              
                // Calculate elapsed hours
                elapsedTimeHours = Convert.ToDecimal((endDate - startDate).TotalHours);
                using SqlCommand InsertCmd = new("proc_UpdatePrescription", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                InsertCmd.Parameters.AddWithValue("@drugist_Code", dis.UserId);
                InsertCmd.Parameters.AddWithValue("@detail_code", dis.detail_code);
                InsertCmd.Parameters.AddWithValue("@remark", dis.remark);
                InsertCmd.Parameters.AddWithValue("@quantity", dis.quantity);
                InsertCmd.ExecuteNonQuery();

                // Update old row
                using (SqlCommand updateCmd = new("proc_updateTodoListTask", conn))
                {
                    updateCmd.CommandType = CommandType.StoredProcedure;
                    updateCmd.Parameters.AddWithValue("@todocode", dis.todocode);
                    updateCmd.Parameters.AddWithValue("@end_date", endDate);
                    updateCmd.Parameters.AddWithValue("@elapsed_time_hours", elapsedTimeHours);
                    updateCmd.Parameters.AddWithValue("@application_detail_id", (object?)dis.detail_code ?? DBNull.Value);
                    updateCmd.ExecuteNonQuery();
                }
                return Ok(new
                {
                    Message = "prescription updated successfully " +
                    " ✅ ToDoList updated ",
                    CompletedToDoCode = dis.todocode,
                    CompletedEndDate = endDate,
                    CompletedElapsedHours = elapsedTimeHours,
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to update prescription data",
                    Details = ex.Message
                });
            }
        }
       
    }
}
