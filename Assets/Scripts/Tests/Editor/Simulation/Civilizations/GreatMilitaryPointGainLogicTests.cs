using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Civilizations {

    public class GreatMilitaryPointGainLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IGreatPersonCanon>                             MockGreatPersonCanon;
        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivModifiers>                                 MockCivModifiers;
        private UnitSignals                                         UnitSignals;

        private Mock<ICivModifier<float>> MockGreatMilitaryGainSpeed;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGreatPersonCanon    = new Mock<IGreatPersonCanon>();
            MockCivConfig           = new Mock<ICivilizationConfig>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivModifiers        = new Mock<ICivModifiers>();
            UnitSignals             = new UnitSignals();

            MockGreatMilitaryGainSpeed = new Mock<ICivModifier<float>>();

            MockCivModifiers.Setup(modifiers => modifiers.GreatMilitaryGainSpeed).Returns(MockGreatMilitaryGainSpeed.Object);

            Container.Bind<IGreatPersonCanon>                            ().FromInstance(MockGreatPersonCanon   .Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig          .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ICivModifiers>                                ().FromInstance(MockCivModifiers       .Object);
            Container.Bind<UnitSignals>                                  ().FromInstance(UnitSignals);

            Container.Bind<GreatMilitaryPointGainLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void UnitGainedExperienceFired_OnNavalMeleeUnit_AddsGreatAdmiralPointsBasedOnExperience() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.NavalMelee, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ, 15 * 3f));
        }

        [Test]
        public void UnitGainedExperienceFired_OnNavalRangedUnit_AddsGreatAdmiralPointsBasedOnExperience() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.NavalRanged, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ, 15 * 3f));
        }

        [Test]
        public void UnitGainedExperienceFired_OnMeleeUnit_AddsGreatGeneralPointsBasedOnExperience() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.Melee, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral, civ, 15 * 3f));
        }

        [Test]
        public void UnitGainedExperienceFired_OnArcheryUnit_AddsGreatGeneralPointsBasedOnExperience() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.Archery, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatGeneral, civ, 15 * 3f));
        }

        [Test]
        public void UnitGainedExperienceFired_OnCityUnit_NoGreatPersonPointsAdded() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.City, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(
                    It.IsAny<GreatPersonType>(), It.IsAny<ICivilization>(), It.IsAny<float>()
                ), Times.Never
            );
        }

        [Test]
        public void UnitGainedExperienceFired_OnCivilianUnit_NoGreatPersonPointsAdded() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.Civilian, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(
                    It.IsAny<GreatPersonType>(), It.IsAny<ICivilization>(), It.IsAny<float>()
                ), Times.Never
            );
        }

        [Test]
        public void UnitGainedExperienceFired_AndTrackPointGainIsFalse_GreatPersonPointsNotAdded() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.NavalMelee, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(1f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = false;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ, 15 * 3f),
                Times.Never
            );
        }

        [Test]
        public void UnitGainedExperienceFired_ModifiedByGreatPersonGenerationModifier() {
            MockCivConfig.Setup(config => config.ExperienceToGreatPersonPointRatio).Returns(3f);

            var civ = BuildCiv();

            var unit = BuildUnit(UnitType.NavalMelee, civ);

            MockGreatMilitaryGainSpeed.Setup(modifier => modifier.GetValueForCiv(civ)).Returns(3f);

            var pointGainLogic = Container.Resolve<GreatMilitaryPointGainLogic>();

            pointGainLogic.IsActive = true;

            UnitSignals.GainedExperience.OnNext(new Tuple<IUnit, int>(unit, 15));

            MockGreatPersonCanon.Verify(canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ, 15 * 3f * 3f));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IUnit BuildUnit(UnitType type, ICivilization owner) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
