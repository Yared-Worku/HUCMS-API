using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class UserDeptAssignmentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserDeptAssignmentController(IConfiguration config)
        {
            _config = config;
        }

        // The route here maps to your Axios call. 
        // If your Axios call is just '/AssignUserDept', you might need to adjust your frontend to hit '/api/HU/Assignment/AssignUserDept'
        [HttpPost]
        public IActionResult AssignUserDept([FromBody] UserDeptAssignment payload)
        {
            // Validate the incoming data
            if (payload == null || payload.Userid == Guid.Empty || payload.DepCode == Guid.Empty)
            {
                return BadRequest(new { Error = "Invalid payload. User ID and Department Code are required." });
            }

            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                using SqlConnection conn = new(connStr);
                using SqlCommand cmd = new("sp_AssignCustomerDepartment", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                // Add parameters matching the Stored Procedure
                cmd.Parameters.AddWithValue("@UserID", payload.Userid);
                cmd.Parameters.AddWithValue("@DepCode", payload.DepCode);

                conn.Open();
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return Ok(new { message = "Success" });
                }
                else
                {
                    // If the Procedure doesn't find the IDs, it might return 0 rows
                    // Some developers accidentally return NotFound() here
                    return NotFound("Assignment failed in database");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}