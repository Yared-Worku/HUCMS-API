using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetMessageController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetMessageController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{todo}")]
        public IActionResult GetMessageByToDo(Guid todo)
        {
            if (todo == Guid.Empty)
            {
                return BadRequest(new { Message = "A valid toDoCode is required." });
            }

            string connStr = _config.GetConnectionString("HU_DB");
            GetMessage result = null;

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getMessageByToDoCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@ToDoCode", todo);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    result = new GetMessage
                    {
                        message = reader["message"] != DBNull.Value ? reader["message"].ToString() : null
                    };
                }

                if (result == null)
                {
                    return NotFound(new { Message = "No specific message found for this task." });
                }

                return Ok(result);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Database error while fetching message",
                    Details = ex.Message
                });
            }
        }
    }
}