using Microsoft.AspNetCore.Mvc;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Renders a sortable, filterable data table
    /// Usage: @await Component.InvokeAsync("DataTable", new { id = "risks-table", columns = columns, data = items })
    /// </summary>
    public class DataTableViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            string id,
            List<DataTableColumn> columns,
            IEnumerable<object>? data = null,
            bool searchable = true,
            bool sortable = true,
            bool paginated = true,
            int pageSize = 10,
            string? emptyMessage = null)
        {
            var model = new DataTableModel
            {
                Id = id,
                Columns = columns,
                Data = data?.ToList() ?? new List<object>(),
                Searchable = searchable,
                Sortable = sortable,
                Paginated = paginated,
                PageSize = pageSize,
                EmptyMessage = emptyMessage
            };

            return View(model);
        }
    }

    public class DataTableModel
    {
        public string Id { get; set; } = "";
        public List<DataTableColumn> Columns { get; set; } = new();
        public List<object> Data { get; set; } = new();
        public bool Searchable { get; set; } = true;
        public bool Sortable { get; set; } = true;
        public bool Paginated { get; set; } = true;
        public int PageSize { get; set; } = 10;
        public string? EmptyMessage { get; set; }
    }

    public class DataTableColumn
    {
        public string Key { get; set; } = "";
        public string Title { get; set; } = "";
        public string? TitleAr { get; set; }
        public string? Width { get; set; }
        public bool Sortable { get; set; } = true;
        public bool Searchable { get; set; } = true;
        public string? Template { get; set; } // status, date, badge, link, actions
        public string? Format { get; set; }
    }
}
