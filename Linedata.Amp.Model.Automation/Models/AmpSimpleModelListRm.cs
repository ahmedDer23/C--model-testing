namespace Linedata.Amp.Model.Automation.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using ReactiveDomain.Foundation;
    using ReactiveDomain.Messaging.Bus;
    using global::Model.Domain;
    using DomainSimpleModel = global::Model.Domain.SimpleModel;

    public class AmpSimpleModelListRm :
        ReadModelBase,
        IHandle<ModelMsg.SimpleModelCreated>,
        IHandle<ModelMsg.SimpleModelDeleted>,
        IHandle<TargetListMsg.ModelNameChanged>,
        IHandle<TargetListMsg.ClassificationSchemeSetToModel>,
        IHandle<TargetListMsg.LegacyModelIdSetToModel>,
        IHandle<TargetListMsg.LongSecurityAddedToTargetList>,
        IHandle<TargetListMsg.ShortSecurityAddedToTargetList>,
        IHandle<TargetListMsg.LongSecurityDeletedFromTargetList>,
        IHandle<TargetListMsg.ShortSecurityDeletedFromTargetList>,
        IHandle<TargetListMsg.LongSecurityTargetWeightUpdated>,
        IHandle<TargetListMsg.ShortSecurityTargetWeightUpdated>
    {
        private readonly ConcurrentDictionary<ModelIdentifier, AmpSimpleModel> _simpleModelList =
            new ConcurrentDictionary<ModelIdentifier, AmpSimpleModel>();
        public IReadOnlyDictionary<ModelIdentifier, AmpSimpleModel> SimpleModels => _simpleModelList;

        private readonly ConcurrentDictionary<ModelIdentifier, AmpSimpleModel> _deletedSimpleModelList =
            new ConcurrentDictionary<ModelIdentifier, AmpSimpleModel>();
        public IReadOnlyDictionary<ModelIdentifier, AmpSimpleModel> DeletedSimpleModels => _deletedSimpleModelList;

        public AmpSimpleModelListRm(
            Func<IListener> getListener) :
            base(getListener)
        {
            EventStream.Subscribe<ModelMsg.SimpleModelCreated>(this);
            EventStream.Subscribe<ModelMsg.SimpleModelDeleted>(this);
            EventStream.Subscribe<TargetListMsg.ModelNameChanged>(this);
            EventStream.Subscribe<TargetListMsg.ClassificationSchemeSetToModel>(this);
            EventStream.Subscribe<TargetListMsg.LegacyModelIdSetToModel>(this);
            EventStream.Subscribe<TargetListMsg.LongSecurityAddedToTargetList>(this);
            EventStream.Subscribe<TargetListMsg.ShortSecurityAddedToTargetList>(this);
            EventStream.Subscribe<TargetListMsg.LongSecurityDeletedFromTargetList>(this);
            EventStream.Subscribe<TargetListMsg.ShortSecurityDeletedFromTargetList>(this);
            EventStream.Subscribe<TargetListMsg.LongSecurityTargetWeightUpdated>(this);
            EventStream.Subscribe<TargetListMsg.ShortSecurityTargetWeightUpdated>(this);

            Start<DomainSimpleModel>(blockUntilLive: true);
        }

        public void Handle(ModelMsg.SimpleModelCreated evt)
        {
            if (_simpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) already exists.");
            }

            var simpleModel = new AmpSimpleModel {
                ModelId = evt.ModelId,
                LegacyModelId = evt.LegacyModelId,
                Name = evt.ModelName,
                SchemeId = evt.SchemeId
            };

            if (!_simpleModelList.TryAdd(simpleModel.ModelId, simpleModel)) {
                throw new InvalidOperationException($"SimpleModel id({evt.ModelId}) not added.");
            }
        }

        public void Handle(TargetListMsg.ModelNameChanged evt)
        {
            if (!_simpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            _simpleModelList[evt.ModelId].Name = evt.ModelName;
        }

        public void Handle(ModelMsg.SimpleModelDeleted evt)
        {
            if (!_simpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (_deletedSimpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"DeletedModel id({evt.ModelId}) already exists");
            }

            var deletedModel = _simpleModelList[evt.ModelId];
            if (!_simpleModelList.TryRemove(deletedModel.ModelId, out deletedModel)) {
                throw new InvalidOperationException($"Cannot remove DeletedModel id {evt.ModelId}");
            }

            if (!_deletedSimpleModelList.TryAdd(deletedModel.ModelId, deletedModel)) {
                throw new InvalidOperationException($"DeletedModel id({evt.ModelId}) not added.");
            }
        }

        public void Handle(TargetListMsg.LongSecurityAddedToTargetList evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (simpleModel.LongSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Long security id({evt.SecurityId}) already exists for simple model id({evt.ModelId}).");
            }

            if (!simpleModel.LongSecurityTargets.TryAdd(
                key: evt.SecurityId,
                value: new AmpSecurityTarget {
                    SecurityId = evt.SecurityId,
                    Weight = evt.Weight
                })) {
                throw new InvalidOperationException($"LongSecurityTargets id({evt.SecurityId}) not added.");
            }
        }

        public void Handle(TargetListMsg.ShortSecurityAddedToTargetList evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (simpleModel.ShortSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Short security id({evt.SecurityId}) already exists for simple model id({evt.ModelId}).");
            }

            if (!simpleModel.ShortSecurityTargets.TryAdd(
                key: evt.SecurityId,
                value: new AmpSecurityTarget {
                    SecurityId = evt.SecurityId,
                    Weight = evt.Weight
                })) {
                throw new InvalidOperationException($"ShortSecurityTargets id({evt.SecurityId}) not added.");
            }
        }

        public void Handle(TargetListMsg.LongSecurityDeletedFromTargetList evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (!simpleModel.LongSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Long security id({evt.SecurityId}) not found for simple model id({evt.ModelId}).");
            }

            if (!simpleModel.LongSecurityTargets.TryRemove(evt.SecurityId, out var value)) {
                throw new InvalidOperationException($"LongSecurityTargets id({evt.SecurityId}) not removed.");
            }
        }

        public void Handle(TargetListMsg.ShortSecurityDeletedFromTargetList evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (!simpleModel.ShortSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Short security id({evt.SecurityId}) not found for simple model id({evt.ModelId}).");
            }

            if (!simpleModel.ShortSecurityTargets.TryRemove(evt.SecurityId, out var value)) {
                throw new InvalidOperationException($"ShortSecurityTargets id({evt.SecurityId}) not removed.");
            }
        }

        public void Handle(TargetListMsg.LongSecurityTargetWeightUpdated evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (!simpleModel.LongSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Long security id({evt.SecurityId}) not found for simple model id({evt.ModelId}).");
            }

            simpleModel.LongSecurityTargets[evt.SecurityId].Weight = evt.Weight;
        }

        public void Handle(TargetListMsg.ShortSecurityTargetWeightUpdated evt)
        {
            if (!_simpleModelList.TryGetValue(evt.ModelId, out var simpleModel)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            if (!simpleModel.ShortSecurityTargets.ContainsKey(evt.SecurityId)) {
                throw new ArgumentException(
                    $"Short security id({evt.SecurityId}) not found for simple model id({evt.ModelId}).");
            }

            simpleModel.ShortSecurityTargets[evt.SecurityId].Weight = evt.Weight;
        }

        public void Handle(TargetListMsg.LegacyModelIdSetToModel evt)
        {
            if (!_simpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            _simpleModelList[evt.ModelId].LegacyModelId = evt.LegacyModelId;
        }

        public void Handle(TargetListMsg.ClassificationSchemeSetToModel evt)
        {
            if (!_simpleModelList.ContainsKey(evt.ModelId)) {
                throw new ArgumentException($"SimpleModel id({evt.ModelId}) not found.");
            }

            _simpleModelList[evt.ModelId].SchemeId = evt.SchemeId;
        }
    }
}
