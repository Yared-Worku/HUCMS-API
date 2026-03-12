using HUCMS.Models.HUCMS.PaymentRefund;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.PaymentRefund
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class getReviewCHController : ControllerBase
    {
        private readonly IConfiguration _config;

        public getReviewCHController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("{todocode}")]
        public IActionResult GetReviewCH([FromRoute] Guid todocode, [FromQuery] Guid? pr_Codefromquery = null)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                using SqlConnection conn = new(connStr);
                conn.Open();

                List<getReviewCH> results = new List<getReviewCH>();

                if (pr_Codefromquery.HasValue && pr_Codefromquery.Value != Guid.Empty)
                {
                    results = FetchReviewData(conn, pr_Codefromquery.Value);
                }

                if (results.Count == 0)
                {
                    Guid fallbackCode = GetPr_Code(conn, todocode);
                    if (fallbackCode != Guid.Empty)
                    {
                        results = FetchReviewData(conn, fallbackCode);
                    }
                }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "Database Error", Details = ex.Message });
            }
        }

        private List<getReviewCH> FetchReviewData(SqlConnection conn, Guid pr_Code)
        {
            var list = new List<getReviewCH>();
            using SqlCommand cmd = new("proc_getReviewCH", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@pr_Code", pr_Code);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new getReviewCH
                {
                    rx = reader["rx"]?.ToString(),
                    quantity = reader["quantity"]?.ToString(),
                    remark = reader["remark"]?.ToString(),
                    drugistFname = reader["DrugistFname"]?.ToString(),
                    drugistLname = reader["DrugistLname"]?.ToString(),
                    doctorFname = reader["DoctorFname"]?.ToString(),
                    doctorLname = reader["DoctorLname"]?.ToString(),
                    file = reader["uploaded_file"]?.ToString()
                });
            }
            return list;
        }

        private Guid GetPr_Code(SqlConnection conn, Guid todocode)
        {
            using SqlCommand cmd = new("proc_GetPrCodeCh", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@todo_code", todocode);

            var result = cmd.ExecuteScalar();

            return result != DBNull.Value && result != null ? (Guid)result : Guid.Empty;
        }
    }
}
