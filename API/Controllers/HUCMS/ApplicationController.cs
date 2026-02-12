using BPM.Entities.HU;
using HU_api.Entities.HU;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HU_api.Controllers.HU
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ToDoListService _toDoListService;
        public ApplicationController(IConfiguration config)
        {
            _config = config;
            _toDoListService = new ToDoListService(_config.GetConnectionString("HU_DB"));
        }

        [HttpPost]
        public IActionResult CreateApplication([FromBody] Application app)
        {
            if (app == null || app.services_service_code == Guid.Empty)
                return BadRequest("Invalid application data");

            string connStr = _config.GetConnectionString("HU_DB");
            using SqlConnection conn = new(connStr);
            conn.Open();

            try
            {
                Guid applicationCode = Guid.Empty;
                string applicationNumber = string.Empty;
                string serviceName = string.Empty;
                Guid processDetailCode = Guid.Empty;
                Guid processDataCode = Guid.Empty;
                Guid ToDoListCode = Guid.Empty;

                if (!string.IsNullOrEmpty(app.application_number))
                {
                    var (dbProcessDetailCode, toDoCode) = GetProcessDetailCode(conn, app.application_number);

                    if (dbProcessDetailCode != Guid.Empty)
                    {
                        processDataCode = InsertApplicationProcessData(conn, app.value, dbProcessDetailCode);
                    }

                    return Ok(new
                    {
                        Message = "✅ Existing application processed",
                        ApplicationNumber = app.application_number,
                        ProcessDetailCode = dbProcessDetailCode,
                        ToDoCode = toDoCode,
                        ProcessDataCode = processDataCode
                    });

                }

                else
                {
                    // ❌ application_number null: full creation process

                    using SqlCommand cmd = new("proc_CreateApplication", conn)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    string registrationCode = app.pro_name;
                    if (app.organization_code != null && app.organization_code != Guid.Empty)
                    {
                        registrationCode = GetOrgRegistrationCode(conn, app.organization_code)?.Trim() ?? app.pro_name;
                    }

                    cmd.Parameters.AddWithValue("@application_code",
                        (app.application_code != null && app.application_code != Guid.Empty)
                            ? app.application_code
                            : Guid.NewGuid());

                    cmd.Parameters.AddWithValue("@application_number",
                        app.application_number ?? $"{registrationCode}-{DateTime.Now.Ticks}");

                    cmd.Parameters.AddWithValue("@is_completed", app.is_completed ?? 0);
                    cmd.Parameters.AddWithValue("@status", app.status ?? "Draft");


                    var dateCreatedParam = cmd.Parameters.Add("@date_created", SqlDbType.DateTime);
                    dateCreatedParam.Value = app.date_created == null || app.date_created == default ? DateTime.Now : app.date_created;

                    cmd.Parameters.AddWithValue("@date_created_et", app.date_created_et ?? EthiopianDateConverter.ToEthiopianDateTimeString(DateTime.UtcNow));
                    cmd.Parameters.AddWithValue("@UserId", app.UserId ?? "00000000-0000-0000-0000-000000000000");
                    cmd.Parameters.AddWithValue("@services_service_code", app.services_service_code);
                    cmd.Parameters.AddWithValue("@is_synched", app.is_synched ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@date_synched", app.date_synched ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@organization_code", app.organization_code ?? (object)DBNull.Value);
                 
                    // ✅ Execute stored procedure and read results
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            applicationCode = reader.GetGuid(reader.GetOrdinal("InsertedApplicationCode"));
                            applicationNumber = reader.GetString(reader.GetOrdinal("ApplicationNumber"));
                        }

                        if (reader.NextResult() && reader.Read())
                        {
                            serviceName = reader.GetString(reader.GetOrdinal("ServiceName"));
                        }
                    }

                    // Insert process details & data
                    processDetailCode = InsertApplicationProcessDetail(conn, applicationCode, app.tasks_task_code.Value);
                    processDataCode = InsertApplicationProcessData(conn, app.value, processDetailCode);

                    // Insert to-do list
                    ToDoListCode = _toDoListService.InsertToDoList(
                        app.tasks_task_code.Value,
                        applicationNumber,
                        DateTime.Now,
                        app.organization_code ?? Guid.Empty,
                        app.UserId ?? "00000000-0000-0000-0000-000000000000",
                        processDetailCode 
                    );

                    return Ok(new
                    {
                        Message = "✅ New application created successfully",
                        ApplicationCode = applicationCode,
                        ApplicationNumber = applicationNumber,
                        ServiceName = serviceName,
                        ProcessDetailCode = processDetailCode,
                        ProcessDataCode = processDataCode,
                        ToDoCode = ToDoListCode
                    });
                }
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

        // Helper to get process detail code if application exists
        private (Guid ApplicationDetailId, Guid ToDoCode) GetProcessDetailCode(SqlConnection conn, string applicationNumber)
        {
            using SqlCommand cmd = new("proc_GetDetailCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@application_number", applicationNumber);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return (reader.GetGuid(0), reader.GetGuid(1));
            }

            return (Guid.Empty, Guid.Empty);
        }


        // New helper method to get registration code
        private string GetOrgRegistrationCode(SqlConnection conn, Guid? organizationCode)
        {
            using SqlCommand cmd = new("proc_GetOrgRegistrationCode", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@organization_code", organizationCode);

            var result = cmd.ExecuteScalar();
            return result != null ? result.ToString() : null;
        }

       [HttpGet]
        public IActionResult GetApplications([FromQuery] Guid userId)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            try
            {
                using SqlConnection conn = new(connStr);
                using SqlCommand cmd = new("sp_GetApplicationsByUser", conn) 
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@userId", userId);
                conn.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                var results = new List<object>();

                while (reader.Read())
                {
                    results.Add(new
                    {
                        userId = reader["userId"] != DBNull.Value ? reader["userId"].ToString() : null,
                        tasks_task_code = reader["tasks_task_code"] != DBNull.Value ? Guid.Parse(reader["tasks_task_code"].ToString()) : Guid.Empty,
                        description_en = reader["description_en"]?.ToString(),
                        service_description_en = reader["service_description_en"]?.ToString(),
                        description_local = reader["description_local"]?.ToString(),
                        meta_data_forms_form_code = reader["meta_data_forms_form_code"] != DBNull.Value ? Guid.Parse(reader["meta_data_forms_form_code"].ToString()) : Guid.Empty,
                        RoleId = reader["RoleId"] != DBNull.Value ? Guid.Parse(reader["RoleId"].ToString()) : Guid.Empty,
                        services_service_code = reader["services_service_code"] != DBNull.Value ? Guid.Parse(reader["services_service_code"].ToString()) : Guid.Empty,
                        status = reader["status"]?.ToString(),
                        organization_code = reader["organization_code"] != DBNull.Value ? Guid.Parse(reader["organization_code"].ToString()) : Guid.Empty,
                        application_detail_id = reader["application_detail_id"] != DBNull.Value ? Guid.Parse(reader["application_detail_id"].ToString()) : Guid.Empty,
                        application_number = reader["application_number"]?.ToString(),
                        start_date = reader["start_date"] != DBNull.Value ? Convert.ToDateTime(reader["start_date"]) : (DateTime?)null
                    });
                }

                return Ok(results);
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
        //private string GetUserIdByUsername(string connStr, string username)
        //{
        //    using SqlConnection conn = new(connStr);
        //    using SqlCommand cmd = new("SELECT userId FROM AspNetUser WHERE Username = @Username", conn);
        //    cmd.Parameters.AddWithValue("@Username", username);

        //    conn.Open();
        //    var result = cmd.ExecuteScalar();
        //    return result != null ? result.ToString() : null;
        //}

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

        private Guid InsertApplicationProcessData(SqlConnection conn, string value, Guid? applicationProcessDetailsProcessDetailCode)
        {
            using SqlCommand cmd = new("proc_InsertApplicationprocessdata", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@value", value ?? (object)DBNull.Value);

            if (applicationProcessDetailsProcessDetailCode.HasValue)
                cmd.Parameters.AddWithValue("@application_process_details_process_detail_code", applicationProcessDetailsProcessDetailCode.Value);
            else
                cmd.Parameters.AddWithValue("@application_process_details_process_detail_code", DBNull.Value);

            SqlParameter outputParam = new("@process_data_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }
    }
}

public static class EthiopianDateConverter
{
    public static (int Year, int Month, int Day) ToEthiopianDate(DateTime gregorianDate)
    {
        int gYear = gregorianDate.Year;
        int gMonth = gregorianDate.Month;
        int gDay = gregorianDate.Day;

        int ethiopianYear = gYear - 7;

        bool nextGregorianYearIsLeap = DateTime.IsLeapYear(gYear + 1);
        int newYearDay = nextGregorianYearIsLeap ? 12 : 11;

        int gregorianDayOfYear = gregorianDate.DayOfYear;
        DateTime ethiopianNewYearGregorian = new DateTime(gYear, 9, newYearDay);
        int ethiopianNewYearDayOfYear = ethiopianNewYearGregorian.DayOfYear;

        int ethiopianMonth = 1;
        int ethiopianDay = 1;

        int daysSinceEthiopianNewYear = gregorianDayOfYear - ethiopianNewYearDayOfYear;

        if (daysSinceEthiopianNewYear >= 0)
        {
            ethiopianMonth = (daysSinceEthiopianNewYear / 30) + 1;
            ethiopianDay = (daysSinceEthiopianNewYear % 30) + 1;
        }
        else
        {
            ethiopianYear--;
            int prevYear = gYear - 1;
            bool prevYearLeap = DateTime.IsLeapYear(prevYear + 1);
            int prevEthiopianNewYearDay = prevYearLeap ? 12 : 11;
            DateTime prevEthiopianNewYearGregorian = new DateTime(prevYear, 9, prevEthiopianNewYearDay);
            int daysInYear = (ethiopianNewYearGregorian - prevEthiopianNewYearGregorian).Days;

            int daysFromPrevNewYear = daysSinceEthiopianNewYear + daysInYear;

            ethiopianMonth = (daysFromPrevNewYear / 30) + 1;
            ethiopianDay = (daysFromPrevNewYear % 30) + 1;
        }

        return (ethiopianYear, ethiopianMonth, ethiopianDay);
    }

    public static string ToEthiopianDateTimeString(DateTime utcDateTime)
    {
        DateTime ethiopianLocalTime = utcDateTime.AddHours(3);

        var (year, month, day) = ToEthiopianDate(ethiopianLocalTime);

        string timePart = ethiopianLocalTime.ToString("HH:mm");

        return $"{year:0000}-{month:00}-{day:00} {timePart}";
    }
}
