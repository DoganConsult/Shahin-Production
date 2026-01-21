using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Organization Chart with SoD (Segregation of Duties) Conflict Detection
    /// Visualizes organizational hierarchy and highlights potential conflicts
    /// </summary>
    public class OrgChartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(bool showSodConflicts = true)
        {
            var model = new OrgChartModel
            {
                ShowSodConflicts = showSodConflicts,
                Nodes = new List<OrgChartNode>
                {
                    new() { Id = 1, Name = "CEO", Title = "Chief Executive Officer", Department = "Executive", ParentId = null, HasSodConflict = false },
                    new() { Id = 2, Name = "CTO", Title = "Chief Technology Officer", Department = "Technology", ParentId = 1, HasSodConflict = false },
                    new() { Id = 3, Name = "CFO", Title = "Chief Financial Officer", Department = "Finance", ParentId = 1, HasSodConflict = false },
                    new() { Id = 4, Name = "CISO", Title = "Chief Information Security Officer", Department = "Security", ParentId = 1, HasSodConflict = true },
                    new() { Id = 5, Name = "VP Engineering", Title = "VP of Engineering", Department = "Technology", ParentId = 2, HasSodConflict = false },
                    new() { Id = 6, Name = "VP Finance", Title = "VP of Finance", Department = "Finance", ParentId = 3, HasSodConflict = true }
                },
                SodConflicts = showSodConflicts ? new List<SodConflict>
                {
                    new() { UserId = 4, UserName = "CISO", ConflictingRoles = new[] { "Security Admin", "Audit Approver" }, Severity = "High" },
                    new() { UserId = 6, UserName = "VP Finance", ConflictingRoles = new[] { "Payment Initiator", "Payment Approver" }, Severity = "Critical" }
                } : new List<SodConflict>()
            };

            return View(model);
        }
    }

    public class OrgChartModel
    {
        public bool ShowSodConflicts { get; set; } = true;
        public List<OrgChartNode> Nodes { get; set; } = new();
        public List<SodConflict> SodConflicts { get; set; } = new();
    }

    public class OrgChartNode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public bool HasSodConflict { get; set; }
    }

    public class SodConflict
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string[] ConflictingRoles { get; set; } = Array.Empty<string>();
        public string Severity { get; set; } = "Medium";
    }
}
