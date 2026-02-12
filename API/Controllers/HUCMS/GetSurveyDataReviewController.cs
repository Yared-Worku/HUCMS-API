using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using BPM.Entities.HU;

namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetSurveyDataReviewController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetSurveyDataReviewController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{processDetailCode}")]
        public IActionResult GetSurveyDataReview(string processDetailCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var results = new List<SurveyDataReview>();

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

                // Call stored procedure directly
                using (SqlCommand cmd = new("proc_getReviewData", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProcessDetailCode", processDetailCode);

                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        results.Add(new SurveyDataReview
                        {
                            Value = reader["value"] != DBNull.Value ? reader["value"].ToString() : null
                        });
                    }
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch survey data.",
                    Details = ex.Message
                });
            }
        }
    }
}
