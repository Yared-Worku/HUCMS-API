using BPM.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BPM.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetLicense : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetLicense(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{AppNo}")]
        public IActionResult GetServiceName(string AppNo)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<AppNo>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetLicense", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@AppNo", AppNo);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new AppNo
                    {
                        ApplicationNumber = reader["Application_No"] != DBNull.Value ? reader["Application_No"].ToString() : null,
                        ServiceDescription_EN = reader["Service_Name"] != DBNull.Value ? reader["Service_Name"].ToString() : null,
                        Applicant_First_Name_EN = reader["Applicant_First_Name_EN"] != DBNull.Value ? reader["Applicant_First_Name_EN"].ToString() : null,
                        Applicant_Last_Name_EN = reader["Applicant_Last_Name_EN"] != DBNull.Value ? reader["Applicant_Last_Name_EN"].ToString() : null,
                        Applicant_Middle_Name_En = reader["Applicant_Middle_Name_En"] != DBNull.Value ? reader["Applicant_Middle_Name_En"].ToString() : null,
                        ID_NO = reader["ID_NO"] != DBNull.Value ? reader["ID_NO"].ToString() : null,
                        Age = reader["Age"] != DBNull.Value ? reader["Age"].ToString() : null,
                        depname = reader["depname"] != DBNull.Value ? reader["depname"].ToString() : null,
                        Gender = reader["Gender"] != DBNull.Value ? reader["Gender"].ToString() : null,
                        Cust_ID = reader["Cust_ID"] != DBNull.Value ? reader["Cust_ID"].ToString() : null,
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this Application Number." });
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch service name",
                    Details = ex.Message
                });
            }
        }
    }
}
