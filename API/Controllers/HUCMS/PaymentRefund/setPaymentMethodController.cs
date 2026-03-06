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
        public IActionResult setpaymentmethod([FromBody] List<setPaymentMethod> sprList) 
        {
            if (sprList == null || sprList.Count == 0)
            {
                return BadRequest("No payment methods provided.");
            }

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            using SqlTransaction transaction = conn.BeginTransaction();

            try
            {
                foreach (var spr in sprList)
                {
                    Insertpaymentmethod(conn, transaction, spr.account_number, spr.UserId, spr.method_code, spr.AllActiveMethods);
                }

                transaction.Commit();

                return Ok(new
                {
                    Message = "✅ Payment methods inserted successfully",
                });
            }
            catch (SqlException ex)
            {
                transaction.Rollback();
                return StatusCode(500, new
                {
                    Error = "❌ Database error occurred",
                    Details = ex.Message
                });
            }
        }

        private void Insertpaymentmethod(SqlConnection conn, SqlTransaction trans, string? account_number, Guid? User_Id, Guid? method_code, string? activeMethods)
        {
            using SqlCommand cmd = new("proc_setPaymentmethod", conn, trans)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@AccNo", (object)account_number ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object)User_Id ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Paymentmethod_Code", (object)method_code ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AllActiveMethods", activeMethods);
            cmd.ExecuteNonQuery();
        }

    }
}
