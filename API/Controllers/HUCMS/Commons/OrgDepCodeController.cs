using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class OrgDepCodeController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrgDepCodeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetAllOrgDepCodes()
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var depList = new List<OrgDepCode>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_getorgDepcode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    depList.Add(new OrgDepCode
                    {
                        name_en = reader["name_en"] != DBNull.Value ? reader["name_en"].ToString() : null,
                        depCode = reader["depCode"] != DBNull.Value ? reader["depCode"].ToString() : null,
                        depName = reader["depName"] != DBNull.Value ? reader["depName"].ToString() : null,
                        orgCode = reader["orgCode"] != DBNull.Value ? reader["orgCode"].ToString() : null
                    });
                }

                return Ok(depList);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "Database Failure", Details = ex.Message });
            }
        }
    }
}