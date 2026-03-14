using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getProcessedDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getProcessedDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{ProcessDetailCode}")]
        public IActionResult getActivepaymentmthod([FromRoute] Guid ProcessDetailCode)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<getActivePaymentMethod>();

            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
 
                using SqlCommand cmd = new("proc_ProcessedData", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@ProcessDetailCode", ProcessDetailCode);
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {

                    results.Add(new getActivePaymentMethod
                    {
                        name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                        method_code = reader["Paymentmethod_Code"] != DBNull.Value ? (Guid)reader["Paymentmethod_Code"] : null,
                        AccNo = reader["AccNo"] != DBNull.Value ? reader["AccNo"].ToString() : null

                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found." });
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch data",
                    Details = ex.Message
                });
            }
        }
    }
}
