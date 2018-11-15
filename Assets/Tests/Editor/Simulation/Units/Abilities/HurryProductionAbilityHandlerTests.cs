using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.HexMap;


namespace Assets.Tests.Simulation.Units.Abilities {

    public class HurryProductionAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<ICityConfig>                                   MockCityConfig;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCityConfig          = new Mock<ICityConfig>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<ICityConfig>                                  ().FromInstance(MockCityConfig         .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<HurryProductionAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleAbilityOnUnit_TrueIfLocationHasDomesticCityWithAnActiveProject() {
            var location = BuildCell();
            var civ      = BuildCiv();

            BuildCity(location, civ, BuildProject(0), 0);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, civ);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.IsTrue(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfAbilityHasNoAppropriateCommandRequest() {
            var location = BuildCell();
            var civ      = BuildCiv();

            BuildCity(location, civ, BuildProject(0), 0);

            var ability = BuildAbility(
                new AbilityCommandRequest() { CommandType = AbilityCommandType.SetUpToBombard },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement }
            );

            var unit = BuildUnit(location, civ);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfNoCityAtLocation() {
            var location = BuildCell();
            var civ      = BuildCiv();

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, civ);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfCityNotDomesticToUnit() {
            var location    = BuildCell();
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            BuildCity(location, foreignCiv, BuildProject(0), 0);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, domesticCiv);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanHandleAbilityOnUnit_FalseIfCityHasNoActiveProject() {
            var location = BuildCell();
            var civ      = BuildCiv();

            BuildCity(location, civ, null, 0);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, civ);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleAbilityOnUnit(ability, unit));
        }

        [Test]
        public void TryHandleAbilityOnUnit_AndAbilityValid_CalculatesAddedProgressFromConfiguration_AndCityPopulation() {
            var location = BuildCell();
            var civ      = BuildCiv();

            var project = BuildProject(15);

            BuildCity(location, civ, project, 10);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, civ);

            MockCityConfig.Setup(config => config.HurryAbilityBaseProduction)  .Returns(300);
            MockCityConfig.Setup(config => config.HurryAbilityPerPopProduction).Returns(55);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            abilityHandler.TryHandleAbilityOnUnit(ability, unit);

            Assert.AreEqual(865, project.Progress);
        }

        [Test]
        public void TryHandleAbilityOnUnit_AndAbilityValid_ReturnsCorrectExecutionResults() {
            var location = BuildCell();
            var civ      = BuildCiv();

            var project = BuildProject(15);

            BuildCity(location, civ, project, 10);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.HurryProduction });
            var unit    = BuildUnit(location, civ);

            MockCityConfig.Setup(config => config.HurryAbilityBaseProduction)  .Returns(300);
            MockCityConfig.Setup(config => config.HurryAbilityPerPopProduction).Returns(55);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.AreEqual(
                new AbilityExecutionResults(true, null),
                abilityHandler.TryHandleAbilityOnUnit(ability, unit)
            );
        }

        [Test]
        public void TryHandleAbilityOnUnit_AndAbilityInvalid_DoesntModifyProjectProgress() {
            var location = BuildCell();
            var civ      = BuildCiv();

            var project = BuildProject(15);

            BuildCity(location, civ, project, 10);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.ClearVegetation });
            var unit    = BuildUnit(location, civ);

            MockCityConfig.Setup(config => config.HurryAbilityBaseProduction)  .Returns(300);
            MockCityConfig.Setup(config => config.HurryAbilityPerPopProduction).Returns(55);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            abilityHandler.TryHandleAbilityOnUnit(ability, unit);

            Assert.AreEqual(15, project.Progress);
        }

        [Test]
        public void TryHandleAbilityOnUnit_AndAbilityInvalid_ReturnsCorrectExecutionResults() {
            var location = BuildCell();
            var civ      = BuildCiv();

            var project = BuildProject(15);

            BuildCity(location, civ, project, 10);

            var ability = BuildAbility(new AbilityCommandRequest() { CommandType = AbilityCommandType.ClearVegetation });
            var unit    = BuildUnit(location, civ);

            MockCityConfig.Setup(config => config.HurryAbilityBaseProduction)  .Returns(300);
            MockCityConfig.Setup(config => config.HurryAbilityPerPopProduction).Returns(55);

            var abilityHandler = Container.Resolve<HurryProductionAbilityHandler>();

            Assert.AreEqual(
                new AbilityExecutionResults(false, null),
                abilityHandler.TryHandleAbilityOnUnit(ability, unit)
            );
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(IHexCell location, ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICity BuildCity(
            IHexCell location, ICivilization owner, IProductionProject activeProject, int population
        ) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.ActiveProject).Returns(activeProject);
            mockCity.Setup(city => city.Population)   .Returns(population);

            var newCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity });

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private IProductionProject BuildProject(int progress) {
            var mockProject = new Mock<IProductionProject>();

            mockProject.SetupAllProperties();

            var newProject = mockProject.Object;

            newProject.Progress = progress;

            return newProject;
        }

        private IAbilityDefinition BuildAbility(params AbilityCommandRequest[] commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
