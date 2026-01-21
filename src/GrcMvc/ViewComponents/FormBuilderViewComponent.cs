using Microsoft.AspNetCore.Mvc;
using GrcMvc.Services.Interfaces;

namespace GrcMvc.ViewComponents
{
    /// <summary>
    /// Dynamic Form Builder - No-code assessment form creator with conditional logic
    /// </summary>
    public class FormBuilderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(Guid? formId = null, string formType = "assessment")
        {
            var model = new FormBuilderModel
            {
                FormId = formId,
                FormType = formType,
                AvailableFields = new List<FormFieldType>
                {
                    new() { Id = "text", Name = "Text Input", Icon = "bi-input-cursor-text" },
                    new() { Id = "textarea", Name = "Long Text", Icon = "bi-textarea" },
                    new() { Id = "number", Name = "Number", Icon = "bi-123" },
                    new() { Id = "select", Name = "Dropdown", Icon = "bi-chevron-down" },
                    new() { Id = "radio", Name = "Radio Buttons", Icon = "bi-ui-radios" },
                    new() { Id = "checkbox", Name = "Checkboxes", Icon = "bi-ui-checks" },
                    new() { Id = "date", Name = "Date Picker", Icon = "bi-calendar" },
                    new() { Id = "file", Name = "File Upload", Icon = "bi-upload" },
                    new() { Id = "rating", Name = "Rating Scale", Icon = "bi-star" },
                    new() { Id = "matrix", Name = "Matrix/Grid", Icon = "bi-grid" },
                    new() { Id = "section", Name = "Section Header", Icon = "bi-layout-text-sidebar" }
                }
            };

            return View(model);
        }
    }

    public class FormBuilderModel
    {
        public Guid? FormId { get; set; }
        public string FormName { get; set; } = "New Form";
        public string FormType { get; set; } = "assessment";
        public string FormJson { get; set; }
        public List<FormFieldType> AvailableFields { get; set; } = new();
    }

    public class FormFieldType
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
