using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IConfiguration _config;
        public TaskController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetTasks([FromQuery] Guid userId)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                using SqlConnection conn = new(connStr);
                conn.Open();

                Guid userRoleId = Guid.Empty;
                Guid orgCode = Guid.Empty;

                // 🔹 1. Get RoleId
                using (SqlCommand roleCmd = new("sp_GetUserRole", conn))
                {
                    roleCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    roleCmd.Parameters.AddWithValue("@userId", userId);

                    object? roleResult = roleCmd.ExecuteScalar();
                    if (roleResult != null && roleResult != DBNull.Value)
                    {
                        userRoleId = Guid.Parse(roleResult.ToString()!);
                    }
                }

                // 🔹 2. Get Organization Code
                using (SqlCommand orgcodeCmd = new("sp_GetOrgCode", conn))
                {
                    orgcodeCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    orgcodeCmd.Parameters.AddWithValue("@userId", userId);

                    object? orgCodeResult = orgcodeCmd.ExecuteScalar();
                    if (orgCodeResult != null && orgCodeResult != DBNull.Value)
                    {
                        orgCode = Guid.Parse(orgCodeResult.ToString()!);
                    }
                }

                // 🔹 3. Fetch tasks 
                using (SqlCommand cmd = new("sp_GetTasks", conn))
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@RoleId", userRoleId);
                    cmd.Parameters.AddWithValue("@organization_code", orgCode);

                    using SqlDataReader reader = cmd.ExecuteReader();
                    var results = new List<object>();

                    while (reader.Read())
                    {
                        results.Add(new
                        {
                            userId = reader["userId"] != DBNull.Value ? reader["userId"].ToString() : null,
                            tasks_task_code = reader["tasks_task_code"] != DBNull.Value ? Guid.Parse(reader["tasks_task_code"].ToString()) : Guid.Empty,
                            description_en = reader["description_en"]?.ToString(),
                            service_description_en = reader["service_description_en"]?.ToString(),
                            description_local = reader["description_local"]?.ToString(),
                            meta_data_forms_form_code = reader["meta_data_forms_form_code"] != DBNull.Value ? Guid.Parse(reader["meta_data_forms_form_code"].ToString()) : Guid.Empty,
                            RoleId = reader["RoleId"] != DBNull.Value ? Guid.Parse(reader["RoleId"].ToString()) : Guid.Empty,
                            services_service_code = reader["services_service_code"] != DBNull.Value ? Guid.Parse(reader["services_service_code"].ToString()) : Guid.Empty,
                            status = reader["status"]?.ToString(),
                            organization_code = reader["organization_code"] != DBNull.Value ? Guid.Parse(reader["organization_code"].ToString()) : Guid.Empty,
                            to_do_code = reader["to_do_code"] != DBNull.Value ? Guid.Parse(reader["to_do_code"].ToString()) : Guid.Empty,
                            application_detail_id = reader["application_detail_id"] != DBNull.Value ? Guid.Parse(reader["application_detail_id"].ToString()) : Guid.Empty,
                            application_number = reader["application_number"]?.ToString(),
                            start_date = reader["start_date"] != DBNull.Value ? Convert.ToDateTime(reader["start_date"]) : (DateTime?)null
                        });
                    }

                    return Ok(results);
                }
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
    }
}
