using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Units {

    public class FreeGreatPeopleCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private CivilizationSignals                                 CivSignals;
        private CitySignals                                         CitySignals;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            CivSignals              = new CivilizationSignals();
            CitySignals             = new CitySignals();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);
            Container.Bind<CitySignals>                                  ().FromInstance(CitySignals);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<FreeGreatPeopleCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetFreeGreatPeopleForCiv_DefaultsToZero() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void SetFreeGreatPeopleForCiv_OverridesFreeGreatPeopleForCiv() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.SetFreeGreatPeopleForCiv(civ, 7);

            Assert.AreEqual(7, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void AddFreeGreatPersonToCiv_IncreasesFreeGreatPeopleForCivByOne() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);

            Assert.AreEqual(3, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void RemoveFreeGreatPersonToCiv_DecreasesFreeGreatPeopleForCivByOne() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.RemoveFreeGreatPersonFromCiv(civ);
            peopleCanon.RemoveFreeGreatPersonFromCiv(civ);
            peopleCanon.RemoveFreeGreatPersonFromCiv(civ);

            Assert.AreEqual(-3, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void ClearCiv_ResetsFreeGreatPeopleOfCivToZero() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);

            peopleCanon.ClearCiv(civ);

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void Clear_ResetsFreeGreatPeopleOfAllCivsToZero() {
            var civOne   = BuildCiv();
            var civTwo   = BuildCiv();
            var civThree = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.AddFreeGreatPersonToCiv(civOne);
            peopleCanon.AddFreeGreatPersonToCiv(civOne);
            peopleCanon.AddFreeGreatPersonToCiv(civTwo);
            peopleCanon.AddFreeGreatPersonToCiv(civTwo);
            peopleCanon.AddFreeGreatPersonToCiv(civThree);
            peopleCanon.AddFreeGreatPersonToCiv(civThree);

            peopleCanon.Clear();

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civOne),   "CivOne not cleared as expected");
            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civTwo),   "CivTwo not cleared as expected");
            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civThree), "CivThree not cleared as expected");
        }

        [Test]
        public void OnCivBeingDestroyed_AndIsActiveTrue_ClearsCiv() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);

            peopleCanon.IsActive = true;

            CivSignals.CivBeingDestroyed.OnNext(civ);

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivBeingDestroyed_AndIsActiveFalse_CivRemainsUncleared() {
            var civ = BuildCiv();

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);
            peopleCanon.AddFreeGreatPersonToCiv(civ);

            peopleCanon.IsActive = false;

            CivSignals.CivBeingDestroyed.OnNext(civ);

            Assert.AreEqual(3, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivDiscoveredTech_AndIsActiveTrue_AddsFreeGreatPeopleFromTech() {
            var civ = BuildCiv();

            var tech = BuildTech(5);

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = true;

            CivSignals.CivDiscoveredTech.OnNext(new UniRx.Tuple<ICivilization, ITechDefinition>(civ, tech));

            Assert.AreEqual(5, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivDiscoveredTech_AndIsActiveFalse_DoesNotAddFreeGreatPeople() {
            var civ = BuildCiv();

            var tech = BuildTech(5);

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = false;

            CivSignals.CivDiscoveredTech.OnNext(
                new UniRx.Tuple<ICivilization, ITechDefinition>(civ, tech)
            );

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivUnlockedPolicy_AndIsActiveTrue_AddsFreeGreatPeopleFromPolicyBonuses() {
            var civ = BuildCiv();

            var policy = BuildPolicy(BuildPolicyBonuses(3));

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = true;

            CivSignals.CivUnlockedPolicy.OnNext(
                new UniRx.Tuple<ICivilization, ISocialPolicyDefinition>(civ, policy)
            );

            Assert.AreEqual(3, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivUnlockedPolicy_AndIsActiveFalse_DoesNotAddFreeGreatPeople() {
            var civ = BuildCiv();

            var policy = BuildPolicy(BuildPolicyBonuses(3));

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = false;

            CivSignals.CivUnlockedPolicy.OnNext(
                new UniRx.Tuple<ICivilization, ISocialPolicyDefinition>(civ, policy)
            );

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivUnlockedPolicyTree_AndIsActiveTrue_AddsFreeGreatPeopleFromUnlockingBonuses() {
            var civ = BuildCiv();

            var policyTree = BuildPolicyTree(
                unlockingBonuses: BuildPolicyBonuses(5), completionBonuses: BuildPolicyBonuses(3)
            );

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = true;

            CivSignals.CivUnlockedPolicyTree.OnNext(
                new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, policyTree)
            );

            Assert.AreEqual(5, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivUnlockedPolicyTree_AndIsActiveFalse_DoesNotAddFreeGreatPeople() {
            var civ = BuildCiv();

            var policyTree = BuildPolicyTree(
                unlockingBonuses: BuildPolicyBonuses(5), completionBonuses: BuildPolicyBonuses(3)
            );

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = false;

            CivSignals.CivUnlockedPolicyTree.OnNext(
                new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, policyTree)
            );

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivFinishedPolicyTree_AndIsActiveTrue_AddsFreeGreatPeopleFromCompletionBonuses() {
            var civ = BuildCiv();

            var policyTree = BuildPolicyTree(
                unlockingBonuses: BuildPolicyBonuses(5), completionBonuses: BuildPolicyBonuses(3)
            );

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = true;

            CivSignals.CivFinishedPolicyTree.OnNext(
                new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, policyTree)
            );

            Assert.AreEqual(3, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCivFinishedPolicyTree_AndIsActiveFalse_DoesNotAddFreeGreatPeople() {
            var civ = BuildCiv();

            var policyTree = BuildPolicyTree(
                unlockingBonuses: BuildPolicyBonuses(5), completionBonuses: BuildPolicyBonuses(3)
            );

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = false;

            CivSignals.CivFinishedPolicyTree.OnNext(
                new UniRx.Tuple<ICivilization, IPolicyTreeDefinition>(civ, policyTree)
            );

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCityGainedBuilding_AndIsActiveTrue_AddsFreeGreatPeopleFromTemplateToCityOwner() {
            var civ = BuildCiv();

            var city = BuildCity(civ);

            var building = BuildBuilding(BuildBuildingTemplate(5));

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = true;
            
            CitySignals.GainedBuilding.OnNext(
                new UniRx.Tuple<ICity, IBuilding>(city, building)
            );

            Assert.AreEqual(5, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        [Test]
        public void OnCityGainedBuilding_AndIsActiveFlase_DoesNotAddFreeGreatPeopleFromTemplateToCityOwner() {
            var civ = BuildCiv();

            var city = BuildCity(civ);

            var building = BuildBuilding(BuildBuildingTemplate(5));

            var peopleCanon = Container.Resolve<FreeGreatPeopleCanon>();

            peopleCanon.IsActive = false;
            
            CitySignals.GainedBuilding.OnNext(
                new UniRx.Tuple<ICity, IBuilding>(city, building)
            );

            Assert.AreEqual(0, peopleCanon.GetFreeGreatPeopleForCiv(civ));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ISocialPolicyBonusesData BuildPolicyBonuses(int freeGreatPeople) {
            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonuses => bonuses.FreeGreatPeople).Returns(freeGreatPeople);

            return mockBonuses.Object;
        }

        private ISocialPolicyDefinition BuildPolicy(ISocialPolicyBonusesData bonuses) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            mockPolicy.Setup(policy => policy.Bonuses).Returns(bonuses);

            return mockPolicy.Object;
        }

        private IPolicyTreeDefinition BuildPolicyTree(
            ISocialPolicyBonusesData unlockingBonuses, ISocialPolicyBonusesData completionBonuses
        ) {
            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Setup(tree => tree.UnlockingBonuses) .Returns(unlockingBonuses);
            mockTree.Setup(tree => tree.CompletionBonuses).Returns(completionBonuses);

            return mockTree.Object;
        }

        private ITechDefinition BuildTech(int freeGreatPeopleProvided) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech.Setup(tech => tech.FreeGreatPeopleProvided).Returns(freeGreatPeopleProvided);

            return mockTech.Object;
        }

        private ICity BuildCity(ICivilization owner) {
            var newCity = new Mock<ICity>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private IBuildingTemplate BuildBuildingTemplate(int freeGreatPeople) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.FreeGreatPeople).Returns(freeGreatPeople);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
