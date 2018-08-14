namespace Linedata.Amp.Model.Automation.Models
{
    using System;
    using System.Collections.Concurrent;
    using ACL.Service.Models;
    using global::Model.Domain;

    public class AmpSimpleModel:AmpModelBase
    {
        public ConcurrentDictionary<ModelIdentifier, SubmodelTarget> Submodels { get; set; }

        public AmpSimpleModel Copy() => new AmpSimpleModel {
            ModelId = ModelId,
            LegacyModelId = LegacyModelId,
            Name = Name,
            SchemeId = SchemeId,
            Deleted = Deleted,
            Submodels = new ConcurrentDictionary<ModelIdentifier, SubmodelTarget>(Submodels),
            LongSecurityTargets = new ConcurrentDictionary<Guid, AmpSecurityTarget>(LongSecurityTargets),
            ShortSecurityTargets = new ConcurrentDictionary<Guid, AmpSecurityTarget>(ShortSecurityTargets)
        };
    }
}
