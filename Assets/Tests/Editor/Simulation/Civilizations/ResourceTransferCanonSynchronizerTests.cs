using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Civilizations {

    public class ResourceTransferCanonSynchronizerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceTransferCanon>                           MockResourceTransferCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockResourceNodeLocationCanon;
        private Mock<ICivilizationTerritoryLogic>                      MockCivTerritoryLogic;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCityTerritoryCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;

        ImprovementSignals       ImprovementSignals;
        ResourceSignals ResourceSignals;
        CivilizationSignals      CivSignals;

        CitySignals CitySignals {
            get { return Container.Resolve<CitySignals>(); }
        }

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourceTransferCanon     = new Mock<IResourceTransferCanon>();
            MockImprovementLocationCanon  = new Mock<IImprovementLocationCanon>();
            MockResourceNodeLocationCanon = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockCivTerritoryLogic         = new Mock<ICivilizationTerritoryLogic>();
            MockCityTerritoryCanon        = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockCityPossessionCanon       = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            ImprovementSignals = new ImprovementSignals();
            ResourceSignals    = new ResourceSignals();
            CivSignals         = new CivilizationSignals();

            Container.Bind<IResourceTransferCanon>                          ().FromInstance(MockResourceTransferCanon    .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockResourceNodeLocationCanon.Object);
            Container.Bind<ICivilizationTerritoryLogic>                     ().FromInstance(MockCivTerritoryLogic        .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCityTerritoryCanon       .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon      .Object);

            Container.Bind<ImprovementSignals> ().FromInstance(ImprovementSignals);
            Container.Bind<ResourceSignals>    ().FromInstance(ResourceSignals);
            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<ResourceTransferCanonSynchronizer>().AsSingle().NonLazy();
        }

        #endregion

        #region tests

        [Test]
        public void ImprovementBeingRemovedFromLocation_SynchronizeCalledProperly() {
            var civ = BuildCiv();
            var resource = BuildResource();

            var location = BuildHexCell(civ);            

            var improvement = BuildImprovement(location);
            BuildNode(location, resource);

            Container.Resolve<ResourceTransferCanonSynchronizer>();

            ImprovementSignals.ImprovementRemovedFromLocationSignal.OnNext(
                new Tuple<IImprovement, IHexCell>(improvement, location)
            );

            MockResourceTransferCanon.Verify(
                canon => canon.SynchronizeResourceForCiv(resource, civ), Times.Once,
                "SynchronizeResourceForCiv was not called as expected"
            );
        }

        [Test]
        public void ImprovementBeingPillaged_SynchronizeCalledProperly() {
            var civ = BuildCiv();
            var resource = BuildResource();

            var location = BuildHexCell(civ);            

            var improvement = BuildImprovement(location);
            BuildNode(location, resource);

            Container.Resolve<ResourceTransferCanonSynchronizer>();

            ImprovementSignals.ImprovementPillagedSignal.OnNext(improvement);

            MockResourceTransferCanon.Verify(
                canon => canon.SynchronizeResourceForCiv(resource, civ), Times.Once,
                "SynchronizeResourceForCiv was not called as expected"
            );
        }

        [Test]
        public void ResourceNodeBeingRemovedFromLocation_SynchronizeCalledPropertly() {
            var civ = BuildCiv();
            var resource = BuildResource();

            var location = BuildHexCell(civ);            

            BuildImprovement(location);
            var node = BuildNode(location, resource);

            Container.Resolve<ResourceTransferCanonSynchronizer>();

            ResourceSignals.ResourceNodeRemovedFromLocationSignal.OnNext(
                new Tuple<IResourceNode, IHexCell>(node, location)
            );

            MockResourceTransferCanon.Verify(
                canon => canon.SynchronizeResourceForCiv(resource, civ), Times.Once,
                "SynchronizeResourceForCiv was not called as expected"
            );
        }

        [Test]
        public void CityLostCellFromBoundaries_SynchronizeCalledProperly() {
            var civ = BuildCiv();
            var resource = BuildResource();

            var location = BuildHexCell(civ);            

            BuildImprovement(location);
            BuildNode(location, resource);

            var city = BuildCity(new List<IHexCell>(), civ);

            Container.Resolve<ResourceTransferCanonSynchronizer>();

            CitySignals.LostCellFromBoundariesSignal.OnNext(
                new Tuple<ICity, IHexCell>(city, location)
            );

            MockResourceTransferCanon.Verify(
                canon => canon.SynchronizeResourceForCiv(resource, civ), Times.Once,
                "SynchronizeResourceForCiv was not called as expected"
            );
        }

        [Test]
        public void CivLosingCity_SynchronizeCalledProperly() {
            var civ = BuildCiv();

            var cityTerritory = new List<IHexCell>();
            var resources = new List<IResourceDefinition>();

            for(int i = 0; i < 5; i++) {
                var resource = BuildResource("Resource " + i.ToString());

                var location = BuildHexCell(civ);            

                BuildImprovement(location);
                BuildNode(location, resource);

                cityTerritory.Add(location);
            }

            var city = BuildCity(cityTerritory, civ);

            Container.Resolve<ResourceTransferCanonSynchronizer>();

            CivSignals.CivLosingCitySignal.OnNext(new Tuple<ICivilization, ICity>(civ, city));

            foreach(var resource in resources) {
                MockResourceTransferCanon.Verify(
                    canon => canon.SynchronizeResourceForCiv(resource, civ), Times.Once,
                    string.Format("SynchronizeResourceForCiv was not called as expected on resource {0}", resource)
                );
            }
        }

        #endregion

        #region utilities

        private ICity BuildCity(List<IHexCell> territory, ICivilization owner) {
            var newCity = new Mock<ICity>().Object;

            MockCityTerritoryCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(territory);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IResourceDefinition BuildResource(string name = "") {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Name = name;

            return mockResource.Object;
        }

        private IHexCell BuildHexCell(ICivilization owner) {
            var newCell = new Mock<IHexCell>().Object;

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell)).Returns(owner);

            return newCell;
        }

        private IImprovement BuildImprovement(IHexCell location) {
            var newImprovement = new Mock<IImprovement>().Object;

            MockImprovementLocationCanon
                .Setup(canon => canon.GetOwnerOfPossession(newImprovement))
                .Returns(location);

            return newImprovement;
        }

        private IResourceNode BuildNode(IHexCell location, IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();
            
            mockNode.Setup(node => node.Resource).Returns(resource);
            
            var newNode = mockNode.Object;

            MockResourceNodeLocationCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        #endregion

        #endregion

    }

}
