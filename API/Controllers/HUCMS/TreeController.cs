using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HU_api.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class TreeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TreeController(IConfiguration config)
        {
            _config = config;
        }
        [HttpGet]
        public IActionResult Services()
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var services = new List<Tree>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("SELECT * FROM vw_Services", conn)
            {
                CommandType = CommandType.Text
            };

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    services.Add(new Tree
                    {
                        service_code = reader["service_code"] != DBNull.Value ? reader["service_code"].ToString() : null,
                        topic_code = reader["topic_code"] != DBNull.Value ? reader["topic_code"].ToString() : null,
                        RoleId = reader["RoleId"] != DBNull.Value ? reader["RoleId"].ToString() : null,
                        organization_code = reader["organization_code"] != DBNull.Value ? reader["organization_code"].ToString() : null,
                        task_code = reader["task_code"] != DBNull.Value ? reader["task_code"].ToString() : null,
                        meta_data_forms_form_code = reader["meta_data_forms_form_code"] != DBNull.Value ? reader["meta_data_forms_form_code"].ToString() : null,
                        Registration_code = reader["Registration_code"] as string,
                        description_en = reader["description_en"] as string,
                        description_local = reader["description_local"] as string,
                        description_am = reader["description_am"] as string,
                        description_or = reader["description_or"] as string,
                        description_tg = reader["description_tg"] as string,
                        //department_en = reader["department_en"] as string,
                        Departments = reader["departments"] as string,

                        is_active = reader["is_active"] != DBNull.Value && (bool)reader["is_active"],
                        is_login_required = reader["is_login_required"] != DBNull.Value && (bool)reader["is_login_required"],
                        is_duplicate_allowed = reader["is_duplicate_allowed"] != DBNull.Value && (bool)reader["is_duplicate_allowed"],
                        RequirementsTOApply_en = reader["RequirementsTOApply_en"] as string,
                        RequirementsTOApply_Local = reader["RequirementsTOApply_Local"] as string,
                        RequirementsTOApply_am = reader["RequirementsTOApply_am"] as string,
                        RequirementsTOApply_or = reader["RequirementsTOApply_or"] as string,
                        RequirementsTOApply_tg = reader["RequirementsTOApply_tg"] as string,

                    });
                }

                return Ok(services);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch services",
                    Details = ex.Message
                });
            }
        }

    }
}
