using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class AddTopicController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AddTopicController(IConfiguration config)
        {
            _config = config;
        }

        //[HttpPost]
        //public IActionResult RegisterTaskType([FromBody] AddTopic model)
        //{
        //    if (model == null)
        //        return BadRequest("Invalid input.");

        //    string connStr = _config.GetConnectionString("HU_DB");

        //    using SqlConnection conn = new(connStr);
        //    using SqlCommand cmd = new("sp_RegisterTopic", conn)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };

        //    // Map parameters
        //    cmd.Parameters.AddWithValue("@description_en", model.description_en ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@description_local", model.description_local ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@description_am", model.description_am ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@description_or", model.description_or ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@description_tg", model.description_tg ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@is_synched", model.is_synched ? 1 : 0);
        //    cmd.Parameters.AddWithValue("@is_active", model.is_active ? 1 : 0);
        //    //cmd.Parameters.AddWithValue("@date_created", model.date_created ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@created_by", model.created_by ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Topic_link", model.Topic_link ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Topic_help_Link", model.Topic_help_Link ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@Parent_Topic_Id", model.Parent_Topic_Id ?? (object)DBNull.Value);
        //    cmd.Parameters.AddWithValue("@is_published", model.is_published ? 1 : 0);

        //    try
        //    {
        //        conn.Open();
        //        int affected = cmd.ExecuteNonQuery();
        //        return Ok(new
        //        {
        //            Message = "✅ topic registered successfully",
        //            AffectedRows = affected
        //        });
        //    }
        //    catch (SqlException ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            Error = "❌ Database error occurred",
        //            Details = ex.Message
        //        });
        //    }
        //}
        [HttpGet]
        public IActionResult GetAllTops()
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var Topics = new List<AddTopic>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("GetAllpublishedTopics", conn);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Topics.Add(NewMethod(reader));
                }

                return Ok(Topics);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch Topics",
                    Details = ex.Message
                });
            }

            static AddTopic NewMethod(SqlDataReader reader)
            {
                return new AddTopic
                {
                    Topic_code = reader["Topic_code"] != DBNull.Value ? reader["Topic_code"].ToString() : null,
                    description_or = reader["description_or"] != DBNull.Value ? reader["description_or"].ToString() : null,
                    description_en = reader["description_en"] as string,
                    description_local = reader["description_local"] as string,
                    description_am = reader["description_am"] as string,
                    description_tg = reader["description_tg"] as string,
                    is_active = reader["is_active"] != DBNull.Value && (bool)reader["is_active"],
                    is_synched = reader["is_synched"] != DBNull.Value && (bool)reader["is_synched"],
                    is_published = reader["is_published"] != DBNull.Value && (bool)reader["is_published"],
                    date_created = reader["date_created"]?.ToString(),
                    FirstName = reader["FirstName"]?.ToString(),
                    LastName = reader["LastName"]?.ToString(),
                    Topic_link = reader["Topic_link"]?.ToString(),
                    Topic_help_Link = reader["Topic_help_Link"]?.ToString(),
                    Parent_Topic_Id = reader["Parent_Topic_Id"]?.ToString(),
                 Parent_Description = reader["Parent_Description"]?.ToString(),
                };
            }
        }
        //[HttpPut("{Topic_code}")]
        //public IActionResult UpdateTopic(Guid Topic_code, [FromBody] AddTopic UpdateTopic)
        //{
        //    if (UpdateTopic == null || Topic_code == Guid.Empty)
        //        return BadRequest("Invalid topic data");

        //    try
        //    {
        //        string connStr = _config.GetConnectionString("HU_DB");

        //        using SqlConnection conn = new(connStr);
        //        using SqlCommand cmd = new("sp_RegisterTopic", conn)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };
        //        cmd.Parameters.AddWithValue("@Topic_code", Topic_code);
        //        cmd.Parameters.AddWithValue("@description_or", UpdateTopic.description_or ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@description_en", UpdateTopic.description_en ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@description_local", UpdateTopic.description_local ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@description_am", UpdateTopic.description_am ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@description_tg", UpdateTopic.description_tg ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@is_active", UpdateTopic.is_active);
        //        cmd.Parameters.AddWithValue("@is_synched", UpdateTopic.is_synched);
        //        cmd.Parameters.AddWithValue("@is_published", UpdateTopic.is_published);
        //        cmd.Parameters.AddWithValue("@Topic_link", UpdateTopic.Topic_link ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@Topic_help_Link", UpdateTopic.Topic_help_Link ?? (object)DBNull.Value);
        //        cmd.Parameters.AddWithValue("@Parent_Topic_Id", UpdateTopic.Parent_Topic_Id ?? (object)DBNull.Value);


        //        conn.Open();
        //        int rows = cmd.ExecuteNonQuery();

        //        return Ok(new
        //        {
        //            message = "✅ Topic updated successfully",
        //            AffectedRows = rows
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Error = "❌ Internal server error", Details = ex.Message });
        //    }
        //}
    }
}
