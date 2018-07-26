using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.MapResources;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingPossessionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IResourceLockingCanon>                         MockResourceLockingCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourceLockingCanon = new Mock<IResourceLockingCanon>();
            MockCityPossessionCanon  = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<IResourceLockingCanon>                        ().FromInstance(MockResourceLockingCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon .Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<BuildingPossessionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a new possession relationship is established, " +
            "BuildingPossessionCanon reserves all required specialty resources " +
            "marked by the building's template. It does this for the civilization " +
            "that now owns the city")]
        public void OnPossessionEstablished_RequiredSpecialtyResourcesReserved() {
            var resourceOne   = BuildResourceDefinition("Resource One");
            var resourceTwo   = BuildResourceDefinition("Resource Two");
            var resourceThree = BuildResourceDefinition("Resource Three");

            var building = BuildBuilding(BuildTemplate(resourceOne, resourceTwo));

            var civilization = BuildCivilization();

            var city = BuildCity(civilization);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceOne, civilization),
                Times.Once, "Resource One was not locked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceTwo, civilization),
                Times.Once, "Resource Two was not locked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.LockCopyOfResourceForCiv(resourceThree, civilization),
                Times.Never, "Resource Three was locked unexpectedly"
            );
        }

        [Test(Description = "When an existing possession relationship is broken, " +
            "BuildingPossessionCanon unreserves all required specialty resource " +
            "marked by the building's template. It does this for the civilization " +
            "that used to own the city")]
        public void OnPossessionBroken_RequiredSpecialtyResourcesUnreserved() {
            var resourceOne   = BuildResourceDefinition("Resource One");
            var resourceTwo   = BuildResourceDefinition("Resource Two");
            var resourceThree = BuildResourceDefinition("Resource Three");

            var building = BuildBuilding(BuildTemplate(resourceOne, resourceTwo));

            var civilization = BuildCivilization();

            var city = BuildCity(civilization);

            var possessionCanon = Container.Resolve<BuildingPossessionCanon>();

            possessionCanon.ChangeOwnerOfPossession(building, city);
            possessionCanon.ChangeOwnerOfPossession(building, null);

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceOne, civilization),
                Times.Once, "Resource One was not unlocked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceTwo, civilization),
                Times.Once, "Resource Two was not unlocked as expected"
            );

            MockResourceLockingCanon.Verify(
                canon => canon.UnlockCopyOfResourceForCiv(resourceThree, civilization),
                Times.Never, "Resource Three was unlocked unexpectedly"
            );
        }

        #endregion

        #region utilities

        private ICity BuildCity(ICivilization owner = null) {
            var newCity = new Mock<ICity>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private IBuildingTemplate BuildTemplate(params IResourceDefinition[] requiredResources) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.ResourcesConsumed).Returns(requiredResources);

            return mockTemplate.Object;
        }

        private IResourceDefinition BuildResourceDefinition(string name = "") {
            var mockDefinition = new Mock<IResourceDefinition>();

            mockDefinition.Name = name;
            mockDefinition.Setup(definition => definition.name).Returns(name);

            return mockDefinition.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
