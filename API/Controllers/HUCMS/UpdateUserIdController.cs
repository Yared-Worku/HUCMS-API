using BPM.Entities.HU;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class UpdateUserIdController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UpdateUserIdController(IConfiguration config)
        {
            _config = config;
        }

        // POST api/UpdateUserId/UpdateUserId
        [HttpPut]
        public IActionResult UpdateUserId([FromBody] UpdateUserId model)
        {
            if (model == null || model.UserId == Guid.Empty || model.ToDoCode == Guid.Empty)
                return BadRequest("Invalid parameters");

            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                using SqlConnection conn = new SqlConnection(connStr);
                using SqlCommand cmd = new SqlCommand("proc_updateuserid", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // 🔹 Add GUID parameters
                cmd.Parameters.Add(new SqlParameter("@userid", SqlDbType.UniqueIdentifier) { Value = model.UserId });
                cmd.Parameters.Add(new SqlParameter("@to_do_code", SqlDbType.UniqueIdentifier) { Value = model.ToDoCode });

                conn.Open();
                cmd.ExecuteNonQuery();

                return Ok(new { message = "UserId updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
