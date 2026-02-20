using HUCMS.Models.HUCMS.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HUCMS.Controllers.HUCMS.Commons
{
    [Route("api/HU/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CustomerController(IConfiguration config)
        {
            _config = config;
        }

        // 🔹 POST: api/HU/Customer
        [HttpPost]
        public IActionResult InsertCustomer([FromBody] Customer customer)
        {

            // ✅ Use Customer_ID and Created_By from frontend
            if (customer.Customer_ID == Guid.Empty)
                return BadRequest("Customer_ID is required from frontend.");

            return SaveCustomer(customer, isInsert: true);
        }

        // 🔹 PUT: api/HU/Customer/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateCustomer(Guid id, [FromBody] Customer customer)
        {
            if (customer == null || id == Guid.Empty)
                return BadRequest("Invalid customer data");

            customer.Customer_ID = id;

            return SaveCustomer(customer, isInsert: false);
        }

        // 🔹 Common Save Method (Insert or Update)
        private IActionResult SaveCustomer(Customer customer, bool isInsert)
        {
            string connStr = _config.GetConnectionString("HU_DB");

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_AddCustomer", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@Customer_ID", customer.Customer_ID);

            // Map other fields
            cmd.Parameters.AddWithValue("@Applicant_First_Name_AM", customer.Applicant_First_Name_AM ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Applicant_First_Name_EN", customer.Applicant_First_Name_EN ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Applicant_Middle_Name_AM", customer.Applicant_Middle_Name_AM ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Applicant_Middle_Name_En", customer.Applicant_Middle_Name_En ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Applicant_Last_Name_AM", customer.Applicant_Last_Name_AM ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Applicant_Last_Name_EN", customer.Applicant_Last_Name_EN ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@TIN", customer.TIN ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Gender", customer.Gender ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Email", customer.Email ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Age", customer.Age ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Mobile_No", customer.Mobile_No ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Photo", customer.Photo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@SDP_ID", customer.SDP_ID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ID_NO", customer.ID_NO ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Created_By", customer.Created_By);
            cmd.Parameters.AddWithValue("@Updated_By", customer.Updated_By ?? "00000000-0000-0000-0000-000000000000");

            cmd.Parameters.AddWithValue("@Created_Date", customer.Created_Date ?? DateTime.Now);
            cmd.Parameters.AddWithValue("@Updated_Date", customer.Updated_Date ?? DateTime.Now);
            //cmd.Parameters.AddWithValue("@Signiture", customer.Signiture ?? (object)DBNull.Value);
            try
            {
                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                return Ok(new
                {
                    Message = isInsert ? "✅ Customer inserted successfully" : "✅ Customer updated successfully",
                    customer.Customer_ID,
                    AffectedRows = rows
                });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Database error", Details = ex.Message });
            }
        }

        // 🔹 GET: api/HU/Customer/{userid}
        [HttpGet("{userid}")]
        public IActionResult GetCustomerByUsername(string userid)
        {
            string connStr = _config.GetConnectionString("HU_DB");
            var result = new List<Customer>();

            using SqlConnection conn = new(connStr);
            using SqlCommand cmd = new("sp_GetCustomerByUsername", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@userid", userid);

            try
            {
                conn.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(new Customer
                    {
                        Customer_ID = reader["Customer_ID"] != DBNull.Value ? (Guid)reader["Customer_ID"] : Guid.Empty,
                        Applicant_First_Name_AM = reader["Applicant_First_Name_AM"]?.ToString(),
                        Applicant_First_Name_EN = reader["Applicant_First_Name_EN"]?.ToString(),
                        Applicant_Middle_Name_AM = reader["Applicant_Middle_Name_AM"]?.ToString(),
                        Applicant_Middle_Name_En = reader["Applicant_Middle_Name_En"]?.ToString(),
                        Applicant_Last_Name_AM = reader["Applicant_Last_Name_AM"]?.ToString(),
                        Applicant_Last_Name_EN = reader["Applicant_Last_Name_EN"]?.ToString(),
                       TIN = reader["TIN"]?.ToString(),
                        Gender = reader["Gender"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Age = reader["Age"] == DBNull.Value ? null : Convert.ToInt32(reader["Age"]),
                        Mobile_No = reader["Mobile_No"]?.ToString(),
                        ID_NO = reader["ID_NO"]?.ToString(),
                        Photo = reader["Photo"]?.ToString(),
                        depname = reader["depname"]?.ToString(),
                        SDP_ID = reader["SDP_ID"]?.ToString(),
                     
                        Created_By = reader["Created_By"]?.ToString(),
                        Updated_By = reader["Updated_By"]?.ToString(),
                        Created_Date = reader["Created_Date"] != DBNull.Value ? (DateTime?)reader["Created_Date"] : null,
                        Updated_Date = reader["Updated_Date"] != DBNull.Value ? (DateTime?)reader["Updated_Date"] : null,
                        //Signiture = reader["Signiture"]?.ToString()
                       
                    });
                }

                return Ok(result);
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Error = "❌ Failed to retrieve customer by username", Details = ex.Message });
            }
        }
    }
}
