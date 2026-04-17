using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class StudentDashboardResponseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentDashboardResponseController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{UserID}")]
        public IActionResult GetDashboardData([FromRoute] Guid UserID, [FromQuery] Guid? roleID)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var response = new StudentDashboardResponse();

            bool isStudentRole = roleID?.ToString().ToUpper() == "C2D34305-61DB-4540-B368-26E4F9564C62";
            string procedureName = isStudentRole ? "proc_GetStudentDashboardData" : "proc_GetOtherRolesDashboardData";

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                using SqlCommand cmd = new(procedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", UserID);
                cmd.Parameters.AddWithValue("@roleID", (object)roleID ?? DBNull.Value);

                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var row = new ApplicationDetail
                    {
                        Application_No = reader["Application_No"]?.ToString(),
                        Service_Name = reader["Service_Name"]?.ToString(),
                        Application_Date = reader["Application_Date"]?.ToString(),
                        status = reader["status"]?.ToString(),
                        RoleName = reader["RoleName"]?.ToString(),
                        UserID = reader["UserID"] != DBNull.Value ? (Guid)reader["UserID"] : null,
                        RoleID = reader["RoleID"] != DBNull.Value ? (Guid)reader["RoleID"] : null
                    };
                    response.Details.Add(row);
                }

                var distinctApps = response.Details
                    .GroupBy(x => x.Application_No)
                    .Select(g => {
                        var statuses = g.Select(x => x.status).ToList();
                        string finalStatus;
                        bool isPickedByCurrentUser = false; 

                        if (statuses.Contains("PS"))
                        {
                            finalStatus = "PS";
                        }
                        else if (statuses.Contains("P"))
                        {
                            finalStatus = "P";
                            isPickedByCurrentUser = g.Any(x => x.status == "P" && x.UserID == UserID);
                        }
                        else if (statuses.Contains("S"))
                        {
                            finalStatus = "S";
                        }
                        else if (statuses.Contains("O"))
                        {
                            finalStatus = "O";
                        }
                        else
                        {
                            // Fallback for "C" (Completed) or any other status
                            finalStatus = statuses.FirstOrDefault() ?? "";
                        }

                        // Pass the flag out of the projection
                        return new { Application_No = g.Key, FinalStatus = finalStatus, IsPickedByCurrentUser = isPickedByCurrentUser };
                    }).ToList();

                response.Stats.Completed = distinctApps.Count(x => x.FinalStatus == "C");
                response.Stats.Rejected = distinctApps.Count(x => x.FinalStatus == "PS");

                // Implement conditional counting for "Picked"
                if (isStudentRole)
                {
                    response.Stats.Picked = distinctApps.Count(x => x.FinalStatus == "P");
                }
                else
                {
                    response.Stats.Picked = distinctApps.Count(x => x.FinalStatus == "P" && x.IsPickedByCurrentUser);
                }

                response.Stats.Suspended = distinctApps.Count(x => x.FinalStatus == "S");
                response.Stats.Open = distinctApps.Count(x => x.FinalStatus == "O");

                response.Details = distinctApps.Select(app =>
                    response.Details.FirstOrDefault(d => d.Application_No == app.Application_No && d.status == app.FinalStatus)
                ).Where(d => d != null).ToList();

                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "Database Error", Details = ex.Message });
            }
        }
    }
}