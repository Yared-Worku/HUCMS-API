using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetPaymentRefundController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetPaymentRefundController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{process_detail_code}")]
        public IActionResult getpaymetRefund(Guid process_detail_code)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<GetPaymentRefund>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_GetPaymentRefund", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@process_detail_code", process_detail_code);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new GetPaymentRefund
                    {
                        amount_inWord = reader["amount_in_word"] != DBNull.Value ? reader["amount_in_word"].ToString() : null,
                        pr_Code = reader["pr_Code"] != DBNull.Value ? (Guid)reader["pr_Code"] : null,
                        amount_inDigit = reader["amount_in_number"] != DBNull.Value ? Convert.ToSingle(reader["amount_in_number"]) : null
                    });
                }

                if (results.Count == 0)
                {
                    return NotFound(new { Message = "No record found for this detailcode." });
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
