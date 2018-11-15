using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationYieldLogic> MockYieldLogic;
        private Mock<ITechCanon>              MockTechCanon;
        private Mock<IGreatPersonCanon>       MockGreatPersonCanon;
        private Mock<IGreatPersonFactory>     MockGreatPersonFactory;
        private CivilizationSignals           CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockYieldLogic         = new Mock<ICivilizationYieldLogic>();
            MockTechCanon          = new Mock<ITechCanon>();
            MockGreatPersonCanon   = new Mock<IGreatPersonCanon>();
            MockGreatPersonFactory = new Mock<IGreatPersonFactory>();
            CivSignals             = new CivilizationSignals();

            Container.Bind<ICivilizationYieldLogic>().FromInstance(MockYieldLogic        .Object);
            Container.Bind<ITechCanon>             ().FromInstance(MockTechCanon         .Object);
            Container.Bind<IGreatPersonCanon>      ().FromInstance(MockGreatPersonCanon  .Object);
            Container.Bind<IGreatPersonFactory>    ().FromInstance(MockGreatPersonFactory.Object);
            Container.Bind<CivilizationSignals>    ().FromInstance(CivSignals);
        }

        #endregion

        #region tests

        [Test(Description = "When PerformIncome is called, Civilization should check " + 
            "CivilizationYieldLogic.GetYieldOfCivilization to inform its stockpile changes " +
            "and LastScienceYield")]
        public void PerformIncome_StockpilesAndYieldsChanged() {
            var civilization = Container.InstantiateComponentOnNewGameObject<Civilization>();

            MockYieldLogic.Setup(logic => logic.GetYieldOfCivilization(civilization))
                .Returns(new YieldSummary(gold: 2, production: 4, culture: 5, science: 10));

            civilization.PerformIncome();

            Assert.AreEqual(2,  civilization.GoldStockpile,    "Civilization has an unexpected GoldStockpile");
            Assert.AreEqual(5,  civilization.CultureStockpile, "Civilization has an unexpected CultureStockpile");
            Assert.AreEqual(10, civilization.LastScienceYield, "Civilization has an unexpected LastScienceYield");
        }

        [Test]
        public void PerformIncome_GreatPersonYieldsAddedToGreatPersonCanon() {
            var civ = Container.InstantiateComponentOnNewGameObject<Civilization>();

            MockYieldLogic.Setup(logic => logic.GetYieldOfCivilization(civ))
                .Returns(new YieldSummary(greatArtist: 5f, greatEngineer: 2f, greatMerchant: -1f, greatScientist: 0f));

            civ.PerformIncome();

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatArtist, civ, 5f),
                Times.Once, "Points not added to GreatArtist as expected"
            );

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ, 2f),
                Times.Once, "Points not added to GreatEngineer as expected"
            );

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatMerchant, civ, -1f),
                Times.Once, "Points not added to GreatMerchant as expected"
            );

            MockGreatPersonCanon.Verify(
                canon => canon.AddPointsTowardsTypeForCiv(GreatPersonType.GreatScientist, civ, 0f),
                Times.Once, "Points not added to GreatScientist as expected"
            );
        }

        [Test(Description = "When PerformResearch is called, if there are any techs " +
            "in the civilization's TechQueue, the civilization will modify its progress " +
            "in that tech by its LastScienceYield")]
        public void PerformResearch_ChangesProgressOnActiveTech() {
            var technology = BuildTech(20);

            var civilization = Container.InstantiateComponentOnNewGameObject<Civilization>();

            civilization.LastScienceYield = 10;

            civilization.TechQueue.Enqueue(technology);

            civilization.PerformResearch();

            MockTechCanon.Verify(
                canon => canon.GetProgressOnTechByCiv(technology, civilization),
                Times.Once,
                "Civilization did not tech TechCanon for the progress of its active tech"
            );

            MockTechCanon.Verify(
                canon => canon.SetProgressOnTechByCiv(technology, civilization, 10),
                Times.Once,
                "Civilization did not set its active tech's progress correctly"
            );
        }

        [Test(Description = "When PerformResearch is called and the civilization has " +
            "made enough progress to research its active technology, it does so, " +
            "removing that tech from its TechQueue")]
        public void PerformResearch_SpendsScienceToResearchTechs() {
            var technology = BuildTech(20);

            var civilization = Container.InstantiateComponentOnNewGameObject<Civilization>();

            civilization.LastScienceYield = 20;

            civilization.TechQueue.Enqueue(technology);

            civilization.PerformResearch();

            MockTechCanon.Verify(
                canon => canon.IsTechAvailableToCiv(technology, civilization),
                Times.AtLeastOnce,
                "Civilization did not check to make sure its active tech was available"
            );

            MockTechCanon.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(technology, civilization),
                Times.AtLeastOnce,
                "Civilization did not set its finished tech as discovered"
            );

            CollectionAssert.DoesNotContain(civilization.TechQueue, technology,
                "TechQueue still contains the discovered tech");
        }

        [Test]
        public void PerformGreatPeopleGeneration_MakesPeopleFromTypesWhereProgressExceedsPointsNeeded() {
            var civ = Container.InstantiateComponentOnNewGameObject<Civilization>();

            MockGreatPersonCanon.Setup(canon => canon.GetPointsNeededForTypeForCiv(It.IsAny<GreatPersonType>(), civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ))
                                .Returns(110f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist, civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ))
                                .Returns(90f);

            civ.PerformGreatPeopleGeneration();

            MockGreatPersonFactory.Verify(
                factory => factory.BuildGreatPerson(GreatPersonType.GreatAdmiral, civ), Times.Once,
                "GreatAdmiral not built as expected"
            );

            MockGreatPersonFactory.Verify(
                factory => factory.BuildGreatPerson(GreatPersonType.GreatArtist, civ), Times.Once,
                "GreatArtist not built as expected"
            );
        }

        [Test]
        public void PerformGreatPeopleGeneration_DoesNothingWithTypesWhereProgressLessThanPointsNeeded() {
            var civ = Container.InstantiateComponentOnNewGameObject<Civilization>();

            MockGreatPersonCanon.Setup(canon => canon.GetPointsNeededForTypeForCiv(It.IsAny<GreatPersonType>(), civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ))
                                .Returns(110f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist, civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ))
                                .Returns(90f);

            civ.PerformGreatPeopleGeneration();

            MockGreatPersonFactory.Verify(
                factory => factory.BuildGreatPerson(GreatPersonType.GreatEngineer, civ), Times.Never,
                "GreatEngineer unexpectedly built"
            );
        }

        [Test]
        public void PerformGreatPeopleGeneration_SubtractsPointsWhenCreatingGreatPerson() {
            var civ = Container.InstantiateComponentOnNewGameObject<Civilization>();

            MockGreatPersonCanon.Setup(canon => canon.GetPointsNeededForTypeForCiv(It.IsAny<GreatPersonType>(), civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ))
                                .Returns(110f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist, civ))
                                .Returns(100f);

            MockGreatPersonCanon.Setup(canon => canon.GetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ))
                                .Returns(90f);

            civ.PerformGreatPeopleGeneration();

            MockGreatPersonCanon.Verify(
                canon => canon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatAdmiral, civ, 10f), Times.Once,
                "GreatAdmiral not set as expected"
            );

            MockGreatPersonCanon.Verify(
                canon => canon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatArtist, civ, 0f), Times.Once,
                "GreatArtist not set as expected"
            );

            MockGreatPersonCanon.Verify(
                canon => canon.SetPointsTowardsTypeForCiv(GreatPersonType.GreatEngineer, civ, It.IsAny<float>()),
                Times.Never, "GreatArtist unexpectedly set"
            );
        }

        #endregion

        #region utilities

        private ITechDefinition BuildTech(int cost) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech.Setup(tech => tech.Cost).Returns(cost);
            MockTechCanon.Setup(
                canon => canon.IsTechAvailableToCiv(mockTech.Object, It.IsAny<ICivilization>())
            ).Returns(true);

            return mockTech.Object;
        }

        #endregion

        #endregion

    }

}
