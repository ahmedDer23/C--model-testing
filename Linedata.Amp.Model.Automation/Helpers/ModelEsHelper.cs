namespace Linedata.Amp.Model.Automation.Helpers
{
    using System;
    using System.Linq;
    using ReactiveDomain.Foundation;
    using Models;

    public static class ModelEsHelper
    {
        private static readonly Func<string, IListener> _getListener = EventStoreHelper.GetListener;

        public static AmpSecurity GetSecurityByName(string securityName)
        {
            var securityRm = new AmpSecurityListRm(() => _getListener(nameof(AmpSecurityListRm)));
            var security = (from sec in securityRm.Securities.Values
                where sec.Name == securityName.Trim()
                select sec).SingleOrDefault();
            return security;
        }

        public static AmpSecurity GetSecurityById(long securityId)
        {
            var securityRm = new AmpSecurityListRm(() => _getListener(nameof(AmpSecurityListRm)));
            var security = (from sec in securityRm.Securities.Values
                where sec.LegacySecurityId == securityId
                select sec).SingleOrDefault();
            return security;
        }

        public static AmpSimpleModel GetSimpleModelByLegacyModelId(long legacyModelId)
        {
            var simpleModelRm = new AmpSimpleModelListRm(() => _getListener(nameof(AmpSimpleModelListRm)));
            var ampModel = (from model in simpleModelRm.SimpleModels.Values
                where model.LegacyModelId == legacyModelId
                select model).SingleOrDefault();
            return ampModel;
        }
        public static AmpSimpleModel GetDeletedSimpleModelById(long legacyModelId)
        {
            var simpleModelRm = new AmpSimpleModelListRm(() => _getListener(nameof(AmpSimpleModelListRm)));
            var ampModel = (from model in simpleModelRm.DeletedSimpleModels.Values
                where model.LegacyModelId == legacyModelId
                select model).SingleOrDefault();
            return ampModel;
        }

        public static AmpSimpleModel GetModelWithSecurities(long legacyModelId)
        {
            var ampModel = GetSimpleModelByLegacyModelId(legacyModelId);
            if (ampModel.LongSecurityTargets.Count > 0 || ampModel.ShortSecurityTargets.Count > 0) {
                return ampModel;
            }
            return null;
        }
    }
}
