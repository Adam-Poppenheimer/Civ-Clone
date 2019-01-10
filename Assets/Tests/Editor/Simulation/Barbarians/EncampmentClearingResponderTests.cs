using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Barbarians {

    public class EncampmentClearingResponderTests : ZenjectUnitTestFixture {

        #region instance fields and properties
        private UnitSignals                                         UnitSignals;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IEncampmentLocationCanon>                      MockEncampmentLocationCanon;
        private Mock<IEncampmentFactory>                            MockEncampmentFactory;
        private Mock<IBarbarianConfig>                              MockBarbarianConfig;
        private Mock<ICivModifiers>                                 MockCivModifiers;

        private Mock<ICivModifier<float>> MockGoldBountyFromEncampments;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            UnitSignals                 = new UnitSignals();
            MockUnitPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();
            MockEncampmentFactory       = new Mock<IEncampmentFactory>();
            MockBarbarianConfig         = new Mock<IBarbarianConfig>();
            MockCivModifiers            = new Mock<ICivModifiers>();

            MockGoldBountyFromEncampments = new Mock<ICivModifier<float>>();

            MockCivModifiers.Setup(modifiers => modifiers.GoldBountyFromEncampments).Returns(MockGoldBountyFromEncampments.Object);

            Container.Bind<UnitSignals>                                  ().FromInstance(UnitSignals);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon    .Object);
            Container.Bind<IEncampmentLocationCanon>                     ().FromInstance(MockEncampmentLocationCanon.Object);
            Container.Bind<IEncampmentFactory>                           ().FromInstance(MockEncampmentFactory      .Object);
            Container.Bind<IBarbarianConfig>                             ().FromInstance(MockBarbarianConfig        .Object);
            Container.Bind<ICivModifiers>                                ().FromInstance(MockCivModifiers           .Object);

            Container.Bind<EncampmentClearingResponder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void OnUnitEnteredLocation_CellHasAnEncampment_AndUnitIsntBarbaric_EncampmentIsDestroyed() {
            var owner      = BuildCiv(BuildCivTemplate(false), 0);            
            var encampment = BuildEncampment();
            var cell       = BuildCell(encampment);
            var unit       = BuildUnit(owner);

            Container.Resolve<EncampmentClearingResponder>();

            UnitSignals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, cell));

            MockEncampmentFactory.Verify(factory => factory.DestroyEncampment(encampment), Times.Once);
        }

        [Test]
        public void OnUnitEnteredLocation_CellHasAnEncampment_AndUnitIsntBarbaric_UnitOwnerGainsGoldBounty() {
            var owner      = BuildCiv(BuildCivTemplate(false), 17);            
            var encampment = BuildEncampment();
            var cell       = BuildCell(encampment);
            var unit       = BuildUnit(owner);

            MockBarbarianConfig.Setup(config => config.EncampmentBounty).Returns(25f);

            MockGoldBountyFromEncampments.Setup(modifier => modifier.GetValueForCiv(owner)).Returns(2f);

            Container.Resolve<EncampmentClearingResponder>();

            UnitSignals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, cell));

            Assert.AreEqual(67, owner.GoldStockpile);
        }

        [Test]
        public void OnUnitEnteredLocation_CellHasAnEncampment_AndUnitIsBarbaric_NothingHappens() {
            var owner      = BuildCiv(BuildCivTemplate(true), 5);            
            var encampment = BuildEncampment();
            var cell       = BuildCell(encampment);
            var unit       = BuildUnit(owner);

            Container.Resolve<EncampmentClearingResponder>();

            UnitSignals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, cell));

            MockEncampmentFactory.Verify(
                factory => factory.DestroyEncampment(encampment),
                Times.Never, "Encampment unexpectedly destroyed"
            );

            Assert.AreEqual(5, owner.GoldStockpile, "owner.GoldStockpile unexpectedly changed");
        }

        [Test]
        public void OnUnitEnteredLocation_CellHasNoEncampment_AndUnitIsntBarbaric_NothingHappens() {
            var owner = BuildCiv(BuildCivTemplate(false), 5);
            var cell  = BuildCell();
            var unit  = BuildUnit(owner);

            Container.Resolve<EncampmentClearingResponder>();

            UnitSignals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(unit, cell));

            MockEncampmentFactory.Verify(
                factory => factory.DestroyEncampment(It.IsAny<IEncampment>()),
                Times.Never, "DestroyEncampment unexpectedly called"
            );

            Assert.AreEqual(5, owner.GoldStockpile, "owner.GoldStockpile unexpectedly changed");
        }

        #endregion

        #region utilities

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template, int goldStockpile) {
            var mockCiv = new Mock<ICivilization>();
            
            mockCiv.SetupAllProperties();
            mockCiv.Setup(civ => civ.Template).Returns(template);

            var newCiv = mockCiv.Object;

            newCiv.GoldStockpile = goldStockpile;

            return newCiv;
        }

        private IEncampment BuildEncampment() {
            return new Mock<IEncampment>().Object;
        }

        private IHexCell BuildCell(params IEncampment[] encampments) {
            var newCell = new Mock<IHexCell>().Object;

            MockEncampmentLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell))
                                       .Returns(encampments);

            return newCell;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
