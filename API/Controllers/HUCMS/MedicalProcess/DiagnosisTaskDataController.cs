
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
                // Declare required variables
                Guid applicationCode = Guid.Empty;
                string applicationNumber = dtd.application_number; // Sent from UI
                Guid processDetailCode = Guid.Empty;
                Guid diagnosis_Code = Guid.Empty;
                Guid created_by = dtd.UserId.Value;


                // Fetch application_code via stored procedure
                applicationCode = GetApplicationCode(conn, applicationNumber);

                if (applicationCode == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        Error = "Application not found for the given application number."
                    });
                }

                // Continue insertion process
                //processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, dtd.tasks_task_code.Value);
                if (dtd.process_detail_code.HasValue && dtd.process_detail_code != Guid.Empty)
                {
                    processDetailCode = dtd.process_detail_code.Value;
                }
                else
                {
                    processDetailCode = InsertApplicationProcessDetail(
                        conn,
                        applicationCode,
                        dtd.tasks_task_code.Value
                    );
                }

                diagnosis_Code = InsertApplicationProcessDiagnosisData(conn, dtd.diagnosis, created_by, processDetailCode);

                // Update TodoDetailId using new helper method
                UpdateTodoDetailId(conn, applicationNumber, processDetailCode);

                return Ok(new
                {
                    Message = "✅ Diagnosis Task data inserted successfully",
                    ProcessDetailCode = processDetailCode,
                    diagnosis_Code
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
                //cmd.Parameters.AddWithValue("@updated_by", dtd.UserId ?? (object)DBNull.Value);

                cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = "✅ Diagnosis updated successfully",
                    dtd.diagnosis_Code
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


        //Helper method to fetch application_code using stored procedure
        private Guid GetApplicationCode(SqlConnection conn, string applicationNumber)
        {
            using SqlCommand cmd = new("proc_getApplicationCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);

            SqlParameter outputParam = new("@application_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return outputParam.Value != DBNull.Value ? (Guid)outputParam.Value : Guid.Empty;
        }

        private Guid InsertApplicationProcessDetail(SqlConnection conn, Guid applicationCode, Guid tasksTaskCode)
        {
            using SqlCommand cmd2 = new("proc_InsertApplicationProcessDetail", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);
            cmd2.Parameters.AddWithValue("@tasks_task_code", tasksTaskCode);

            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd2.Parameters.Add(outputParam);

            cmd2.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        private Guid InsertApplicationProcessDiagnosisData(SqlConnection conn, string diagnosis,Guid created_by, Guid? applicationProcessDetailsProcessDetailCode)
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

            SqlParameter outputParam = new("@diagnosis_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        // helper method to update Todo Detail ID
        private void UpdateTodoDetailId(SqlConnection conn, string applicationNumber, Guid processDetailCode)
        {
            using SqlCommand cmd = new("proc_updateTodoDetailId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@process_detail_code", processDetailCode);

            cmd.ExecuteNonQuery();
        }
    }
}
