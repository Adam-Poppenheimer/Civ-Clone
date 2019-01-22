using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Technology;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Technology {

    public class UnitUpgradeLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITechCanon> MockTechCanon;

        private List<IUnitUpgradeLine> AllUpgradeLines = new List<IUnitUpgradeLine>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllUpgradeLines.Clear();

            MockTechCanon = new Mock<ITechCanon>();

            Container.Bind<ITechCanon>().FromInstance(MockTechCanon.Object);

            Container.Bind<IEnumerable<IUnitUpgradeLine>>().WithId("All Upgrade Lines").FromInstance(AllUpgradeLines);

            Container.Bind<UnitUpgradeLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetUpgradeLineForUnit_ReturnsFirstUpgradeLineContainingUnitTemplate() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

                          BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                });
            var lineTwo = BuildUpgradeLine(new List<IUnitTemplate>() { templateOne, templateTwo   });
                          BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo, templateThree });

            var unit = BuildUnit(templateTwo);

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            Assert.AreEqual(lineTwo, upgradeLogic.GetUpgradeLineForUnit(unit));
        }

        [Test]
        public void GetUpgradeLineForUnit_ReturnsNullIfNoUpgradeLineContainsUnit() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne, templateTwo   });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo, templateThree });

            var unit = BuildUnit(BuildTemplate());

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            Assert.IsNull(upgradeLogic.GetUpgradeLineForUnit(unit));
        }

        [Test]
        public void GetCuttingEdgeUnitsForCiv_GetsLastResearchedUnitTemplate_FromEveryUpgradeLine_IgnoringDuplicates() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne, templateTwo   });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo, templateThree });

            var civ = BuildCiv(templateOne, templateTwo);

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { templateOne, templateTwo },
                upgradeLogic.GetCuttingEdgeUnitsForCiv(civ)
            );
        }

        [Test]
        public void GetCuttingEdgeUnitsForCiv_IgnoresNullEntries() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne, templateTwo   });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo, templateThree });

            var civ = BuildCiv(templateOne);

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { templateOne },
                upgradeLogic.GetCuttingEdgeUnitsForCiv(civ)
            );
        }

        [Test]
        public void GetCuttingEdgeUnitsForCivs_GetsLastUnitTemplate_ResearchedByAnyCiv_FromEveryUpgradeLine_IgnoringDuplicates() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();
            var templateFour  = BuildTemplate();

            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                               });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne,  templateTwo                 });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo,  templateThree               });            
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo,  templateThree, templateFour });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateFour, templateThree               });

            var civOne = BuildCiv(templateOne, templateTwo);
            var civTwo = BuildCiv(templateTwo, templateFour);

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { templateOne, templateTwo, templateFour },
                upgradeLogic.GetCuttingEdgeUnitsForCivs(civOne, civTwo)
            );
        }

        [Test]
        public void GetCuttingEdgeUnitsForCivs_IgnoresNullEntries() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne                });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateOne, templateTwo   });
            BuildUpgradeLine(new List<IUnitTemplate>() { templateTwo, templateThree });

            var civOne = BuildCiv(templateOne);
            var civTwo = BuildCiv(templateOne);

            var upgradeLogic = Container.Resolve<UnitUpgradeLogic>();

            CollectionAssert.AreEquivalent(
                new List<IUnitTemplate>() { templateOne },
                upgradeLogic.GetCuttingEdgeUnitsForCivs(civOne, civTwo)
            );
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        private IUnit BuildUnit(IUnitTemplate template) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Template).Returns(template);

            return mockUnit.Object;
        }

        private IUnitUpgradeLine BuildUpgradeLine(List<IUnitTemplate> units) {
            var mockLine = new Mock<IUnitUpgradeLine>();

            mockLine.Setup(line => line.Units).Returns(units.AsReadOnly());

            var newLine = mockLine.Object;

            AllUpgradeLines.Add(newLine);

            return newLine;
        }

        private ICivilization BuildCiv(params IUnitTemplate[] discoveredTemplates) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var template in discoveredTemplates) {
                MockTechCanon.Setup(canon => canon.IsUnitResearchedForCiv(template, newCiv)).Returns(true);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
