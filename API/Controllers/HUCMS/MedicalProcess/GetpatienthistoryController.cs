
using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class GetpatienthistoryController : ControllerBase
    {
        private readonly IConfiguration _config;

        public GetpatienthistoryController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetPatientHistory([FromQuery] Guid customerID)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var results = new List<Getpatienthistory>();

            using SqlConnection conn = new(connStr);
            try
            {
                conn.Open();

     
                    using (SqlCommand cmd = new("proc_GetPatientHistory", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UserId", customerID);

                        using SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            results.Add(new Getpatienthistory
                            {
                                detail_diagnosis = reader["detail_diagnosis"] != DBNull.Value ? reader["detail_diagnosis"].ToString() : null,
                                Doctor_FirstName = reader["Doctor_FirstName"] != DBNull.Value ? reader["Doctor_FirstName"].ToString() : null,
                                created_date = reader["created_date"] != DBNull.Value ? reader["created_date"].ToString() : null,
                                Doctor_LastName = reader["Doctor_LastName"] != DBNull.Value ? reader["Doctor_LastName"].ToString() : null,
                                LabTechnician_FirstName = reader["LabTechnician_FirstName"] != DBNull.Value ? reader["LabTechnician_FirstName"].ToString() : null,
                                LabTechnician_LastName = reader["LabTechnician_LastName"] != DBNull.Value ? reader["LabTechnician_LastName"].ToString() : null,
                                RX = reader["RX"] != DBNull.Value ? reader["RX"].ToString() : null,
                                appointment_reason = reader["appointment_reason"] != DBNull.Value ? reader["appointment_reason"].ToString() : null,
                                appointment_date = reader["appointment_date"] != DBNull.Value ? Convert.ToDateTime(reader["appointment_date"]).ToString("yyyy-MM-dd") : null,
                                reason_for_referal = reader["reason_for_referal"] != DBNull.Value ? reader["reason_for_referal"].ToString() : null,
                                lab_test = reader["lab_test"] != DBNull.Value ? reader["lab_test"].ToString() : null,
                                lab_result = reader["lab_result"] != DBNull.Value ? reader["lab_result"].ToString() : null
                            });
                        }
                    }

                return Ok(results);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Failed to fetch patient history.",
                    Details = ex.Message
                });
            }
        }
    }
}
