using HUCMS.Models.HUCMS.MedicalProcess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.MedicalProcess
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class LabRequestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public LabRequestController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public IActionResult CreateLabRequest([FromBody] LabRequest request)
        {
            if (request == null || request.UserId == Guid.Empty)
                return BadRequest("Invalid lab request data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid applicationCode = Guid.Empty;
                string applicationNumber = request.application_number; 
                Guid processDetailCode = Guid.Empty;
                Guid labCode = Guid.Empty;
                Guid created_by = request.UserId.Value;
 
                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new { Error = "Application not found for the given application number." });
                }

                if (request.detail_code.HasValue && request.detail_code != Guid.Empty)
                {
                    processDetailCode = request.detail_code.Value;
                }
                else
                {
                    processDetailCode = InsertApplicationProcessDetail(
                        conn,
                        applicationCode,
                        request.tasks_task_code.Value
                    );
                }

                bool isNew = request.detail_code == null || request.detail_code == Guid.Empty;
                
                // SMART FIX: Pass request.lab_Code to the helper
                labCode = InsertApplicationProcessLabTestData(conn, request.lab_test, created_by, processDetailCode, request.diagnosisCode, request.lab_Code);

                if (isNew)
                {
                    TodoInsert(conn, applicationNumber, processDetailCode, request.UserId, request.organization_code, request.tasks_task_code);
                }   

                return Ok(new
                {
                    Message = "✅ Lab test data processed successfully",
                    ProcessDetailCode = processDetailCode,
                    lab_Code = labCode
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error occurred", Details = ex.Message });
            }
        }

        [HttpPut]
        public IActionResult UpdateLabTaskData([FromBody] LabRequest request)
        {
            if (request == null || request.detail_code == Guid.Empty)
                return BadRequest("Invalid lab data for update");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                using SqlCommand cmd = new("proc_InsertLabRequest", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@diagnosis_Code", request.diagnosisCode);
                cmd.Parameters.AddWithValue("@lab_test", request.lab_test ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@detail_code", request.detail_code ?? (object)DBNull.Value);
                // Ensure ID is passed for the update logic in SP
                cmd.Parameters.AddWithValue("@lab_Code", request.lab_Code); 

                cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = "✅ lab data updated successfully",
                    request.lab_Code
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error occurred", Details = ex.Message });
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

        private Guid InsertApplicationProcessDetail(SqlConnection conn, Guid applicationCode, Guid tasks_task_code)
        {
            using SqlCommand cmd2 = new("proc_InsertApplicationProcessDetailLabtest", conn) { CommandType = CommandType.StoredProcedure };
            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);
            cmd2.Parameters.AddWithValue("@tasks_task_code", tasks_task_code);
            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier) { Direction = ParameterDirection.Output };
            cmd2.Parameters.Add(outputParam);
            cmd2.ExecuteNonQuery();
            return (Guid)outputParam.Value;
        }

        private Guid InsertApplicationProcessLabTestData(SqlConnection conn, string lab_test, Guid created_by, Guid? applicationProcessDetailsProcessDetailCode, Guid? diagnosis_Code, Guid? existingLabCode)
        {
            using SqlCommand cmd = new("proc_InsertLabRequest", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@lab_test", lab_test ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@diagnosis_Code", (object)diagnosis_Code ?? DBNull.Value); 
            cmd.Parameters.AddWithValue("@created_by", created_by);
            cmd.Parameters.AddWithValue("@detail_code", (object)applicationProcessDetailsProcessDetailCode ?? DBNull.Value);

            // SMART FIX: Change to InputOutput and assign the existing code
            SqlParameter outputParam = new("@lab_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.InputOutput,
                Value = (object)existingLabCode ?? DBNull.Value
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        private void TodoInsert(SqlConnection conn, string applicationNumber, Guid processDetailCode, Guid? UserId, Guid? organization_code, Guid? tasks_task_code)
        {
            using SqlCommand cmd = new("proc_TodoLabTest", conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@application_detail_id", processDetailCode);
            cmd.Parameters.AddWithValue("@organization_code", (object)organization_code ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@tasks_task_code", (object)tasks_task_code ?? DBNull.Value); //this will be handled manually in the sp
            cmd.ExecuteNonQuery();
        }
    }
}