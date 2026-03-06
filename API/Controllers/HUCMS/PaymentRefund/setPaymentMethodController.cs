using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class setPaymentMethodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public setPaymentMethodController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult setpaymentmethod([FromBody] setPaymentMethod spr)
        {
         
            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                    Insertpaymentmethod(conn, spr.account_number, spr.User_Id, spr.method_code);
                
             
                return Ok(new
                {
                    Message = "✅ payment method inserted successfully",
         
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Database error occurred",
                    Details = ex.Message
                });
            }
        }
        private void  Insertpaymentmethod(SqlConnection conn, int? account_number, Guid? User_Id, Guid? method_code)
        {
            using SqlCommand cmd = new("proc_setPaymentmethod", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@AccNo", account_number.Value);
            cmd.Parameters.AddWithValue("@UserId", User_Id);
            cmd.Parameters.AddWithValue("@Paymentmethod_Code", method_code.Value);
            cmd.ExecuteNonQuery();
        }

    }
}
