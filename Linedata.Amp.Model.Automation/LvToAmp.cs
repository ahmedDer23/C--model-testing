namespace Linedata.Amp.Model.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Serilog;
    using Xunit;
    using Helpers;
    using global::Model.Domain;
    using Qa.Foundation;
    using Models;

    public class LvToAmp:IClassFixture<TestFixture>
    {
        readonly long _currentUserId;
        readonly ILogger _logger;
        readonly TestFixture _fixture;
        public LvToAmp(TestFixture fixture)
        {
            _fixture = fixture;
            _fixture.FileName = nameof(LvToAmp);
            _logger = _fixture.Logger;
            _currentUserId = CommonHelper.GetCurrentUserId();
        }

        public sealed class SimpleModelInput : TestObjectBase
        {
            public string Name { get; set; }
            public int Hierarchy { get; set; }
            public List<ModelSecurity> LongSecurities { get; set; }
            public List<ModelSecurity> ShortSecurities { get; set; }
        }

        public sealed class SimpleModelOutput
        {
            public long LegacyModelId { get; set; }
            public string Name { get; set; }
            public ModelType ModelType { get; set; }
            public List<ModelSecurity> LongSecurities { get; set; }
            public List<ModelSecurity> ShortSecurities { get; set; }
        }

        public sealed class ModelSecurity
        {
            public long LegacyId { get; set; }
            public string Type { get; set; }
            public decimal Weight { get; set; }
        }

        [Theory]
        [TestData("CreateSimpleModel")]
        public async Task CreateSimpleModel(TestCase<SimpleModelInput, SimpleModelOutput> tc)
        {
            try 
            {
                var newModel = new AmpSimpleModel();
                var ampModel = new SimpleModelOutput();

                //Create Model in LV
                tc.Input.Name = tc.Expected.Name = Guid.NewGuid().ToString();
                var legacyModelId = ModelLvHelper.CreateModelFromLv(tc.Input.Name, tc.Input.Hierarchy, _currentUserId);
                tc.Expected.LegacyModelId = legacyModelId;

                //Get model from Event Store
                newModel = CommonHelper.Execute(
                    () => ModelEsHelper.GetSimpleModelByLegacyModelId(legacyModelId),
                    TimeSpan.FromMilliseconds(10),
                    300,
                    newModel);

                //Actual data
                if (newModel != null) {
                    ampModel.LegacyModelId = newModel.LegacyModelId ?? -1;
                    ampModel.Name = newModel.Name;
                    ampModel.ModelType = newModel.ModelId.ModelType;
                }

                await tc.Verify(ampModel);
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Something went wrong in test case {tc.TestCaseId}");
                throw;
            }
        }

        [Theory]
        [TestData("AddLongSecurities")]
        [TestData("UpdateLongSecurities")]
        public async Task AddLongSecurities(TestCase<SimpleModelInput, SimpleModelOutput> tc)
        {
            try {
                var newModel = new AmpSimpleModel();
                var ampModel = new SimpleModelOutput();
                var securityList = new List<AmpSecurity>();

                //Create Model in LV
                tc.Input.Name = tc.Expected.Name = Guid.NewGuid().ToString();
                var legacyModelId = ModelLvHelper.CreateModelFromLv(tc.Input.Name, tc.Input.Hierarchy, _currentUserId);
                tc.Expected.LegacyModelId = legacyModelId;
                Thread.Sleep(2000);

                //Add securities
                foreach (var security in tc.Input.LongSecurities) {
                    ModelLvHelper.AddSecurityToModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        weight: security.Weight,
                        positionType: security.Type);

                    Thread.Sleep(1000);

                    //Get securities from event store
                    var ampSecurity = ModelEsHelper.GetSecurityById(security.LegacyId);

                    if (securityList.TrueForAll(s => s.SecurityId != ampSecurity.SecurityId)) {
                        securityList.Add(ampSecurity);
                    }
                }

                //Get model from Event Store
                newModel = CommonHelper.Execute(
                    () => ModelEsHelper.GetSimpleModelByLegacyModelId(legacyModelId),
                    TimeSpan.FromMilliseconds(10),
                    300,
                    newModel);

                //Actual data
                if (newModel != null) {
                    ampModel.LegacyModelId = newModel.LegacyModelId ?? -1;
                    ampModel.Name = newModel.Name;
                    ampModel.ModelType = newModel.ModelId.ModelType;

                    var modelSecurityList = new List<ModelSecurity>();
                    foreach (var security in securityList) {
                        if (newModel.LongSecurityTargets.ContainsKey(security.SecurityId)) {
                            var modelSecurity = new ModelSecurity {
                                LegacyId = security.LegacySecurityId,
                                Type = "Long",
                                Weight = newModel.LongSecurityTargets[security.SecurityId].Weight
                            };
                            modelSecurityList.Add(modelSecurity);
                        }
                    }

                    ampModel.LongSecurities = modelSecurityList;
                }

                await tc.Verify(ampModel);
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Something went wrong in test case {tc.TestCaseId}");
                throw;
            }
        }

        [Theory]
        [TestData("AddShortSecurities")]
        [TestData("UpdateShortSecurities")]
        public async Task AddShortSecurities(TestCase<SimpleModelInput, SimpleModelOutput> tc)
        {
            try 
            {
                var newModel = new AmpSimpleModel();
                var ampModel = new SimpleModelOutput();
                var securityList = new List<AmpSecurity>();

                //Create Model in LV
                tc.Input.Name = tc.Expected.Name = Guid.NewGuid().ToString();
                var legacyModelId = ModelLvHelper.CreateModelFromLv(tc.Input.Name, tc.Input.Hierarchy, _currentUserId);
                tc.Expected.LegacyModelId = legacyModelId;
                Thread.Sleep(2000);

                //Add securities
                foreach (var security in tc.Input.ShortSecurities) {
                    ModelLvHelper.AddSecurityToModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        weight: security.Weight,
                        positionType: security.Type);

                    Thread.Sleep(1000);

                    //Get securities from event store
                    var ampSecurity = ModelEsHelper.GetSecurityById(security.LegacyId);

                    if (securityList.TrueForAll(s => s.SecurityId != ampSecurity.SecurityId)) {
                        securityList.Add(ampSecurity);
                    }
                }

                //Get model from Event Store
                newModel = CommonHelper.Execute(
                    () => ModelEsHelper.GetSimpleModelByLegacyModelId(legacyModelId),
                    TimeSpan.FromMilliseconds(10),
                    300,
                    newModel);

                //Actual data
                if (newModel != null) {
                    ampModel.LegacyModelId = newModel.LegacyModelId ?? -1;
                    ampModel.Name = newModel.Name;
                    ampModel.ModelType = newModel.ModelId.ModelType;

                    var modelSecurityList = new List<ModelSecurity>();
                    foreach (var security in securityList) {
                        if (newModel.ShortSecurityTargets.ContainsKey(security.SecurityId)) {
                            var modelSecurity = new ModelSecurity {
                                LegacyId = security.LegacySecurityId,
                                Type = "Short",
                                Weight = newModel.ShortSecurityTargets[security.SecurityId].Weight
                            };
                            modelSecurityList.Add(modelSecurity);
                        }
                    }

                    ampModel.ShortSecurities = modelSecurityList;
                }

                await tc.Verify(ampModel);
            }
            catch (Exception ex) {
                _logger.Error(ex, $"Something went wrong in test case {tc.TestCaseId}");
                throw;
            }
        }

        [Theory]
        [TestData("DeleteLongSecurities")]
        public async Task DeleteLongSecurities(TestCase<SimpleModelInput, SimpleModelOutput> tc)
        {
            try
            {
                var newModel = new AmpSimpleModel();
                var ampModel = new SimpleModelOutput();
                var securityList = new List<AmpSecurity>();

                //Create Model in LV
                tc.Input.Name = tc.Expected.Name = Guid.NewGuid().ToString();
                var legacyModelId = ModelLvHelper.CreateModelFromLv(tc.Input.Name, tc.Input.Hierarchy, _currentUserId);
                tc.Expected.LegacyModelId = legacyModelId;
                Thread.Sleep(2000);

                foreach (var security in tc.Input.LongSecurities) {

                    //Add securities
                    ModelLvHelper.AddSecurityToModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        weight: security.Weight,
                        positionType: security.Type);

                    Thread.Sleep(1000);

                    //Remove Securities
                    ModelLvHelper.RemoveSecurityFromModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        positionType: security.Type,
                        clearOverride: 1);

                    Thread.Sleep(1000);

                    //Get securities from event store
                    var ampSecurity = ModelEsHelper.GetSecurityById(security.LegacyId);

                    if (securityList.TrueForAll(s => s.SecurityId != ampSecurity.SecurityId)) {
                        securityList.Add(ampSecurity);
                    }
                }

                //Get model from Event Store
                newModel = CommonHelper.Execute(
                    () => ModelEsHelper.GetSimpleModelByLegacyModelId(legacyModelId),
                    TimeSpan.FromMilliseconds(10),
                    300,
                    newModel);

                //Actual data
                if (newModel != null) {
                    ampModel.LegacyModelId = newModel.LegacyModelId ?? -1;
                    ampModel.Name = newModel.Name;
                    ampModel.ModelType = newModel.ModelId.ModelType;

                    var modelSecurityList = new List<ModelSecurity>();
                    foreach (var security in securityList) {
                        if (newModel.LongSecurityTargets.Count == 0) break;

                        if (newModel.LongSecurityTargets.ContainsKey(security.SecurityId)) {
                            var modelSecurity = new ModelSecurity {
                                LegacyId = security.LegacySecurityId,
                                Type = "Long",
                                Weight = newModel.LongSecurityTargets[security.SecurityId].Weight
                            };
                            modelSecurityList.Add(modelSecurity);
                        }
                    }

                    ampModel.LongSecurities = newModel.LongSecurityTargets.Count == 0 ? null : modelSecurityList;
                }

                await tc.Verify(ampModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Something went wrong in test case {tc.TestCaseId}");
                throw;
            }
        }

        [Theory]
        [TestData("DeleteShortSecurities")]
        public async Task DeleteShortSecurities(TestCase<SimpleModelInput, SimpleModelOutput> tc)
        {
            try 
            {
                var newModel = new AmpSimpleModel();
                var ampModel = new SimpleModelOutput();
                var securityList = new List<AmpSecurity>();

                //Create Model in LV
                tc.Input.Name = tc.Expected.Name = Guid.NewGuid().ToString();
                var legacyModelId = ModelLvHelper.CreateModelFromLv(tc.Input.Name, tc.Input.Hierarchy, _currentUserId);
                tc.Expected.LegacyModelId = legacyModelId;
                Thread.Sleep(2000);

                foreach (var security in tc.Input.ShortSecurities) {

                    //Add securities
                    ModelLvHelper.AddSecurityToModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        weight: security.Weight,
                        positionType: security.Type);

                    Thread.Sleep(1000);

                    //Remove Securities
                    ModelLvHelper.RemoveSecurityFromModel(
                        modelId: legacyModelId,
                        hierarchySectorId: tc.Input.Hierarchy,
                        securityId: security.LegacyId,
                        positionType: security.Type,
                        clearOverride: 1);

                    Thread.Sleep(1000);

                    //Get securities from event store
                    var ampSecurity = ModelEsHelper.GetSecurityById(security.LegacyId);

                    if (securityList.TrueForAll(s => s.SecurityId != ampSecurity.SecurityId)) {
                        securityList.Add(ampSecurity);
                    }
                }

                //Get model from Event Store
                newModel = CommonHelper.Execute(
                    () => ModelEsHelper.GetSimpleModelByLegacyModelId(legacyModelId),
                    TimeSpan.FromMilliseconds(10),
                    300,
                    newModel);

                //Actual data
                if (newModel != null) {
                    ampModel.LegacyModelId = newModel.LegacyModelId ?? -1;
                    ampModel.Name = newModel.Name;
                    ampModel.ModelType = newModel.ModelId.ModelType;

                    var modelSecurityList = new List<ModelSecurity>();
                    foreach (var security in securityList) {
                        if (newModel.ShortSecurityTargets.Count == 0) break;

                        if (newModel.ShortSecurityTargets.ContainsKey(security.SecurityId)) {
                            var modelSecurity = new ModelSecurity {
                                LegacyId = security.LegacySecurityId,
                                Type = "Short",
                                Weight = newModel.ShortSecurityTargets[security.SecurityId].Weight
                            };
                            modelSecurityList.Add(modelSecurity);
                        }
                    }

                    ampModel.ShortSecurities = newModel.ShortSecurityTargets.Count == 0 ? null : modelSecurityList;
                }

                await tc.Verify(ampModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Something went wrong in test case {tc.TestCaseId}");
                throw;
            }
        }
    }
}
