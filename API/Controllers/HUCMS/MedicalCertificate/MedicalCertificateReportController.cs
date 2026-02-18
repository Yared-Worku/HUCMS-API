using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using HUCMS.Models.HUCMS.MedicalCertificate;

namespace HUCMS.Controllers.HUCMS.MedicalCertificate
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class MedicalCertificateReportController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MedicalCertificateReportController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult GetCertificateReport()
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var reports = new List<MedicalCertificateReport>();

            using SqlConnection conn = new(connStr);

            try
            {
                using SqlCommand cmd = new("proc_getMedicalCertificateReport", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // ❌ Removed parameter
                // cmd.Parameters.AddWithValue("@Application_No", applicationNumber);

                conn.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reports.Add(new MedicalCertificateReport
                    {
                        Application_No = reader["Application_No"]?.ToString(),
                        Service_Name = reader["Service_Name"]?.ToString(),
                        Cust_Phone_No = reader["Cust_Phone_No"]?.ToString(),
                        Applicant_First_Name_EN = reader["Applicant_First_Name_EN"]?.ToString(),
                        Applicant_Middle_Name_En = reader["Applicant_Middle_Name_En"]?.ToString(),
                        Applicant_Last_Name_EN = reader["Applicant_Last_Name_EN"]?.ToString(),
                        ID_NO = reader["ID_NO"]?.ToString(),
                        Age = reader["Age"] != DBNull.Value ? Convert.ToInt32(reader["Age"]) : null,
                        Depname = reader["depname"]?.ToString(),
                        Gender = reader["Gender"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Cust_TIN_No = reader["Cust_TIN_No"]?.ToString(),
                        RX = reader["RX"]?.ToString(),
                        Patient_Condition = reader["patient_condition"]?.ToString(),
                        Health_Profetional_Recomendation = reader["health_profetional_recomendation"]?.ToString(),
                        Detail_Diagnosis = reader["detail_diagnosis"]?.ToString(),
                        Date_Of_Issuance = reader["date_of_issuance"]?.ToString(),
                        Fname_Doctor = reader["Fname_Doctor"]?.ToString(),
                        Lname_Doctor = reader["Lname_Doctor"]?.ToString(),
                        Attended_Date = reader["attended_date"]?.ToString(),
                        Cust_Photo = reader["Cust_Photo"]?.ToString()
                    });
                }

                if (reports.Count == 0)
                    return NotFound(new { Message = "No medical certificate records found." });

                return Ok(reports);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ Database error while fetching medical report.",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = "❌ An unexpected error occurred.",
                    Details = ex.Message
                });
            }
        }

    }
}