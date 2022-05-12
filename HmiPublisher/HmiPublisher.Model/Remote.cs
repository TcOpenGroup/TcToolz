using Newtonsoft.Json;
using System.ComponentModel;

namespace HmiPublisher.Model
{
    public class Remote
    {
        [DefaultValueAttribute(1)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string TargetUserName { get; set; }
        public string TargetPass { get; set; }
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string SourcePathDocumentation { get; set; }
        public string DestinationPathDocumentation { get; set; }
        public string ExecutableFilePath { get; set; }
        public bool ClearSharedFolder { get; set; }
        // obsolete
        public bool Exclude { get; set; }
        public bool Include { get; set; } = true;
        public Compression Compression { get; set; } = Compression.Default;

        [JsonIgnore]
        public string FullSourcePath { get; set; }
        [JsonIgnore]
        public string IpAddress { get; set; }
        [JsonIgnore]
        public bool InProgress { get; set; }
        [JsonIgnore]
        public bool IsError { get; set; }
        [JsonIgnore]
        public bool Compressing { get; set; }
        [JsonIgnore]
        public bool IsSuccess { get; set; }
        [JsonIgnore]
        public bool IndeterminateProgressBar { get; set; }
        [JsonIgnore]
        public string ProgressMessage { get; set; }
        [JsonIgnore]
        public string ProgressMBytes { get; set; }
        [JsonIgnore]
        public int ProgressPercentage { get; set; }
    }

    public enum Compression
    {
        [Description("None")]
        None,
        [Description("Level0")]
        Level0,
        [Description("BestSpeed")]
        BestSpeed,
        [Description("Level1")]
        Level1,
        [Description("Level2")]
        Level2,
        [Description("Level3")]
        Level3,
        [Description("Level4")]
        Level4,
        [Description("Level5")]
        Level5,
        [Description("Default")]
        Default,
        [Description("Level6")]
        Level6,
        [Description("Level7")]
        Level7,
        [Description("Level8")]
        Level8,
        [Description("BestCompression")]
        BestCompression,
        //[Description("Level9")]
        //Level9
    }

}
