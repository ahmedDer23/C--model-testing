namespace Linedata.Amp.Model.Automation.Models
{
    using System;
    using System.Collections.Concurrent;
    using global::Model.Domain;

    public class AmpModelBase
    {
        public ModelIdentifier ModelId { get; set; }
        public long? LegacyModelId { get; set; }
        public string Name { get; set; }
        public Guid SchemeId { get; set; }
        public bool Deleted { get; set; }
        public ConcurrentDictionary<Guid, AmpSecurityTarget> LongSecurityTargets { get; set; }
        public ConcurrentDictionary<Guid, AmpSecurityTarget> ShortSecurityTargets { get; set; }

        public AmpModelBase()
        {
            LongSecurityTargets = new ConcurrentDictionary<Guid, AmpSecurityTarget>();
            ShortSecurityTargets = new ConcurrentDictionary<Guid, AmpSecurityTarget>();
        }
    }
}
