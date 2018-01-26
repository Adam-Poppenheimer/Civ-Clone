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

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationYieldLogic> YieldLogicMock;

        private Mock<ITechCanon> TechCanonMock;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            YieldLogicMock = new Mock<ICivilizationYieldLogic>();
            TechCanonMock  = new Mock<ITechCanon>();

            Container.Bind<ICivilizationConfig>    ().FromMock();
            Container.Bind<ICivilizationYieldLogic>().FromInstance(YieldLogicMock.Object);
            Container.Bind<ITechCanon>             ().FromInstance(TechCanonMock .Object);

            Container.Bind<Civilization>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When PerformIncome is called, Civilization should check " + 
            "CivilizationYieldLogic.GetYieldOfCivilization to inform its stockpile changes " +
            "and LastScienceYield")]
        public void PerformIncome_StockpilesAndYieldsChanged() {
            var civilization = Container.Resolve<Civilization>();

            YieldLogicMock.Setup(logic => logic.GetYieldOfCivilization(civilization))
                .Returns(new ResourceSummary(gold: 2, production: 4, culture: 5, science: 10));

            civilization.PerformIncome();

            Assert.AreEqual(2,  civilization.GoldStockpile,    "Civilization has an unexpected GoldStockpile");
            Assert.AreEqual(5,  civilization.CultureStockpile, "Civilization has an unexpected CultureStockpile");
            Assert.AreEqual(10, civilization.LastScienceYield, "Civilization has an unexpected LastScienceYield");
        }

        [Test(Description = "When PerformResearch is called, if there are any techs " +
            "in the civilization's TechQueue, the civilization will modify its progress " +
            "in that tech by its LastScienceYield")]
        public void PerformResearch_ChangesProgressOnActiveTech() {
            var technology = BuildTech(20);

            var civilization = Container.Resolve<Civilization>();

            civilization.LastScienceYield = 10;

            civilization.TechQueue.Enqueue(technology);

            civilization.PerformResearch();

            TechCanonMock.Verify(
                canon => canon.GetProgressOnTechByCiv(technology, civilization),
                Times.Once,
                "Civilization did not tech TechCanon for the progress of its active tech"
            );

            TechCanonMock.Verify(
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

            var civilization = Container.Resolve<Civilization>();

            civilization.LastScienceYield = 20;

            civilization.TechQueue.Enqueue(technology);

            civilization.PerformResearch();

            TechCanonMock.Verify(
                canon => canon.IsTechAvailableToCiv(technology, civilization),
                Times.AtLeastOnce,
                "Civilization did not check to make sure its active tech was available"
            );

            TechCanonMock.Verify(
                canon => canon.SetTechAsDiscoveredForCiv(technology, civilization),
                Times.AtLeastOnce,
                "Civilization did not set its finished tech as discovered"
            );

            CollectionAssert.DoesNotContain(civilization.TechQueue, technology,
                "TechQueue still contains the discovered tech");
        }

        #endregion

        #region utilities

        private ITechDefinition BuildTech(int cost) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech.Setup(tech => tech.Cost).Returns(cost);
            TechCanonMock.Setup(
                canon => canon.IsTechAvailableToCiv(mockTech.Object, It.IsAny<ICivilization>())
            ).Returns(true);

            return mockTech.Object;
        }

        #endregion

        #endregion

    }

}
