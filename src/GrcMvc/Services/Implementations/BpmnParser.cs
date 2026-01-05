using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace GrcMvc.Services.Implementations
{
    /// <summary>
    /// BPMN 2.0 XML Parser - Extracts workflow steps from BPMN XML
    /// Supports startEvent, userTask, endEvent elements
    /// </summary>
    public class BpmnParser
    {
        private readonly ILogger<BpmnParser> _logger;

        public BpmnParser(ILogger<BpmnParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Parse BPMN XML and extract workflow steps
        /// </summary>
        public BpmnWorkflow Parse(string bpmnXml)
        {
            if (string.IsNullOrWhiteSpace(bpmnXml))
            {
                _logger.LogWarning("BPMN XML is empty, returning empty workflow");
                return new BpmnWorkflow { Steps = new List<BpmnStep>() };
            }

            try
            {
                var doc = XDocument.Parse(bpmnXml);
                XNamespace bpmn = "http://www.omg.org/spec/BPMN/20100524/MODEL";

                var process = doc.Descendants(bpmn + "process").FirstOrDefault();
                if (process == null)
                {
                    _logger.LogWarning("No BPMN process found in XML");
                    return new BpmnWorkflow { Steps = new List<BpmnStep>() };
                }

                var steps = new List<BpmnStep>();
                int sequence = 0;

                // Extract all BPMN elements in order
                var elements = process.Elements()
                    .Where(e => e.Name.LocalName == "startEvent" || 
                                e.Name.LocalName == "userTask" || 
                                e.Name.LocalName == "endEvent")
                    .ToList();

                foreach (var element in elements)
                {
                    sequence++;
                    var step = new BpmnStep
                    {
                        Id = element.Attribute("id")?.Value ?? $"step_{sequence}",
                        Name = element.Attribute("name")?.Value ?? element.Name.LocalName,
                        Type = MapBpmnType(element.Name.LocalName),
                        Sequence = sequence,
                        Assignee = element.Descendants().FirstOrDefault(d => d.Name.LocalName == "assignee")?.Value,
                        DueDateOffsetDays = ParseDueDateOffset(element),
                        Priority = ParsePriority(element),
                        Description = element.Descendants().FirstOrDefault(d => d.Name.LocalName == "documentation")?.Value
                    };

                    steps.Add(step);
                }

                _logger.LogInformation("Parsed BPMN workflow with {StepCount} steps", steps.Count);
                return new BpmnWorkflow { Steps = steps };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing BPMN XML");
                throw new InvalidOperationException($"Failed to parse BPMN XML: {ex.Message}", ex);
            }
        }

        private BpmnStepType MapBpmnType(string bpmnElementName)
        {
            return bpmnElementName switch
            {
                "startEvent" => BpmnStepType.Start,
                "userTask" => BpmnStepType.Task,
                "endEvent" => BpmnStepType.End,
                _ => BpmnStepType.Task
            };
        }

        private int? ParseDueDateOffset(XElement element)
        {
            // Look for dueDateOffsetDays in extension elements or attributes
            var dueDateAttr = element.Attribute("dueDateOffsetDays");
            if (dueDateAttr != null && int.TryParse(dueDateAttr.Value, out var days))
                return days;

            // Check in extension elements
            var extension = element.Descendants().FirstOrDefault(d => d.Name.LocalName == "dueDateOffsetDays");
            if (extension != null && int.TryParse(extension.Value, out var days2))
                return days2;

            return null;
        }

        private int ParsePriority(XElement element)
        {
            // Default priority: 2 (Medium)
            var priorityAttr = element.Attribute("priority");
            if (priorityAttr != null && int.TryParse(priorityAttr.Value, out var priority))
                return Math.Clamp(priority, 1, 4); // 1=High, 2=Medium, 3=Low, 4=Critical

            return 2; // Default: Medium
        }
    }

    /// <summary>
    /// Parsed BPMN Workflow structure
    /// </summary>
    public class BpmnWorkflow
    {
        public List<BpmnStep> Steps { get; set; } = new();
    }

    /// <summary>
    /// BPMN Step definition
    /// </summary>
    public class BpmnStep
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public BpmnStepType Type { get; set; }
        public int Sequence { get; set; }
        public string? Assignee { get; set; }
        public int? DueDateOffsetDays { get; set; }
        public int Priority { get; set; } = 2;
        public string? Description { get; set; }
    }

    /// <summary>
    /// BPMN Step Type
    /// </summary>
    public enum BpmnStepType
    {
        Start,
        Task,
        End
    }
}
