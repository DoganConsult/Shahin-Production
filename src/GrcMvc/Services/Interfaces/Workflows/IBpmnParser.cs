using GrcMvc.Services.Implementations;

namespace GrcMvc.Services.Interfaces.Workflows
{
    /// <summary>
    /// Interface for BPMN 2.0 XML parsing
    /// </summary>
    public interface IBpmnParser
    {
        /// <summary>
        /// Parse BPMN XML and extract workflow steps
        /// </summary>
        BpmnWorkflow Parse(string bpmnXml);
    }
}
