using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Improvements;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.GameMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class BuildImprovementAbilityHandlerTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.BuildImprovement,
                            ArgsToPass = new List<string>() { "Improvement One" }
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Right type, valid template name argued").Returns(true);

                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.BuildImprovement,
                            ArgsToPass = new List<string>() { "Improvement One", "Arglebargle" }
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Right type, valid template name argued, redundant argument").Returns(true);

                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.BuildImprovement,
                            ArgsToPass = new List<string>() { "Improvement Two" }
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Right type, template name argued, but template not valid on tile").Returns(false);

                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.BuildImprovement,
                            ArgsToPass = new List<string>() { "Arglebargle", "Improvement One" }
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Right type, first argument not name of template").Returns(false);

                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.BuildImprovement,
                            ArgsToPass = new List<string>()
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Right type, no arguments").Returns(false);

                yield return new TestCaseData(
                    "Build Improvement", 
                    new List<AbilityCommandRequest>() {
                        new AbilityCommandRequest() {
                            CommandType = AbilityCommandType.FoundCity,
                            ArgsToPass = new List<string>() { "Improvement One" }
                        }
                    },
                    new List<string>() { "Improvement One", "Improvement Two" },
                    new List<string>() { "Improvement One" }
                ).SetName("Wrong type, valid template name argued").Returns(false);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<IImprovementValidityLogic> MockImprovementValidityLogic;
        private Mock<IUnitPositionCanon>        MockUnitPositionCanon;
        private Mock<IImprovementFactory>       MockImprovementFactory;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        private List<IImprovementTemplate> AllTemplates = new List<IImprovementTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTemplates.Clear();

            MockImprovementValidityLogic = new Mock<IImprovementValidityLogic>();
            MockUnitPositionCanon        = new Mock<IUnitPositionCanon>();
            MockImprovementFactory       = new Mock<IImprovementFactory>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<IEnumerable<IImprovementTemplate>>().WithId("Available Improvement Templates").FromInstance(AllTemplates);

            Container.Bind<IImprovementValidityLogic>().FromInstance(MockImprovementValidityLogic.Object);
            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementFactory>      ().FromInstance(MockImprovementFactory      .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<BuildImprovementAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanHandleAbilityOnUnit should return true if and only if the following conditions are met:\n" + 
            "\t1. There is exactly one CommandRequest whose type is BuildImprovement\n" +
            "\t2. That command request has an argument that is the name of some template in Available Improvement Templates\n" +
            "\t3. The improvement of that name is valid on the argued unit's location\n")]
        [TestCaseSource("TestCases")]
        public bool CanHandleAbilityOnUnitTests(
            string abilityName, IEnumerable<AbilityCommandRequest> commandRequests,
            IEnumerable<string> templateNames, List<string> validTemplateNamesOnLocation
        ){
            var ability = BuildAbility(abilityName, commandRequests);

            foreach(var name in templateNames) {
                BuildTemplate(name);
            }

            var unit = BuildUnit(BuildTile(validTemplateNamesOnLocation));

            var abilityHandler = Container.Resolve<BuildImprovementAbilityHandler>();

            return abilityHandler.CanHandleAbilityOnUnit(ability, unit);
        }

        [Test(Description = "TryHandleAbilityOnUnit should return identically to CanHandleAbilityOnUnit. " +
            "It should also create a new improvement of the argued template at the argued location when " +
            "it returns true")]
        [TestCaseSource("TestCases")]
        public bool TryHandleAbilityOnUnitTests_ImprovementCreated(
            string abilityName, IEnumerable<AbilityCommandRequest> commandRequests,
            IEnumerable<string> templateNames, List<string> validTemplateNamesOnLocation
         ){
            var ability = BuildAbility(abilityName, commandRequests);

            string expectedTemplateName = null;

            var improvementRequests = commandRequests.Where(request => request.CommandType == AbilityCommandType.BuildImprovement);
            if(improvementRequests.Count() != 0) {
                var argsOfFirstRequest = commandRequests.First().ArgsToPass;
                if(argsOfFirstRequest != null) {
                    expectedTemplateName = argsOfFirstRequest.FirstOrDefault();
                }
            }

            IImprovementTemplate expectedTemplate = null;

            foreach(var name in templateNames) {
                var newTemplate = BuildTemplate(name);
                if(name.Equals(expectedTemplateName)) {
                    expectedTemplate = newTemplate;
                }
            }            

            var location = BuildTile(validTemplateNamesOnLocation);
            var unit = BuildUnit(location);

            var abilityHandler = Container.Resolve<BuildImprovementAbilityHandler>();

            var handleResult = abilityHandler.TryHandleAbilityOnUnit(ability, unit);
            if(handleResult) {
                MockImprovementFactory.Verify(
                    factory => factory.Create(expectedTemplate, location), 
                    Times.Once, "ImprovementFactory.Create was not called as expected"
                );
                return true;
            }else {
                MockImprovementFactory.Verify(
                    factory => factory.Create(It.IsAny<IImprovementTemplate>(), It.IsAny<IMapTile>()),
                    Times.Never, "ImprovementFactory.Create was called unexpectedly"
                );
                return false;
            }
        }

        [Test(Description = "CanHandleAbilityOnUnit and TryHandleAbilityOnUnit should throw an InvalidOperationException " +
            "when its argued ability contains multiple command requests whose type is BuildImprovement")]
        public void BothMethods_ThrowWhenMultipleImprovementRequests() {
            var ability = BuildAbility("Invalid Ability", new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement }
            });

            var unit = BuildUnit(BuildTile(new List<string>()));

            var abilityHandler = Container.Resolve<BuildImprovementAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => abilityHandler.CanHandleAbilityOnUnit(ability, unit),
                "CanHandleAbilityOnUnit failed to throw on an ability with too many improvement commands");

            Assert.Throws<InvalidOperationException>(() => abilityHandler.TryHandleAbilityOnUnit(ability, unit),
                "TryHandleAbilityOnUnit failed to throw on an ability with too many improvement commands");
        }

        #endregion

        #region utilities

        private IUnitAbilityDefinition BuildAbility(string name, IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IUnitAbilityDefinition>();

            mockAbility.Setup(ability => ability.name).Returns(name);
            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private IImprovementTemplate BuildTemplate(string name) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.name).Returns(name);

            AllTemplates.Add(mockTemplate.Object);

            MockImprovementLocationCanon.Setup(
                canon => canon.CanPlaceImprovementOfTemplateAtLocation(mockTemplate.Object, It.IsAny<IMapTile>())
            ).Returns(true);

            return mockTemplate.Object;
        }

        private IMapTile BuildTile(List<string> validTemplateNames) {
            var mockTile = new Mock<IMapTile>();

            foreach(var name in validTemplateNames) {
                MockImprovementValidityLogic
                    .Setup(logic => logic.IsTemplateValidForTile(
                        It.Is<IImprovementTemplate>(template => template.name.Equals(name)),
                        mockTile.Object
                    ))
                    .Returns(true);
            }            

            return mockTile.Object;
        }

        private IUnit BuildUnit(IMapTile location) {
            var mockUnit = new Mock<IUnit>();

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
