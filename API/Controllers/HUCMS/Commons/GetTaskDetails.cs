using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetTaskDetails : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetTaskDetails(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetTaskDetail([FromQuery] Guid taskCode, [FromQuery] string applicationNumber)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var results = new List<TaskDetail>();

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                // 1️⃣ Get all application_detail_ids
                var applicationDetailIds = new List<Guid>();
                using (SqlCommand cmdGetIds = new("proc_GetApplicationDetailId", conn))
                {
                    cmdGetIds.CommandType = CommandType.StoredProcedure;
                    cmdGetIds.Parameters.AddWithValue("@TaskCode", taskCode);
                    cmdGetIds.Parameters.AddWithValue("@ApplicationNumber", applicationNumber);

                    using SqlDataReader reader = cmdGetIds.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader["application_detail_id"] != DBNull.Value)
                            applicationDetailIds.Add((Guid)reader["application_detail_id"]);
                    }
                }

                if (applicationDetailIds.Count == 0)
                    return NotFound(new { Message = "No Application Detail IDs found." });

                // 2️⃣ For each ID, fetch task details
                foreach (var id in applicationDetailIds)
                {
                    using (SqlCommand cmd = new("proc_GetTaskDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@application_detail_id", id);

                        using SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            results.Add(new TaskDetail
                            {
                                value = reader["value"] != DBNull.Value ? reader["value"].ToString() : null
                            });
                        }
                    }
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch task details.",
                    Details = ex.Message
                });
            }
        }

    }
}
