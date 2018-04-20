﻿using System;
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
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Technology {

    [TestFixture]
    public class TechCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<ITechDefinition>              AvailableTechs     = new List<ITechDefinition>();
        private List<ISpecialtyResourceDefinition> AvailableResources = new List<ISpecialtyResourceDefinition>();
        private List<IAbilityDefinition>           AvailableAbilities = new List<IAbilityDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTechs    .Clear();
            AvailableResources.Clear();
            AvailableAbilities.Clear();

            Container.Bind<List<ITechDefinition>>                    ().WithId("Available Techs")              .FromInstance(AvailableTechs);
            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>().WithId("Available Specialty Resources").FromInstance(AvailableResources);
            Container.Bind<IEnumerable<IAbilityDefinition>>          ().WithId("Available Abilities")          .FromInstance(AvailableAbilities);

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

        [Test(Description = "GetResearchedBuildings should return only building templates " +
            "enabled by discovered techs. Buildings with no technology should be unavailable")]
        public void GetResearchedBuildings_GetsBuildingsFromDiscoveredTechs() {
            var discoveredBuildings = new List<IBuildingTemplate>() {
                BuildBuildingTemplate(), BuildBuildingTemplate()
            };

            var undiscoveredBuildings = new List<IBuildingTemplate>() {
                BuildBuildingTemplate(), BuildBuildingTemplate()
            };

            var unassociatedBuildings = new List<IBuildingTemplate>() {
                BuildBuildingTemplate(), BuildBuildingTemplate()
            };

            var discoveredTech   = BuildTech("Discovered tech",   buildings: discoveredBuildings);
            var undiscoveredTech = BuildTech("Undiscovered tech", buildings: undiscoveredBuildings);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(discoveredBuildings, techCanon.GetResearchedBuildings(civ));
        }

        [Test(Description = "GetResearchedUnits should return only unit templates " +
            "enabled by discovered techs. Units with no technology should be unavailable")]
        public void GetResearchedUnits_GetsUnitsFromDiscoveredTechs() {
            var discoveredUnits = new List<IUnitTemplate>() {
                BuildUnitTemplate(), BuildUnitTemplate()
            };

            var undiscoveredUnits = new List<IUnitTemplate>() {
                BuildUnitTemplate(), BuildUnitTemplate()
            };

            var unassociatedBuildings = new List<IUnitTemplate>() {
                BuildUnitTemplate(), BuildUnitTemplate()
            };

            var discoveredTech   = BuildTech("Discovered tech",   units: discoveredUnits);
            var undiscoveredTech = BuildTech("Undiscovered tech", units: undiscoveredUnits);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(discoveredUnits, techCanon.GetResearchedUnits(civ));
        }

        [Test(Description = "GetResearchedAbilities should include all abilities except " +
            "those enabled by undiscovered techs. This includes abilities enabled by no tech")]
        public void GetResearchedAbilities_ExcludesAbilitiesFromUndiscoveredTechs() {
            var discoveredAbilities = new List<IAbilityDefinition>() {
                BuildAbilityDefinition(), BuildAbilityDefinition()
            };

            var undiscoveredAbilities = new List<IAbilityDefinition>() {
                BuildAbilityDefinition(), BuildAbilityDefinition()
            };

            var unassociatedAbilities = new List<IAbilityDefinition>() {
                BuildAbilityDefinition(), BuildAbilityDefinition()
            };

            var availableAbilities = discoveredAbilities.Concat(unassociatedAbilities);

            var discoveredTech   = BuildTech("Discovered tech",   abilities: discoveredAbilities);
            var undiscoveredTech = BuildTech("Undiscovered tech", abilities: undiscoveredAbilities);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(availableAbilities, techCanon.GetResearchedAbilities(civ));
        }

        [Test(Description = "GetVisibleResources should include all resources except " +
            "those made visible by undiscovered techs. This includes resources made visible by no tech")]
        public void GetVisibleResources_ExcludesResourcesFromUndiscoveredTechs() {
            var discoveredResources = new List<ISpecialtyResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition()
            };

            var undiscoveredResources = new List<ISpecialtyResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition(), BuildResourceDefinition()
            };
            
            var unassociatedResources = new List<ISpecialtyResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition()
            };

            var visibleResources = discoveredResources.Concat(unassociatedResources);

            var discoveredTech   = BuildTech("Discovered tech",   resources: discoveredResources);
            var undiscoveredTech = BuildTech("Undiscovered tech", resources: undiscoveredResources);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(visibleResources, techCanon.GetResourcesVisibleToCiv(civ));
        }

        #endregion

        #region utilities

        private ITechDefinition BuildTech(
            string name,
            List<ITechDefinition> prerequisities = null,
            List<IBuildingTemplate> buildings = null,
            List<IUnitTemplate> units = null,
            List<IAbilityDefinition> abilities = null,
            List<ISpecialtyResourceDefinition> resources = null
        ){
            var mockTech = new Mock<ITechDefinition>();
            mockTech.Name = name;

            mockTech.Setup(tech => tech.Name).Returns(name);

            mockTech.Setup(tech => tech.Prerequisites)    .Returns(prerequisities != null ? prerequisities : new List<ITechDefinition>());
            mockTech.Setup(tech => tech.BuildingsEnabled) .Returns(buildings      != null ? buildings      : new List<IBuildingTemplate>());
            mockTech.Setup(tech => tech.UnitsEnabled)     .Returns(units          != null ? units          : new List<IUnitTemplate>());
            mockTech.Setup(tech => tech.AbilitiesEnabled) .Returns(abilities      != null ? abilities      : new List<IAbilityDefinition>());
            mockTech.Setup(tech => tech.RevealedResources).Returns(resources      != null ? resources      : new List<ISpecialtyResourceDefinition>());

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
            var newAbility = new Mock<IAbilityDefinition>().Object;

            AvailableAbilities.Add(newAbility);

            return newAbility;
        }

        private ISpecialtyResourceDefinition BuildResourceDefinition() {
            var newResource = new Mock<ISpecialtyResourceDefinition>().Object;

            AvailableResources.Add(newResource);

            return newResource;
        }

        #endregion

        #endregion

    }

}
