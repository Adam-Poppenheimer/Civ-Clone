using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.Technology {

    [TestFixture]
    public class TechCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<ITechDefinition> AvailableTechs = new List<ITechDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTechs.Clear();

            Container.Bind<List<ITechDefinition>>().WithId("Available Techs").FromInstance(AvailableTechs);

            Container.Bind<IEnumerable<IAbilityDefinition>>().WithId("Available Abilities")
                .FromInstance(new List<IAbilityDefinition>());

            Container.Bind<CivilizationSignals>().AsSingle();

            Container.Bind<TechCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetPrerequisiteChainToResearchTech should always return a chain of techs " + 
            "that can be researched from start to finish and result in the discovery of the argued tech. " +
            "It is not currently obligated to check for prerequisite cycles or other technologies that " +
            "cannot be researched")]
        public void GetPrerequisiteChainToResearchTech_ReturnsProperChain() {
            var tierOneTechA = BuildTech("Tier One Tech A");
            var tierOneTechB = BuildTech("Tier One Tech B");
            var tierOneTechC = BuildTech("Tier One Tech C");

            var tierTwoTechA = BuildTech("Tier Two Tech A", new List<ITechDefinition>() { tierOneTechA, tierOneTechB });
            var tierTwoTechB = BuildTech("Tier Two Tech B", new List<ITechDefinition>() { tierOneTechB, tierOneTechC });

            var tierThreeTechA = BuildTech("Tier Three Tech A", new List<ITechDefinition>() { tierTwoTechA, tierTwoTechB });

            var civilization = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            var prerequisiteChain = techCanon.GetPrerequisiteChainToResearchTech(tierThreeTechA, civilization);

            Assert.AreEqual(prerequisiteChain.Last(), tierThreeTechA);

            CollectionAssert.AllItemsAreUnique(prerequisiteChain);

            foreach(var chainLink in prerequisiteChain) {
                int chainIndex = prerequisiteChain.IndexOf(chainLink);
                foreach(var prerequisite in chainLink.Prerequisites) {
                    int prerequisiteIndex = prerequisiteChain.IndexOf(prerequisite);

                    if(prerequisiteIndex == -1) {
                        Assert.That(techCanon.IsTechDiscoveredByCiv(prerequisite, civilization), 
                            string.Format(
                                "Undiscovered prerequisite {0} of tech {1} is not in the prerequisite chain",
                                prerequisite, chainLink
                            )
                        );
                    }else {
                        Assert.That(prerequisiteIndex < chainIndex,
                            string.Format("Prerequisite {0} of tech {1} proceeds it in the prerequisite chain",
                                prerequisite, chainLink
                            )
                        );
                    }
                }
            }
        }

        [Test(Description = "When SetTechAsResearchedForCiv is called on a tech, that " +
            "tech should be considered researched by GetTechsDiscoveredByCiv and " + 
            "IstechAvailableToCiv. It should also no longer be considered available by " +
            "GetTechsAvailableToCiv or IsTechAvailableToCiv")]
        public void SetTechAsResearchedForCiv_ReflectedInDiscoveryQueries() {
            var techOne = BuildTech("Tech One");
            var techTwo = BuildTech("Tech Two");

            var civOne = BuildCivilization();
            var civTwo = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.SetTechAsDiscoveredForCiv(techOne, civOne);
            Assert.IsTrue(techCanon.IsTechDiscoveredByCiv(techOne, civOne),  "TechOne's discovery by CivOne not acknowledged");
            Assert.IsFalse(techCanon.IsTechDiscoveredByCiv(techOne, civTwo), "TechOne's discovery by CivTwo falsely acknowledged");

            CollectionAssert.Contains(techCanon.GetTechsDiscoveredByCiv(civOne), techOne, "GetTechsDiscoveredByCiv on CivOne does not contain TechOne");
            CollectionAssert.DoesNotContain(techCanon.GetTechsDiscoveredByCiv(civTwo), techOne, "GetTechsDiscoveredByCiv on CivTwo falsely contains TechOne");

            techCanon.SetTechAsDiscoveredForCiv(techTwo, civOne);

            Assert.IsTrue(techCanon.IsTechDiscoveredByCiv(techTwo, civOne), "TechTwo's discovery by CivOne not acknowledged");

            CollectionAssert.Contains(techCanon.GetTechsDiscoveredByCiv(civOne), techTwo, "GetTechsDiscoveredByCiv on CivOne does not contain TechTwo");
        }

        [Test(Description = "GetTechsAvailableToCiv should always return all non-researched " +
            "techs that have no prerequisities")]
        public void GetTechsAvailableToCiv_ReturnsNoPrerequisiteTechs() {
            var techOne   = BuildTech("Tech One");
            var techTwo   = BuildTech("Tech Two");
            var techThree = BuildTech("Tech Three");

            BuildTech("Tech Four", prerequisities: new List<ITechDefinition>() { techOne, techTwo });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                new List<ITechDefinition>() { techOne, techTwo, techThree },
                techCanon.GetTechsAvailableToCiv(civ),
                "GetTechsAvailableToCiv returned an unexpected collection of techs"
            );
        }

        [Test(Description = "GetTechsAvailableToCiv and IsTechAvailableToCiv should include any " +
            "tech that hasn't been discovered and whose prerequisites are all discovered")]
        public void AvailableTechsConsiderDiscoveredTechs() {
            var techOne   = BuildTech("Tech One");
            var techTwo   = BuildTech("Tech Two");
            var techThree = BuildTech("Tech Three");

            var techFour = BuildTech("Tech Four", prerequisities: new List<ITechDefinition>() { techOne, techTwo });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            CollectionAssert.AreEquivalent(
                new List<ITechDefinition>() { techThree, techFour },
                techCanon.GetTechsAvailableToCiv(civ),
                "GetTechsAvailableToCiv returned an unexpected collection of techs"
            );

            Assert.IsFalse(techCanon.IsTechAvailableToCiv(techOne,   civ), "TechOne is falsely considered available to civ");
            Assert.IsFalse(techCanon.IsTechAvailableToCiv(techTwo,   civ), "TechTwo is falsely considered available to civ");
            Assert.IsTrue (techCanon.IsTechAvailableToCiv(techThree, civ), "TechThree is not considered available to civ");
            Assert.IsTrue (techCanon.IsTechAvailableToCiv(techFour,  civ), "TechFour is not considered available to civ");
        }

        [Test(Description = "SetTechAsResearchedForCiv should modify what improvements are " + 
            "considered researched by GetResearchedBuildings and IsBuildingResearchedForCity")]
        public void SetTechAsResearchedForCiv_ReflectedInBuildingQueries() {
            var techOne = BuildTech("Tech One", buildings: new List<IBuildingTemplate>() {
                BuildBuildingTemplate(),
                BuildBuildingTemplate(),
            });

            var techTwo = BuildTech("Tech Two", buildings: new List<IBuildingTemplate>() {
                BuildBuildingTemplate(),
                BuildBuildingTemplate(),
                BuildBuildingTemplate(),
            });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            CollectionAssert.AreEquivalent(
                techOne.BuildingsEnabled.Concat(techTwo.BuildingsEnabled),
                techCanon.GetResearchedBuildings(civ),
                "GetResearchedBuildings returned an unexpected set of templates"
            );
        }

        [Test(Description = "SetTechAsResearchedForCiv should modify what units are " + 
            "considered researched by GetResearchedUnits and IsUnitResearchedForCity")]
        public void SetTechAsResearchedForCiv_ReflectedInUnitQueries() {
            var techOne = BuildTech("Tech One", units: new List<IUnitTemplate>() {
                BuildUnitTemplate(),
                BuildUnitTemplate(),
            });

            var techTwo = BuildTech("Tech Two", units: new List<IUnitTemplate>() {
                BuildUnitTemplate(),
                BuildUnitTemplate(),
                BuildUnitTemplate(),
            });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            CollectionAssert.AreEquivalent(
                techOne.UnitsEnabled.Concat(techTwo.UnitsEnabled),
                techCanon.GetResearchedUnits(civ),
                "GetResearchedUnits returned an unexpected set of templates"
            );
        }

        [Test(Description = "SetTechAsResearchedForCiv should modify what abilities are " + 
            "considered researched by GetResearchedAbilities and IsAbilityResearchedForCity")]
        public void SetTechAsResearchedForCiv_ReflectedInAbilityQueries() {
            var techOne = BuildTech("Tech One", abilities: new List<IAbilityDefinition>() {
                BuildAbilityDefinition(),
                BuildAbilityDefinition(),
            });

            var techTwo = BuildTech("Tech Two", abilities: new List<IAbilityDefinition>() {
                BuildAbilityDefinition(),
                BuildAbilityDefinition(),
                BuildAbilityDefinition(),
            });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            CollectionAssert.AreEquivalent(
                techOne.AbilitiesEnabled.Concat(techTwo.AbilitiesEnabled),
                techCanon.GetResearchedAbilities(civ),
                "GetResearchedAbilities returned an unexpected set of definitions"
            );
        }

        #endregion

        #region utilities

        private ITechDefinition BuildTech(
            string name,
            List<ITechDefinition> prerequisities = null,
            List<IBuildingTemplate> buildings = null,
            List<IUnitTemplate> units = null,
            List<IAbilityDefinition> abilities = null
        ){
            var mockTech = new Mock<ITechDefinition>();
            mockTech.Name = name;

            mockTech.Setup(tech => tech.Name).Returns(name);

            mockTech.Setup(tech => tech.Prerequisites)   .Returns(prerequisities != null ? prerequisities : new List<ITechDefinition>());
            mockTech.Setup(tech => tech.BuildingsEnabled).Returns(buildings      != null ? buildings      : new List<IBuildingTemplate>());
            mockTech.Setup(tech => tech.UnitsEnabled)    .Returns(units          != null ? units          : new List<IUnitTemplate>());
            mockTech.Setup(tech => tech.AbilitiesEnabled).Returns(abilities      != null ? abilities      : new List<IAbilityDefinition>());

            AvailableTechs.Add(mockTech.Object);

            return mockTech.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IBuildingTemplate BuildBuildingTemplate() {
            return new Mock<IBuildingTemplate>().Object;
        }

        private IUnitTemplate BuildUnitTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        private IAbilityDefinition BuildAbilityDefinition() {
            return new Mock<IAbilityDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
