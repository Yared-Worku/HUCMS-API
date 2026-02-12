using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace HU_api.Entities.HU
{
    public class ToDoListService
    {
        private readonly string _connectionString;

        public ToDoListService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Guid InsertToDoList(Guid tasksTaskCode, string applicationNumber,
            DateTime startDate, Guid organizationCode, string userId, Guid processDetailCode)
        {
            using SqlConnection conn = new(_connectionString);
            using SqlCommand cmd = new("proc_CreateTo_do_list", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@tasks_task_code", tasksTaskCode);
            cmd.Parameters.AddWithValue("@application_number", applicationNumber);
            cmd.Parameters.AddWithValue("@start_date", startDate);
            cmd.Parameters.AddWithValue("@organization_code", organizationCode);
            cmd.Parameters.AddWithValue("@userId", Guid.Parse(userId));
            cmd.Parameters.AddWithValue("@application_detail_id", processDetailCode);

            // add OUTPUT parameter
            SqlParameter outputParam = new("@to_do_code", SqlDbType.UniqueIdentifier)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(outputParam);

            conn.Open();
            cmd.ExecuteNonQuery();

            return (Guid)outputParam.Value;
        }

    }
}
