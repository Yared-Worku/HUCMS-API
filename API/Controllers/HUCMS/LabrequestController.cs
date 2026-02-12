using Azure.Core;
using BPM.Entities.HU;
using HUCMS.Models.HUCMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS
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
                // Declare required variables
                Guid applicationCode = Guid.Empty;
                string applicationNumber = request.application_number; // Sent from UI
                Guid processDetailCode = Guid.Empty;
                Guid labCode = Guid.Empty;
                Guid created_by = request.UserId.Value;
 
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
                //processDetailCode = InsertApplicationProcessDetail(conn, applicationCode);
                if (request.detail_code.HasValue && request.detail_code != Guid.Empty)
                {
                    processDetailCode = request.detail_code.Value;
                }
                else
                {
                    processDetailCode = InsertApplicationProcessDetail(
                        conn,
                        applicationCode
                    );
                }
                labCode = InsertApplicationProcessLabTestData(conn, request.lab_test, created_by, processDetailCode, request.diagnosisCode);

                //  Todoinsert using new helper method
                TodoInsert(conn, applicationNumber, processDetailCode, request.UserId, request.organization_code, request.tasks_task_code);

                return Ok(new
                {
                    Message = "✅ Lab test data inserted successfully",
                    ProcessDetailCode = processDetailCode,
                    lab_Code = labCode
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

                cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = "✅ lab data updated successfully",
                    lab_Code = request.lab_Code
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

        private Guid InsertApplicationProcessDetail(SqlConnection conn, Guid applicationCode)
        {
            using SqlCommand cmd2 = new("proc_InsertApplicationProcessDetailLabtest", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd2.Parameters.AddWithValue("@applications_application_code", applicationCode);
            
            SqlParameter outputParam = new("@process_detail_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd2.Parameters.Add(outputParam);

            cmd2.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        private Guid InsertApplicationProcessLabTestData(SqlConnection conn, string lab_test, Guid created_by, Guid? applicationProcessDetailsProcessDetailCode, Guid? diagnosis_Code)
        {
            using SqlCommand cmd = new("proc_InsertLabRequest", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@lab_test", lab_test ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@diagnosis_Code", diagnosis_Code); 
            cmd.Parameters.AddWithValue("@created_by", created_by);


            if (applicationProcessDetailsProcessDetailCode.HasValue)
                cmd.Parameters.AddWithValue("@detail_code", applicationProcessDetailsProcessDetailCode.Value);
            else
                cmd.Parameters.AddWithValue("@detail_code", DBNull.Value);

            SqlParameter outputParam = new("@lab_Code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

        // helper method to update Todo Detail ID
        private void TodoInsert(SqlConnection conn, string applicationNumber, Guid processDetailCode, Guid? UserId, Guid? organization_code, Guid? tasks_task_code)
        {
            using SqlCommand cmd = new("proc_TodoLabTest", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@application_number", applicationNumber ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@application_detail_id", processDetailCode);
            //cmd.Parameters.AddWithValue("@UserId", UserId);
            cmd.Parameters.AddWithValue("@organization_code", organization_code);
            cmd.Parameters.AddWithValue("@tasks_task_code", tasks_task_code);
            cmd.ExecuteNonQuery();
        }
    }
}
