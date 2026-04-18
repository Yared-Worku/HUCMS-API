using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class DiagnosisTaskDataController : ControllerBase
    {
        private readonly IConfiguration _config;

        public DiagnosisTaskDataController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult InsertTaskData([FromBody] DiagnosisTaskData dtd)
        {
            if (dtd == null || dtd.services_service_code == Guid.Empty)
                return BadRequest("Invalid task data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid applicationCode = Guid.Empty;
                string applicationNumber = dtd.application_number; 
                Guid processDetailCode = Guid.Empty;
                Guid diagnosis_Code = Guid.Empty;
                Guid created_by = dtd.UserId.Value;

                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new { Error = "Application not found." });
                }

                if (dtd.process_detail_code.HasValue && dtd.process_detail_code != Guid.Empty)
                {
                    processDetailCode = dtd.process_detail_code.Value;
                }
                else
                {
                    processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, dtd.tasks_task_code.Value);
                }

                // FIXED: Pass dtd.diagnosis_Code here to allow the SP to see the existing ID
                diagnosis_Code = InsertApplicationProcessDiagnosisData(conn, dtd.diagnosis, created_by, processDetailCode, dtd.diagnosis_Code);

                UpdateTodoDetailId(conn, applicationNumber, processDetailCode);

                return Ok(new
                {
                    Message = "✅ Diagnosis Task data processed successfully",
                    ProcessDetailCode = processDetailCode,
                    diagnosis_Code
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error", Details = ex.Message });
            }
        }

        [HttpPut]
        public IActionResult UpdateDiagnosisTaskData([FromBody] DiagnosisTaskData dtd)
        {
            if (dtd == null || dtd.diagnosis_Code == Guid.Empty)
                return BadRequest("Invalid diagnosis data for update");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                using SqlCommand cmd = new("proc_InsertApplicationProcessDiagnosisdata", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@diagnosis_Code", dtd.diagnosis_Code);
                cmd.Parameters.AddWithValue("@diagnosis", dtd.diagnosis ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = "✅ Diagnosis updated successfully",
                    dtd.diagnosis_Code
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error", Details = ex.Message });
            }
        }

        private Guid GetApplicationCode(SqlConnection conn, string applicationNumber)
        {
            using SqlCommand cmd = new("proc_getApplicationCode", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            SqlParameter outputParam = new("@application_code", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(outputParam);
            cmd.ExecuteNonQuery();
            return outputParam.Value != DBNull.Value ? (Guid)outputParam.Value : Guid.Empty;
        }

        private Guid InsertApplicationProcessDetail(SqlConnection conn, Guid applicationCode, Guid tasksTaskCode)
        {
            using SqlCommand cmd2 = new("proc_InsertApplicationProcessDetail", conn) { CommandType = CommandType.StoredProcedure };
            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);
            cmd2.Parameters.AddWithValue("@tasks_task_code", tasksTaskCode);
            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
            cmd2.Parameters.Add(outputParam);
            cmd2.ExecuteNonQuery();
            return (Guid)outputParam.Value;
        }

        // MODIFIED: Added existingDiagnosisCode parameter and changed direction to InputOutput
        private Guid InsertApplicationProcessDiagnosisData(SqlConnection conn, string diagnosis, Guid created_by, Guid? applicationProcessDetailsProcessDetailCode, Guid? existingDiagnosisCode)
        {
            using SqlCommand cmd = new("proc_InsertApplicationProcessDiagnosisdata", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@diagnosis", diagnosis ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@created_by", created_by);

            if (applicationProcessDetailsProcessDetailCode.HasValue)
                cmd.Parameters.AddWithValue("@application_process_details_code", applicationProcessDetailsProcessDetailCode.Value);
            else
                cmd.Parameters.AddWithValue("@application_process_details_code", DBNull.Value);

            // FIXED: Set Direction to InputOutput and assign the existing code value
            SqlParameter outputParam = new("@diagnosis_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.InputOutput,
                Value = (object)existingDiagnosisCode ?? DBNull.Value
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        private void UpdateTodoDetailId(SqlConnection conn, string applicationNumber, Guid processDetailCode)
        {
            using SqlCommand cmd = new("proc_updateTodoDetailId", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@process_detail_code", processDetailCode);
            cmd.ExecuteNonQuery();
        }
    }
}