using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Core {

    public class CivilizationRoundExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory> MockCivFactory;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockCivFactory = new Mock<ICivilizationFactory>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<ICivilizationFactory>().FromInstance(MockCivFactory.Object);

            Container.Bind<CivilizationRoundExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformStartOfRoundActions_EachCiv_HasProperMethodsCalledInOrder() {
            var civMocks = new List<Mock<ICivilization>>() {
                BuildMockCiv(BuildCivTemplate(false)), BuildMockCiv(BuildCivTemplate(false)),
                BuildMockCiv(BuildCivTemplate(false))
            };

            List<MockSequence> mockSequences = new List<MockSequence>();

            foreach(var civMoq in civMocks) {
                var sequence = new MockSequence();

                civMoq.InSequence(sequence).Setup(city => city.PerformIncome               ());
                civMoq.InSequence(sequence).Setup(city => city.PerformResearch             ());
                civMoq.InSequence(sequence).Setup(city => city.PerformGreatPeopleGeneration());
                civMoq.InSequence(sequence).Setup(city => city.PerformGoldenAgeTasks       ());

                mockSequences.Add(sequence);
            }

            var roundExecuter = Container.Resolve<CivilizationRoundExecuter>();

            roundExecuter.PerformStartOfRoundActions();
            
            foreach(var civMock in civMocks) {
                civMock.VerifyAll();
            }
        }

        [Test]
        public void PerformStartOfRoundActions_SkipsCivsWithBarbaricTemplates() {
            var mockCiv = BuildMockCiv(BuildCivTemplate(true));

            var roundExecuter = Container.Resolve<CivilizationRoundExecuter>();

            roundExecuter.PerformStartOfRoundActions();

            mockCiv.Verify(civ => civ.PerformIncome               (), Times.Never, "PerformIncome unexpectedly called");
            mockCiv.Verify(civ => civ.PerformResearch             (), Times.Never, "PerformResearch unexpectedly called");
            mockCiv.Verify(civ => civ.PerformGreatPeopleGeneration(), Times.Never, "PerformGreatPeopleGeneration unexpectedly called");
            mockCiv.Verify(civ => civ.PerformGoldenAgeTasks       (), Times.Never, "PerformGoldenAgeTasks unexpectedly called");
        }

        #endregion

        #region utilities

        private Mock<ICivilization> BuildMockCiv(ICivilizationTemplate template) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Template).Returns(template);

            AllCivs.Add(mockCiv.Object);

            return mockCiv;
        }

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        #endregion

        #endregion

    }

}
