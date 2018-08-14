namespace Linedata.Amp.Model.Automation.Models
{
    using System;

    public class AmpSecurity
    {
        public Guid SecurityId { get; set; }
        public long LegacySecurityId { get; set; }
        public string Name { get; set; }
        public int AssetTypeCode { get; set; }
        public bool Deleted { get; set; }
    }
}
