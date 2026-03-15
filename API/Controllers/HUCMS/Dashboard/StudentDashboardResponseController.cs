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
            string procedureName;

            if (roleID?.ToString().ToUpper() == "4ED1B191-AD58-4EAD-B269-02576B4DD8D0")
            {
                procedureName = "proc_GetStudentDashboardData";
            }
            else
            {
                procedureName = "proc_GetOtherRolesDashboardData"; 
            }
            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                using SqlCommand cmd = new(procedureName, conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserID", UserID);

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

                        if (statuses.Contains("PS"))
                        {
                            finalStatus = "PS";
                        }
                        else if (statuses.Contains("P"))
                        {
                            finalStatus = "P";
                        }
                        else if (statuses.Contains("S"))
                        {
                            finalStatus = "S";
                        }
                        else
                        {
                            // Fallback to the first status found (usually "C" or "O")
                            finalStatus = statuses.FirstOrDefault() ?? "";
                        }

                        return new { FinalStatus = finalStatus };
                    }).ToList();

                response.Stats.Completed = distinctApps.Count(x => x.FinalStatus == "C");
                response.Stats.Rejected = distinctApps.Count(x => x.FinalStatus == "PS");
                response.Stats.Completed = distinctApps.Count(x => x.FinalStatus == "C");
                response.Stats.Picked = distinctApps.Count(x => x.FinalStatus == "P");
                response.Stats.Suspended = distinctApps.Count(x => x.FinalStatus == "S");
                response.Stats.Open = distinctApps.Count(x => x.FinalStatus == "O");
                response.Stats.Rejected = distinctApps.Count(x => x.FinalStatus == "PS");
                return Ok(response);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "Database Error", Details = ex.Message });
            }
        }
    }
}