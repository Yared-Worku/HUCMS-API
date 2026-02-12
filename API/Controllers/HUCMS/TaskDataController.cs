using BPM.Models.HU;
using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class TaskDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TaskDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult InsertTaskData([FromBody] TaskData td)
        {
            if (td == null || td.services_service_code == Guid.Empty)
                return BadRequest("Invalid task data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                // Declare required variables
                Guid applicationCode = Guid.Empty;
                string applicationNumber = td.application_number; // Sent from UI
                Guid processDetailCode = Guid.Empty;
                Guid processDataCode = Guid.Empty;

                // Fetch application_code via stored procedure
                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "Application not found for the given application number."
                    });
                }

                // Continue insertion process
                processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, td.tasks_task_code.Value);
                processDataCode = InsertApplicationProcessData(conn, td.value, processDetailCode);

                // Update TodoDetailId using new helper method
                UpdateTodoDetailId(conn, applicationNumber, processDetailCode);

                return Ok(new
                {
                    Message = "✅ Task data inserted successfully",
                    ProcessDetailCode = processDetailCode,
                    ProcessDataCode = processDataCode
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

        private Guid InsertApplicationProcessData(SqlConnection conn, string value, Guid? applicationProcessDetailsProcessDetailCode)
        {
            using SqlCommand cmd = new("proc_InsertApplicationprocessdata", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@value", value ?? (object)DBNull.Value);

            if (applicationProcessDetailsProcessDetailCode.HasValue)
                cmd.Parameters.AddWithValue("@application_process_details_process_detail_code", applicationProcessDetailsProcessDetailCode.Value);
            else
                cmd.Parameters.AddWithValue("@application_process_details_process_detail_code", DBNull.Value);

            SqlParameter outputParam = new("@process_data_code", SqlDbType.UniqueIdentifier)
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
