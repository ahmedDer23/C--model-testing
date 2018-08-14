namespace Linedata.Amp.Model.Automation.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using ReactiveDomain.Foundation;
    using ReactiveDomain.Messaging.Bus;
    using global::Model.Domain;
    using DomainSecurity = global::Model.Domain.Aggregates.Security;

    public class AmpSecurityListRm :
        ReadModelBase,
        IHandle<SecurityMsg.SecurityCreated>,
        IHandle<SecurityMsg.SecurityDeleted>
    {
        private readonly ConcurrentDictionary<Guid, AmpSecurity> _securityList = new ConcurrentDictionary<Guid, AmpSecurity>();

        public IReadOnlyDictionary<Guid, AmpSecurity> Securities => _securityList;

        public AmpSecurityListRm(Func<IListener> getListener,Guid id=new Guid()) : base(getListener)
        {
            EventStream.Subscribe<SecurityMsg.SecurityCreated>(this);
            EventStream.Subscribe<SecurityMsg.SecurityDeleted>(this);

            Start<DomainSecurity>(blockUntilLive: true);
        }

        public void Handle(SecurityMsg.SecurityCreated evt)
        {
            if (_securityList.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException($"Security id({evt.SecurityId}) already exists.");
            }

            var security = new AmpSecurity {
                SecurityId = evt.SecurityId,
                LegacySecurityId = evt.LegacySecurityId,
                Name = evt.FullName,
                AssetTypeCode = (int)evt.AssetType
            };

            if (!_securityList.TryAdd(security.SecurityId, security)) {
                throw new InvalidOperationException($"Security id({evt.SecurityId}) not added.");
            }
        }

        public void Handle(SecurityMsg.SecurityDeleted evt)
        {
            if (!_securityList.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException($"Security id({evt.SecurityId}) not found.");
            }

            _securityList[evt.SecurityId].Deleted = true;
        }
    }
}
