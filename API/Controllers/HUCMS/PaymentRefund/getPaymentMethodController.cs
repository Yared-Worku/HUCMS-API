using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getPaymentMethodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getPaymentMethodController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult getmwthod()
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<getPaymentMethod>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getMethod", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
        
            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new getPaymentMethod
                    {
                        name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                        method_code = reader["Paymentmethod_Code"] != DBNull.Value ? (Guid)reader["Paymentmethod_Code"] : null

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
