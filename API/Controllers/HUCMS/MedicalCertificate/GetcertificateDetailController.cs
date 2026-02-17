using HUCMS.Models.HUCMS.MedicalCertificate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalCertificate
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetcertificateDetailController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetcertificateDetailController(IConfiguration config)
        {
            _config = config;
        }

        // Added required parameters
        [HttpGet]
        public IActionResult Getcertificate(
            [FromQuery] string applicationNumber)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var results = new List<GetcertificateDetail>();

            using SqlConnection conn = new(connStr);

            try
            {
                conn.Open();

                // 1️⃣ Get all application_detail_ids
                var applicationDetailIds = new List<Guid>();

                using (SqlCommand cmdGetIds = new("proc_GetApplicationDetailIdforcertificate", conn))
                {
                    cmdGetIds.CommandType = CommandType.StoredProcedure;
   
                    cmdGetIds.Parameters.AddWithValue("@ApplicationNumber", applicationNumber);

                    using SqlDataReader reader = cmdGetIds.ExecuteReader();
                    while (reader.Read())
                    {
                        if (reader["application_detail_id"] != DBNull.Value)
                            applicationDetailIds.Add((Guid)reader["application_detail_id"]);
                    }
                }

                if (applicationDetailIds.Count == 0)
                    return NotFound(new { Message = "No Application Detail IDs found." });

                // 2️⃣ For each ID fetch certificate details
                foreach (var id in applicationDetailIds)
                {
                    using (SqlCommand cmd = new("proc_getcetrificateDetail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@application_detail_id", id);

                        using SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            results.Add(new GetcertificateDetail
                            {
                                treatment = reader["RX"] != DBNull.Value ? reader["RX"].ToString() : null,
                                diagnosis = reader["detail_diagnosis"] != DBNull.Value ? reader["detail_diagnosis"].ToString() : null,
                                attendedondate = reader["created_date"] != DBNull.Value? reader["created_date"].ToString(): null,
                                detail_code = id
                            });
                        }
                    }
                }

                if (results.Count == 0)
                    return NotFound(new { Message = "No certificate records found." });

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch certificate details.",
                    Details = ex.Message
                });
            }
        }
    }
}
