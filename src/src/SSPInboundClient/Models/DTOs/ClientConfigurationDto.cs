namespace SSPInboundClient.Models.DTOs
{
    /// <summary>
    /// Represents client configuration settings
    /// </summary>
    public class ClientConfigurationDto
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int RateLimitPerMinute { get; set; }
        public List<string> AllowedEndpoints { get; set; } = new List<string>();
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
        public TransformationSettingsDto? Transformation { get; set; }
    }

    /// <summary>
    /// Represents transformation settings for a client
    /// </summary>
    public class TransformationSettingsDto
    {
        public string SourceFormat { get; set; } = string.Empty;
        public string TargetFormat { get; set; } = string.Empty;
        public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>();
        public List<TransformationRuleDto> Rules { get; set; } = new List<TransformationRuleDto>();
    }

    /// <summary>
    /// Represents a transformation rule
    /// </summary>
    public class TransformationRuleDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int Priority { get; set; } = 100;
        public bool IsActive { get; set; } = true;
    }
}