using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    public class GreatPersonFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICapitalCityCanon>                        MockCapitalCityCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<IUnitFactory>                             MockUnitFactory;
        private Mock<IUnitConfig>                              MockUnitConfig;
        private Mock<IHexGrid>                                 MockGrid;
        private CivilizationSignals                            CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCapitalCityCanon  = new Mock<ICapitalCityCanon>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockUnitFactory       = new Mock<IUnitFactory>();
            MockUnitConfig        = new Mock<IUnitConfig>();
            MockGrid              = new Mock<IHexGrid>();

            CivSignals = new CivilizationSignals();

            Container.Bind<ICapitalCityCanon>                       ().FromInstance(MockCapitalCityCanon .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IUnitFactory>                            ().FromInstance(MockUnitFactory      .Object);
            Container.Bind<IUnitConfig>                             ().FromInstance(MockUnitConfig       .Object);
            Container.Bind<IHexGrid>                                ().FromInstance(MockGrid             .Object);
            Container.Bind<CivilizationSignals>                     ().FromInstance(CivSignals);

            Container.Bind<GreatPersonFactory>().AsSingle();
        }

        #endregion

        #region tests 

        [Test]
        public void CanBuildGreatPerson_ReturnsTrueIfCivHasCapital() {
            var capitalLocation = BuildCell(true);

            var civ = BuildCiv(BuildCity(capitalLocation));

            var factory = Container.Resolve<GreatPersonFactory>();

            Assert.IsTrue(factory.CanBuildGreatPerson(GreatPersonType.GreatEngineer, civ));
        }

        [Test]
        public void CanBuildGreatPerson_ReturnsTrueIfCivHasNoCapital() {
            var civ = BuildCiv(null);

            var factory = Container.Resolve<GreatPersonFactory>();

            Assert.IsFalse(factory.CanBuildGreatPerson(GreatPersonType.GreatEngineer, civ));
        }

        [Test]
        public void BuildGreatPerson_BuildsUnitOfCorrectTemplateAtCapital() {
            var capitalLocation = BuildCell(true);

            var civ = BuildCiv(BuildCity(capitalLocation));

            var engineerTemplate = BuildUnitTemplate();

            MockUnitConfig.Setup(config => config.GetTemplateForGreatPersonType(GreatPersonType.GreatEngineer)).Returns(engineerTemplate);

            var factory = Container.Resolve<GreatPersonFactory>();

            factory.BuildGreatPerson(GreatPersonType.GreatEngineer, civ);
            
            MockUnitFactory.Verify(unitFactory => unitFactory.BuildUnit(capitalLocation, engineerTemplate, civ), Times.Once);
        }

        [Test]
        public void BuildGreatPerson_BuildsUnitAtCellClosestToCapitalIfCapitalCannotReceiveUnit() {
            var capitalLocation = BuildCell(false);

            var cellOneAway   = BuildCell(false);
            var cellTwoAway   = BuildCell(true);
            var cellThreeAway = BuildCell(true);

            MockGrid.Setup(grid => grid.GetCellsInRing(capitalLocation, 1)).Returns(new List<IHexCell>() { cellOneAway });
            MockGrid.Setup(grid => grid.GetCellsInRing(capitalLocation, 2)).Returns(new List<IHexCell>() { cellTwoAway });
            MockGrid.Setup(grid => grid.GetCellsInRing(capitalLocation, 3)).Returns(new List<IHexCell>() { cellThreeAway });

            var civ = BuildCiv(BuildCity(capitalLocation));

            var engineerTemplate = BuildUnitTemplate();

            MockUnitConfig.Setup(config => config.GetTemplateForGreatPersonType(GreatPersonType.GreatEngineer)).Returns(engineerTemplate);

            var factory = Container.Resolve<GreatPersonFactory>();

            factory.BuildGreatPerson(GreatPersonType.GreatEngineer, civ);
            
            MockUnitFactory.Verify(
                unitFactory => unitFactory.BuildUnit(capitalLocation, engineerTemplate, civ),
                Times.Never, "Unexpectedly attempted to create unit at capitalLocation"
            );

            MockUnitFactory.Verify(
                unitFactory => unitFactory.BuildUnit(cellOneAway, engineerTemplate, civ),
                Times.Never, "Unexpectedly attempted to create unit at cellOneAway"
            );

            MockUnitFactory.Verify(
                unitFactory => unitFactory.BuildUnit(cellTwoAway, engineerTemplate, civ),
                Times.Once, "Failed to create unit at cellTwoAway"
            );

            MockUnitFactory.Verify(
                unitFactory => unitFactory.BuildUnit(cellThreeAway, engineerTemplate, civ),
                Times.Never, "Unexpectedly attempted to create unit at cellThreeAway"
            );
        }

        [Test]
        public void BuildGreatPerson_FiresGreatPersonBornSignals() {
            var capitalLocation = BuildCell(true);

            var civ = BuildCiv(BuildCity(capitalLocation));

            var engineerTemplate = BuildUnitTemplate();

            MockUnitConfig.Setup(config => config.GetTemplateForGreatPersonType(GreatPersonType.GreatEngineer)).Returns(engineerTemplate);

            var factory = Container.Resolve<GreatPersonFactory>();

            GreatPersonBirthData? birthData = null;
            CivSignals.GreatPersonBorn.Subscribe(data => birthData = data);

            var greatPerson = factory.BuildGreatPerson(GreatPersonType.GreatEngineer, civ);

            Assert.IsTrue(birthData.HasValue, "GreatPersonBornSignal was never fired");

            Assert.AreEqual(
                GreatPersonType.GreatEngineer, birthData.Value.Type,
                "GreatPersonBornSignal was given the wrong Type"
            );

            Assert.AreEqual(
                civ, birthData.Value.Owner,
                "GreatPersonBornSignal was given the wrong Owner"
            );

            Assert.AreEqual(
                greatPerson, birthData.Value.GreatPerson,
                "GreatPersonBornSignal was given the wrong GreatPerson"
            );
        }

        [Test]
        public void BuildGreatPerson_ThrowsInvalidOperationExceptionIfConstructionInvalid() {
            var civ = BuildCiv(null);

            var factory = Container.Resolve<GreatPersonFactory>();

            Assert.Throws<InvalidOperationException>(() => factory.BuildGreatPerson(GreatPersonType.GreatEngineer, civ));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(ICity capital) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            return newCiv;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        private IHexCell BuildCell(bool isValidLocation) {
            var newCell = new Mock<IHexCell>().Object;

            MockGrid.Setup(grid => grid.GetCellsInRing(newCell, 0)).Returns(new List<IHexCell>() { newCell });

            MockUnitFactory.Setup(
                factory => factory.CanBuildUnit(newCell, It.IsAny<IUnitTemplate>(), It.IsAny<ICivilization>())
            ).Returns(isValidLocation);

            return newCell;
        }

        private IUnitTemplate BuildUnitTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        #endregion

        #endregion

    }

}
