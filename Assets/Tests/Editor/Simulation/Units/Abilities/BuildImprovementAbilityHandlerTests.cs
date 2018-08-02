using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Improvements;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class BuildImprovementAbilityHandlerTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanHandleAbilityOnUnitTestData {

            public AbilityTestData Ability;

            public UnitTestData Unit;

            public List<ImprovementTemplateTestData> ImprovementTemplates = new List<ImprovementTemplateTestData>();

        }

        public class TryHandleAbilityOnUnitTestData {

            public AbilityTestData Ability;

            public UnitTestData Unit;

            public List<ImprovementTemplateTestData> ImprovementTemplates = new List<ImprovementTemplateTestData>();

            public bool ExpectNewSite;

            public bool ExpectAttachmentToExistingSite;

            public bool ExpectLockingOfUnit;

        }

        public class AbilityTestData {

            public string Name;

            public List<AbilityCommandRequest> CommandRequests = new List<AbilityCommandRequest>();

        }

        public class UnitTestData {

            public HexCellTestData Location = new HexCellTestData();

        }

        public class HexCellTestData {

            public List<ImprovementTestData> Improvements = new List<ImprovementTestData>();

        }

        public class ImprovementTemplateTestData {

            public string Name;

            public bool IsValid;

            public int TurnsToConstruct;

        }

        public class ImprovementTestData {

            public int TemplateIndex;

            public int WorkInvested;

            public bool IsConstructed;

            public bool IsPillaged;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanHandleAbilityOnUnitTestCases {
            get {
                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability flagging valid template on empty cell").Returns(true);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Invalid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability flagging invalid template on empty cell").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true },
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Invalid Template", "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability flagging valid template but in wrong args slot").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true },
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.ClearVegetation,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability flagging valid template but with wrong command type").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true },
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.ClearVegetation,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            },
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Invalid Template" }
                            },
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability has mix of valid and invalid requests").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true },
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            },
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() { }
                }).SetName("Ability has multiple valid requests").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 0, IsConstructed = false }
                            }
                        }
                    }
                }).SetName("Unit location has unconstructed improvement").Returns(true);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 0, IsConstructed = true }
                            }
                        }
                    }
                }).SetName("Unit location has constructed improvement").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 0, IsConstructed = false, IsPillaged = true }
                            }
                        }
                    }
                }).SetName("Unit location has unconstructed and pillaged improvement").Returns(false);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 0 }
                            }
                        }
                    }
                }).SetName("Unit location has an improvement of the flagged template").Returns(true);

                yield return new TestCaseData(new CanHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Template 1", IsValid = true },
                        new ImprovementTemplateTestData() { Name = "Template 2", IsValid = true }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability One",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Template 1" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 1 }
                            }
                        }
                    }
                }).SetName("Unit location has an improvement of a different template").Returns(true);
            }
        }

        public static IEnumerable TryHandleAbilityOnUnitTestCases {
            get {
                yield return new TestCaseData(new TryHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true, TurnsToConstruct = 2 }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability 1",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                 CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() { }
                    },
                    ExpectNewSite = true,
                    ExpectLockingOfUnit = true
                }).SetName("Valid ability on unit with empty location").Returns(new AbilityExecutionResults(true, null));

                yield return new TestCaseData(new TryHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Invalid Template", IsValid = false, TurnsToConstruct = 2 }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability 1",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                 CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Invalid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() { }
                    },
                    ExpectLockingOfUnit = false
                }).SetName("Invalid ability").Returns(new AbilityExecutionResults(false, null));

                yield return new TestCaseData(new TryHandleAbilityOnUnitTestData() {
                    ImprovementTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Valid Template", IsValid = true, TurnsToConstruct = 2 }
                    },
                    Ability = new AbilityTestData() {
                        Name = "Ability 1",
                        CommandRequests = new List<AbilityCommandRequest>() {
                            new AbilityCommandRequest() {
                                 CommandType = AbilityCommandType.BuildImprovement,
                                ArgsToPass = new List<string>() { "Valid Template" }
                            }
                        }
                    },
                    Unit = new UnitTestData() {
                        Location = new HexCellTestData() {
                            Improvements = new List<ImprovementTestData>() {
                                new ImprovementTestData() { TemplateIndex = 0 }
                            }
                        }
                    },
                    ExpectAttachmentToExistingSite = true,
                    ExpectLockingOfUnit = true
                }).SetName("Valid ability on unit whose location has a valid site").Returns(new AbilityExecutionResults(true, null));
            }
        }

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

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<IHexCell>()))
                .Returns(new List<IImprovement>());

            MockImprovementFactory
                .Setup(factory => factory.BuildImprovement(It.IsAny<IImprovementTemplate>(), It.IsAny<IHexCell>()))
                .Returns<IImprovementTemplate, IHexCell>((template, cell) => BuildImprovement(template, cell));

            Container.Bind<IEnumerable<IImprovementTemplate>>().WithId("Available Improvement Templates").FromInstance(AllTemplates);

            Container.Bind<IImprovementValidityLogic>().FromInstance(MockImprovementValidityLogic.Object);
            Container.Bind<IUnitPositionCanon>       ().FromInstance(MockUnitPositionCanon       .Object);
            Container.Bind<IImprovementFactory>      ().FromInstance(MockImprovementFactory      .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<BuildImprovementAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("CanHandleAbilityOnUnitTestCases")]
        [Test(Description = "")]
        public bool CanHandleAbilityOnUnitTests(CanHandleAbilityOnUnitTestData testData) {
            var allImprovementTemplates = testData.ImprovementTemplates.Select(data => BuildImprovementTemplate(data)).ToList();
            
            var ability = BuildAbility(testData.Ability);
            var unit    = BuildUnit(testData.Unit, allImprovementTemplates);

            var abilityHandler = Container.Resolve<BuildImprovementAbilityHandler>();

            return abilityHandler.CanHandleAbilityOnUnit(ability, unit);
        }

        [TestCaseSource("TryHandleAbilityOnUnitTestCases")]
        [Test(Description = "")]
        public AbilityExecutionResults TryHandleAbilityOnUnitTests(TryHandleAbilityOnUnitTestData testData) {
            var allImprovementTemplates = testData.ImprovementTemplates.Select(data => BuildImprovementTemplate(data)).ToList();
            
            Mock<IUnit> mockUnit;

            var ability  = BuildAbility(testData.Ability);
            var testUnit = BuildUnit(testData.Unit, allImprovementTemplates, out mockUnit);

            var abilityHandler = Container.Resolve<BuildImprovementAbilityHandler>();

            var retval = abilityHandler.TryHandleAbilityOnUnit(ability, testUnit);

            var unitLocation = MockUnitPositionCanon.Object.GetOwnerOfPossession(testUnit);

            if(testData.ExpectNewSite) {
                MockImprovementFactory.Verify(
                    factory => factory.BuildImprovement(It.IsAny<IImprovementTemplate>(), unitLocation),
                    Times.Once, "A new improvement construction site was not created at the expected location"
                );
            }

            if(testData.ExpectAttachmentToExistingSite) {
                MockImprovementFactory.Verify(
                    factory => factory.BuildImprovement(It.IsAny<IImprovementTemplate>(), unitLocation),
                    Times.Never, "A new improvement construction site was unexpectedly created"
                );
            }

            if(testData.ExpectLockingOfUnit) {
                mockUnit.Verify(
                    unit => unit.LockIntoConstruction(), Times.Once, "Unit was not locked into construction as expected"
                );
            }else {
                mockUnit.Verify(
                    unit => unit.LockIntoConstruction(), Times.Never, "Unit was unexpectedly locked into construction"
                );
            }

            return retval;
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(AbilityTestData abilityData) {
            var mockDefinition = new Mock<IAbilityDefinition>();

            mockDefinition.Setup(definition => definition.name).Returns(abilityData.Name);
            mockDefinition.Setup(definition => definition.CommandRequests).Returns(abilityData.CommandRequests);

            return mockDefinition.Object;
        }

        private IImprovementTemplate BuildImprovementTemplate(ImprovementTemplateTestData templateData) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.name)            .Returns(templateData.Name);    
            mockTemplate.Setup(template => template.TurnsToConstruct).Returns(templateData.TurnsToConstruct);        

            var newTemplate = mockTemplate.Object;

            MockImprovementValidityLogic
                .Setup(logic => logic.IsTemplateValidForCell(newTemplate, It.IsAny<IHexCell>(), false))
                .Returns(templateData.IsValid);

            AllTemplates.Add(newTemplate);
            return newTemplate;
        }

        private IUnit BuildUnit(UnitTestData unitData, List<IImprovementTemplate> availableTemplates) {
            Mock<IUnit> mock;

            return BuildUnit(unitData, availableTemplates, out mock);
        }

        private IUnit BuildUnit(UnitTestData unitData, List<IImprovementTemplate> availableTemplates, out Mock<IUnit> mockUnit) {
            mockUnit = new Mock<IUnit>();
            var newUnit = mockUnit.Object;

            var unitLocation = BuildCell(unitData.Location, availableTemplates);

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(unitLocation);

            return newUnit;
        }

        private IHexCell BuildCell(HexCellTestData cellData, List<IImprovementTemplate> availableTemplates) {
            var newCell = new Mock<IHexCell>().Object;

            var improvements = cellData.Improvements.Select(data => BuildImprovement(data, availableTemplates));

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(improvements);
            
            return newCell;
        }

        private IImprovement BuildImprovement(
            ImprovementTestData improvementData, List<IImprovementTemplate> validTemplates
        ){
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.SetupAllProperties();

            mockImprovement.Setup(improvement => improvement.Template)     .Returns(validTemplates[improvementData.TemplateIndex]);
            mockImprovement.Setup(improvament => improvament.IsConstructed).Returns(improvementData.IsConstructed);
            mockImprovement.Setup(improvament => improvament.IsPillaged)   .Returns(improvementData.IsPillaged);

            return mockImprovement.Object;
        }

        private IImprovement BuildImprovement(
            IImprovementTemplate template, IHexCell location, int workInvested = 0,
            bool isConstructed = false, bool isPillaged = false
        ) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.SetupAllProperties();

            mockImprovement.Setup(improvement => improvement.Template)     .Returns(template);
            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(isConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(isPillaged);

            var newImprovement = mockImprovement.Object;

            newImprovement.WorkInvested = workInvested;

            MockImprovementLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newImprovement))
                .Returns(location);

            return newImprovement;
        }

        #endregion

        #endregion

    }

}
