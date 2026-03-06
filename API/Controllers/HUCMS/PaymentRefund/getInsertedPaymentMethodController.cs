using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getInsertedPaymentMethodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getInsertedPaymentMethodController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{userid}")] 
        public IActionResult getinsertedmethod([FromRoute] Guid userid) 
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var results = new List<getInsertedPaymentMethod>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("proc_getInsertedmethod", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@UserId", userid);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    results.Add(new getInsertedPaymentMethod
                    {
                        account_number = reader["AccNo"] != DBNull.Value ? reader["AccNo"].ToString() : null,
                        method_code = reader["Paymentmethod_Code"] != DBNull.Value ? (Guid)reader["Paymentmethod_Code"] : null,
                        Name = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : null,
                        status = reader["status"] != DBNull.Value ? (bool)reader["status"] : null
                    });
                }
                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
