using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UsersController(IConfiguration config)
        {
            _config = config;
        }

        // 🔹 Get All Users
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var users = new List<User>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_GetAllUsers", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserID = reader["UserID"] != DBNull.Value ? Guid.Parse(reader["UserID"].ToString()) : null,
                        FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : null,
                        LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : null,
                        Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : null,
                        depCode = reader["depCode"] != DBNull.Value ? Guid.Parse(reader["depCode"].ToString()) : null
                    });
                }

                return Ok(users);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Failed to fetch Users", Details = ex.Message });
            }
        }

        // 🔹 Assign Department to User
        [HttpPut("{userId}/AssignDepartment")]
        public IActionResult AssignDepartment(Guid userId, [FromBody] AssignDepartmentRequest request)
        {
            if (userId == Guid.Empty || string.IsNullOrEmpty(request.depCode))
                return BadRequest("Invalid request");

            string connStr = _config.GetConnectionString("HU_DB");

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_AssignUserDepartment", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@depCode", Guid.Parse(request.depCode));

            try
            {
                conn.Open();
                int affected = cmd.ExecuteNonQuery();
                return Ok(new { Message = "✅ Department assigned successfully", AffectedRows = affected });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error occurred", Details = ex.Message });
            }
        }
    }

    // 🔹 DTO for Assigning Department
    public class AssignDepartmentRequest
    {
        public string depCode { get; set; }
    }
}
