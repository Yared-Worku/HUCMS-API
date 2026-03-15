namespace HUCMS.Models.HUCMS.Dashboard
{
    public class StudentDashboardResponse
    {
    }
}
namespace HUCMS.Models.HUCMS.PaymentRefund
{
    public class ApplicationDetail
    {
        public string? Application_No { get; set; }
        public string? Service_Name { get; set; }
        public string? Application_Date { get; set; }
        public string? status { get; set; }
        public string? RoleName { get; set; }
        public Guid? UserID { get; set; }
        public Guid? RoleID { get; set; }
    }

    public class StatusCounts
    {
        public int Completed { get; set; }
        public int Open { get; set; }
        public int Suspended { get; set; }
        public int Picked { get; set; }
        public int Rejected { get; set; }
    }

    public class StudentDashboardResponse
    {
        public StatusCounts Stats { get; set; } = new StatusCounts();
        public List<ApplicationDetail> Details { get; set; } = new List<ApplicationDetail>();
    }
}