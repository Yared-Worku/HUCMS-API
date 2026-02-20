using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using Action = HUCMS.Models.HUCMS.Commons.Action;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetActionsByTaskCodeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetActionsByTaskCodeController(IConfiguration config)
        {
            _config = config;
        }

        // Accept taskCode as query parameter
        [HttpGet]
        public IActionResult GetActions([FromQuery] Guid taskCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<Action>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetActions", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@TaskCode", taskCode);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new Action
                    {
                        task_rules_code = reader["task_rules_code"] != DBNull.Value ? Guid.Parse(reader["task_rules_code"].ToString()) : null,
                        decision_rule_en = reader["decision_rule_en"] != DBNull.Value ? reader["decision_rule_en"].ToString() : null
                    });
                }

                if (results.Count == 0)
                    return NotFound(new { Message = "No record found for this task code." });

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch actions",
                    Details = ex.Message
                });
            }
        }
    }
}
