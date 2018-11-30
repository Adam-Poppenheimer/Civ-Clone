using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CityCombatModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                       MockUnitPositionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;
        private Mock<ICityModifiers>                           MockCityModifiers;

        private Mock<ICityModifier<float>> MockCityGarrisionedModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCityModifiers     = new Mock<ICityModifiers>();

            MockCityGarrisionedModifier = new Mock<ICityModifier<float>>();

            MockCityModifiers.Setup(modifiers => modifiers.GarrisionedRangedCombatStrength)
                             .Returns(MockCityGarrisionedModifier.Object);

            Container.Bind<IUnitPositionCanon>                      ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<ICityModifiers>                          ().FromInstance(MockCityModifiers    .Object);

            Container.Bind<CityCombatModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyCityModifiersToCombat_AppliesBonusesIfAttackerIsGarrisionedCity() {
            var city = BuildCity(0.5f);

            var cell = BuildCell(city);

            var attacker = BuildUnit(UnitType.City, cell);
            var defender = BuildUnit(UnitType.Melee, cell);

            var combatInfo = new CombatInfo() { AttackerCombatModifier = 1f, CombatType = CombatType.Ranged };

            var modifierLogic = Container.Resolve<CityCombatModifierLogic>();

            modifierLogic.ApplyCityModifiersToCombat(attacker, defender, combatInfo);
            
            Assert.AreEqual(1.5f, combatInfo.AttackerCombatModifier);
        }

        [Test]
        public void ApplyCityModifiersToCombat_DoesNotApplyBonusesIfNoCityOnCell() {
            var cell = BuildCell();

            var attacker = BuildUnit(UnitType.City, cell);
            var defender = BuildUnit(UnitType.Melee, cell);

            var combatInfo = new CombatInfo() { AttackerCombatModifier = 1f, CombatType = CombatType.Ranged };

            var modifierLogic = Container.Resolve<CityCombatModifierLogic>();

            modifierLogic.ApplyCityModifiersToCombat(attacker, defender, combatInfo);
            
            Assert.AreEqual(1f, combatInfo.AttackerCombatModifier);
        }

        [Test]
        public void ApplyCityModifiersToCombat_DoesNotApplyBonusesIfAttackerIsntACity() {
            var city = BuildCity(0.5f);

            var cell = BuildCell(city);

            var attacker = BuildUnit(UnitType.Melee, cell);
            var defender = BuildUnit(UnitType.Melee, cell);

            var combatInfo = new CombatInfo() { AttackerCombatModifier = 1f, CombatType = CombatType.Ranged };

            var modifierLogic = Container.Resolve<CityCombatModifierLogic>();

            modifierLogic.ApplyCityModifiersToCombat(attacker, defender, combatInfo);
            
            Assert.AreEqual(1f, combatInfo.AttackerCombatModifier);
        }

        [Test]
        public void ApplyCityModifiersToCombat_DoesNotApplyBonusesIfCombatTypeNotRanged() {
            var city = BuildCity(0.5f);

            var cell = BuildCell(city);

            var attacker = BuildUnit(UnitType.City, cell);
            var defender = BuildUnit(UnitType.Melee, cell);

            var combatInfo = new CombatInfo() { AttackerCombatModifier = 1f, CombatType = CombatType.Melee };

            var modifierLogic = Container.Resolve<CityCombatModifierLogic>();

            modifierLogic.ApplyCityModifiersToCombat(attacker, defender, combatInfo);
            
            Assert.AreEqual(1f, combatInfo.AttackerCombatModifier);
        }

        #endregion

        #region utilities

        private ICity BuildCity(float garrionedCombatStrength) {
            var newCity = new Mock<ICity>().Object;

            MockCityGarrisionedModifier.Setup(modifier => modifier.GetValueForCity(newCity))
                                       .Returns(garrionedCombatStrength);

            return newCity;
        }

        private IHexCell BuildCell(params ICity[] cities) {
            var newCell = new Mock<IHexCell>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(cities);

            return newCell;
        }

        private IUnit BuildUnit(UnitType type, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
