﻿using System;
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
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class BuildImprovementAbilityHandlerTests : ZenjectUnitTestFixture {

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

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<IHexCell>()))
                .Returns(new List<IImprovement>());

            Container.Bind<IEnumerable<IImprovementTemplate>>().WithId("Available Improvement Templates").FromInstance(AllTemplates);

            Container.Bind<IImprovementValidityLogic>().FromInstance(MockImprovementValidityLogic.Object);
            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementFactory>      ().FromInstance(MockImprovementFactory      .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<BuildImprovementAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanHandleAbilityOnUnit can only return true if the argued ability " +
            "has a single BuildImprovement command whose first argument is a valid template name. " + 
            "TryHandleAbilityOnUnit should also return an AbilityExecutionResults that reflects " +
            "CanHandleAbilityOnUnit")]
        public void CanHandleAbilityOnUnit_RequiresProperlyFormedDefinition() {
            var validAbility = BuildDefinition(new List<AbilityCommandRequest>(){
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement One" } }
            });

            var validAbility_RedundantArguments = BuildDefinition(new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement One", "Improvement Three" } }
            });

            var invalidAbility_Duplicates = BuildDefinition(new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement One" } },
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement One" } }
            });

            var invalidAbility_WrongType = BuildDefinition(new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.FoundCity, ArgsToPass = new List<string>() { "Improvement One" } }
            });

            var invalidAbility_NonTemplateArgument = BuildDefinition(new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Not An Improvement" } }
            });

            var invalidAbility_InvalidTemplateArgument = BuildDefinition(new List<AbilityCommandRequest>() {
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement Three" } }
            });            

            BuildImprovementTemplate("Improvement One",   true);
            BuildImprovementTemplate("Improvement Two",   true);
            BuildImprovementTemplate("Improvement Three", false);

            var unit = BuildUnit(BuildTile());

            var handlerToTest = Container.Resolve<BuildImprovementAbilityHandler>();

            Assert.IsTrue (handlerToTest.CanHandleAbilityOnUnit(validAbility,                           unit), "CanHandleAbilityOnUnit invoked on validAbility was not handled correctly");
            Assert.IsTrue (handlerToTest.CanHandleAbilityOnUnit(validAbility_RedundantArguments,        unit), "CanHandleAbilityOnUnit invoked on validAbility_RedundantArguments was not handled correctly");
            Assert.IsFalse(handlerToTest.CanHandleAbilityOnUnit(invalidAbility_Duplicates,              unit), "CanHandleAbilityOnUnit invoked on invalidAbility_Duplicates was not handled correctly");
            Assert.IsFalse(handlerToTest.CanHandleAbilityOnUnit(invalidAbility_WrongType,               unit), "CanHandleAbilityOnUnit invoked on invalidAbility_WrongType was not handled correctly");
            Assert.IsFalse(handlerToTest.CanHandleAbilityOnUnit(invalidAbility_NonTemplateArgument,     unit), "CanHandleAbilityOnUnit invoked on invalidAbility_NonTemplateArgument was not handled correctly");
            Assert.IsFalse(handlerToTest.CanHandleAbilityOnUnit(invalidAbility_InvalidTemplateArgument, unit), "CanHandleAbilityOnUnit invoked on invalidAbility_InvalidTemplateArgument was not handled correctly");

            Assert.IsTrue (handlerToTest.TryHandleAbilityOnUnit(validAbility,                           unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on validAbility was not handled correctly");
            Assert.IsTrue (handlerToTest.TryHandleAbilityOnUnit(validAbility_RedundantArguments,        unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on validAbility_RedundantArguments was not handled correctly");
            Assert.IsFalse(handlerToTest.TryHandleAbilityOnUnit(invalidAbility_Duplicates,              unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on invalidAbility_Duplicates was not handled correctly");
            Assert.IsFalse(handlerToTest.TryHandleAbilityOnUnit(invalidAbility_WrongType,               unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on invalidAbility_WrongType was not handled correctly");
            Assert.IsFalse(handlerToTest.TryHandleAbilityOnUnit(invalidAbility_NonTemplateArgument,     unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on invalidAbility_NonTemplateArgument was not handled correctly");
            Assert.IsFalse(handlerToTest.TryHandleAbilityOnUnit(invalidAbility_InvalidTemplateArgument, unit).AbilityHandled, "TryHandleAbilityOnUnit invoked on invalidAbility_InvalidTemplateArgument was not handled correctly");
        }

        [Test(Description = "TryHandleAbilityOnUnit should, in its valid cases, produce a new " +
            "improvement of the specified template and return a new BuildImprovementOngoingAbility " +
            "with the new improvement and the argued unit")]
        public void TryHandleAbilityOnUnit_BuildsNewImprovement() {
            var templateOne = BuildImprovementTemplate("Improvement One", true);

            var ability = BuildDefinition(new List<AbilityCommandRequest>(){
                new AbilityCommandRequest() { CommandType = AbilityCommandType.BuildImprovement, ArgsToPass = new List<string>() { "Improvement One" } }
            });            

            var unitLocation = BuildTile();
            var unit = BuildUnit(unitLocation);

            var handlerToTest = Container.Resolve<BuildImprovementAbilityHandler>();

            IImprovement constructedImprovement = null;
            MockImprovementFactory
                .Setup(factory => factory.Create(It.IsAny<IImprovementTemplate>(), It.IsAny<IHexCell>()))
                .Returns<IImprovementTemplate, IHexCell>(delegate(IImprovementTemplate template, IHexCell location) {
                    constructedImprovement = BuildImprovement(template, location);
                    return constructedImprovement;
                })
                .Callback(delegate(IImprovementTemplate template, IHexCell tile) {
                    Assert.AreEqual(templateOne, template, "ImprovementFactory.Create was called with an unexpected template");
                    Assert.AreEqual(unitLocation, tile, "ImprovementFactory.Create was called with an unexpected tile");
                });

            handlerToTest.TryHandleAbilityOnUnit(ability, unit);
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildDefinition(IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockDefinition = new Mock<IAbilityDefinition>();

            mockDefinition.Setup(definition => definition.CommandRequests).Returns(commandRequests);

            return mockDefinition.Object;
        }

        private IImprovementTemplate BuildImprovementTemplate(string name, bool isValid) {
            var mockTemplate = new Mock<IImprovementTemplate>();
            var newTemplate = mockTemplate.Object;

            mockTemplate.Setup(template => template.name).Returns(name);            

            MockImprovementValidityLogic.Setup(logic => logic.IsTemplateValidForCell(newTemplate, It.IsAny<IHexCell>()))
                .Returns(isValid);

            AllTemplates.Add(newTemplate);
            return newTemplate;
        }

        private IUnit BuildUnit(IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(location);

            return mockUnit.Object;
        }

        private IHexCell BuildTile() {
            var mockTile = new Mock<IHexCell>();

            return mockTile.Object;
        }

        private IImprovement BuildImprovement(IImprovementTemplate template, IHexCell location) {
            var mockImprovement = new Mock<IImprovement>();
            var newImprovement = mockImprovement.Object;

            mockImprovement.Setup(improvement => improvement.Template).Returns(template);

            MockImprovementLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newImprovement)).Returns(location);
            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        #endregion

        #endregion

    }

}
