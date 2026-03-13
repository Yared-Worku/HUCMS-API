using HUCMS.Models.HUCMS.Commons;
using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using System.Data;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getActivePaymentMethodController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getActivePaymentMethodController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{AppNo}")]
        public IActionResult getActivepaymentmthod([FromRoute] string AppNo)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            var results = new List<getActivePaymentMethod>();

            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid UserId = Guid.Empty;
                UserId = GetApplicantUserId(conn, AppNo);

                if (UserId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "userid not found for the given application number."
                    });
                }
                using SqlCommand cmd = new("proc_PaymentMethodForFinance", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", UserId);
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
        private Guid GetApplicantUserId(SqlConnection conn, string applicationNumber)
        {
            using SqlCommand cmd = new("proc_getApplicantUserId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);

            SqlParameter outputParam = new("@UserId", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return outputParam.Value != DBNull.Value ? (Guid)outputParam.Value : Guid.Empty;
        }

    }
}
