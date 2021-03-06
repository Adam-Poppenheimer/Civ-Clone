﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Technology;
using Assets.Simulation.MapResources;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Technology {

    [TestFixture]
    public class TechCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private CivilizationSignals CivSignals;

        private List<ITechDefinition>      AvailableTechs        = new List<ITechDefinition>();
        private List<IResourceDefinition>  AvailableResources    = new List<IResourceDefinition>();
        private List<IAbilityDefinition>   AvailableAbilities    = new List<IAbilityDefinition>();
        private List<IImprovementTemplate> AvailableImprovements = new List<IImprovementTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTechs       .Clear();
            AvailableResources   .Clear();
            AvailableAbilities   .Clear();
            AvailableImprovements.Clear();

            CivSignals = new CivilizationSignals();

            Container.Bind<List<ITechDefinition>>            ().WithId("Available Techs")                .FromInstance(AvailableTechs);
            Container.Bind<IEnumerable<IResourceDefinition>> ().WithId("Available Resources")            .FromInstance(AvailableResources);
            Container.Bind<IEnumerable<IAbilityDefinition>>  ().WithId("Available Abilities")            .FromInstance(AvailableAbilities);
            Container.Bind<IEnumerable<IImprovementTemplate>>().WithId("Available Improvement Templates").FromInstance(AvailableImprovements);

            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<TechCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetAvailableImprovementsFromTechs_ReturnsAllImprovementsExceptThoseInNonArguedTechs() {
            var improvementOne   = BuildImprovementTemplate();
            var improvementTwo   = BuildImprovementTemplate();
            var improvementThree = BuildImprovementTemplate();

            var discoveredTech = BuildTech("Discovered Tech",   improvements: new List<IImprovementTemplate>() { improvementTwo });
                                 BuildTech("Undiscovered Tech", improvements: new List<IImprovementTemplate>() { improvementThree });

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                new List<IImprovementTemplate>() { improvementOne, improvementTwo },
                techCanon.GetAvailableImprovementsFromTechs(new List<ITechDefinition>() { discoveredTech })
            );
        }

        [Test]
        public void GetAvailableBuildingsFromTechs_ReturnsAllBuildingsInArguedTechs() {
                                BuildBuildingTemplate();
            var buildingTwo   = BuildBuildingTemplate();
            var buildingThree = BuildBuildingTemplate();

            var discoveredTech = BuildTech("Discovered Tech",   buildings: new List<IBuildingTemplate>() { buildingTwo });
                                 BuildTech("Undiscovered Tech", buildings: new List<IBuildingTemplate>() { buildingThree });

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                new List<IBuildingTemplate>() { buildingTwo },
                techCanon.GetAvailableBuildingsFromTechs(new List<ITechDefinition>() { discoveredTech })
            );
        }

        [Test]
        public void GetVisibleResourcesFromTechs_ReturnsAllResourcesExceptThoseInNonArguedTechs() {
            var resourceOne   = BuildResourceDefinition();
            var resourceTwo   = BuildResourceDefinition();
            var resourceThree = BuildResourceDefinition();

            var discoveredTech = BuildTech("Discovered Tech",   resources: new List<IResourceDefinition>() { resourceTwo });
                                 BuildTech("Undiscovered Tech", resources: new List<IResourceDefinition>() { resourceThree });

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                new List<IResourceDefinition>() { resourceOne, resourceTwo },
                techCanon.GetDiscoveredResourcesFromTechs(new List<ITechDefinition>() { discoveredTech })
            );
        }

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

        [Test]
        public void SetTechAsDiscoveredForCiv_ReflectedInBuildingQueries() {
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

        [Test]
        public void SetTechAsDiscoveredForCiv_ReflectedInUnitQueries() {
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

        [Test]
        public void SetTechAsDiscoveredForCiv_ReflectedInAbilityQueries() {
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

        [Test]
        public void SetTechAsDiscoveredForCiv_FiresTechDiscoveredSignal() {
            var tech = BuildTech("Tech One");

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            CivSignals.CivDiscoveredTech.Subscribe(data => {
                Assert.AreEqual(civ,  data.Item1, "Signal passed unexpected civ");
                Assert.AreEqual(tech, data.Item2, "Signal passed unexpected tech");

                Assert.Pass();
            });

            techCanon.SetTechAsDiscoveredForCiv(tech, civ);

            Assert.Fail("TechDiscovered never fired");
        }

        [Test]
        public void SetTechAsUndiscoveredForCiv_ReflectedInDiscoveryQueries() {
            var techOne = BuildTech("Tech One");
            var techTwo = BuildTech("Tech Two");

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            techCanon.SetTechAsUndiscoveredForCiv(techOne, civ);

            CollectionAssert.AreEquivalent(new ITechDefinition[]{ techTwo }, techCanon.GetTechsDiscoveredByCiv(civ));
        }

        [Test]
        public void SetTechAsUndiscoveredForCiv_ReflectedInAbiltiyQueries() {
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

            techCanon.SetTechAsUndiscoveredForCiv(techOne, civ);

            CollectionAssert.AreEquivalent(techTwo.AbilitiesEnabled, techCanon.GetResearchedAbilities(civ));
        }

        [Test]
        public void SetTechAsUndiscoveredForCiv_ReflectedInBuildingQueries() {
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

            techCanon.SetTechAsUndiscoveredForCiv(techOne, civ);

            CollectionAssert.AreEquivalent(techTwo.BuildingsEnabled, techCanon.GetResearchedBuildings(civ));
        }

        [Test]
        public void SetTechAsUndiscoveredForCiv_ReflectedInUnitQueries() {
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

            techCanon.SetTechAsUndiscoveredForCiv(techOne, civ);

            CollectionAssert.AreEquivalent(techTwo.UnitsEnabled, techCanon.GetResearchedUnits(civ));
        }

        [Test]
        public void SetTechAsUndiscoveredForCiv_FiresTechUndiscoveredSignal() {
            var techOne = BuildTech("Tech One");
            var techTwo = BuildTech("Tech Two");

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            CivSignals.CivUndiscoveredTech.Subscribe(data => {
                Assert.AreEqual(civ,     data.Item1, "Signal passed unexpected civ");
                Assert.AreEqual(techOne, data.Item2, "Signal passed unexpected tech");

                Assert.Pass();
            });

            techCanon.SetTechAsUndiscoveredForCiv(techOne, civ);

            Assert.Fail("TechDiscovered never fired");
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

            BuildBuildingTemplate();
            BuildBuildingTemplate();

            var discoveredTech   = BuildTech("Discovered tech",   buildings: discoveredBuildings);
            BuildTech("Undiscovered tech", buildings: undiscoveredBuildings);

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

            BuildUnitTemplate();
            BuildUnitTemplate();

            var discoveredTech   = BuildTech("Discovered tech",   units: discoveredUnits);
            BuildTech("Undiscovered tech", units: undiscoveredUnits);

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
            BuildTech("Undiscovered tech", abilities: undiscoveredAbilities);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(availableAbilities, techCanon.GetResearchedAbilities(civ));
        }

        [Test(Description = "GetVisibleResources should include all resources except " +
            "those made visible by undiscovered techs. This includes resources made visible by no tech")]
        public void GetVisibleResources_ExcludesResourcesFromUndiscoveredTechs() {
            var discoveredResources = new List<IResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition()
            };

            var undiscoveredResources = new List<IResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition(), BuildResourceDefinition()
            };
            
            var unassociatedResources = new List<IResourceDefinition>() {
                BuildResourceDefinition(), BuildResourceDefinition()
            };

            var visibleResources = discoveredResources.Concat(unassociatedResources);

            var discoveredTech   = BuildTech("Discovered tech",   resources: discoveredResources);
            BuildTech("Undiscovered tech", resources: undiscoveredResources);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(visibleResources, techCanon.GetResourcesVisibleToCiv(civ));
        }

        [Test]
        public void GetResearchedPolicyTrees_GetsPoliciesFromDiscoveredTechs() {
            var discoveredPolicyTrees = new List<IPolicyTreeDefinition>() {
                BuildPolicyTree(), BuildPolicyTree(), BuildPolicyTree()
            };

            var undiscoveredPolicyTrees = new List<IPolicyTreeDefinition>() {
                BuildPolicyTree(), BuildPolicyTree(), BuildPolicyTree()
            };

            var discoveredTech = BuildTech("Discovered tech", policyTrees: discoveredPolicyTrees);
            BuildTech("Undiscovered tech", policyTrees: undiscoveredPolicyTrees);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();
            techCanon.SetTechAsDiscoveredForCiv(discoveredTech, civ);

            CollectionAssert.AreEquivalent(discoveredPolicyTrees, techCanon.GetResearchedPolicyTrees(civ));
        }

        [Test]
        public void GetTechsOfEra_ReturnsOnlyTechsWithArguedEra() {
                            BuildTech("Tech One",   era: TechnologyEra.Ancient);
                            BuildTech("Tech Two",   era: TechnologyEra.Ancient);
            var techThree = BuildTech("Tech Three", era: TechnologyEra.Classical);
            var techFour  = BuildTech("Tech Four",  era: TechnologyEra.Classical);
                            BuildTech("Tech Five",  era: TechnologyEra.Medieval);
                            BuildTech("Tech Six",   era: TechnologyEra.Medieval);

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                new List<ITechDefinition>() { techThree, techFour },
                techCanon.GetTechsOfEra(TechnologyEra.Classical)
            );
        }

        [Test]
        public void GetTechsOfPreviousEras_AndEraHasNoPreviousEra_ReturnsEmptySet() {
            BuildTech("Tech One", era: TechnologyEra.Ancient);
            BuildTech("Tech Two", era: TechnologyEra.Ancient);

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.IsEmpty(techCanon.GetTechsOfPreviousEras(TechnologyEra.Ancient));
        }

        [Test]
        public void GetTechsOfPreviousEras_ReturnsAllTechsFromPreviousEra() {
            var ancientTechs = new List<ITechDefinition>() {
                BuildTech("Tech One", era: TechnologyEra.Ancient),
                BuildTech("Tech Two", era: TechnologyEra.Ancient),
            };

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                ancientTechs, techCanon.GetTechsOfPreviousEras(TechnologyEra.Classical)
            );
        }

        [Test]
        public void GetTechsofPreviousEras_ReturnsAllTechsFromErasBeforeThePreviousEra() {
            var ancientTechs = new List<ITechDefinition>() {
                BuildTech("Tech One", era: TechnologyEra.Ancient),
                BuildTech("Tech Two", era: TechnologyEra.Ancient),
            };

            var classicalTechs = new List<ITechDefinition>() {
                BuildTech("Tech Three", era: TechnologyEra.Classical),
                BuildTech("Tech Four",  era: TechnologyEra.Classical),
            };

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(
                ancientTechs.Concat(classicalTechs), techCanon.GetTechsOfPreviousEras(TechnologyEra.Medieval)
            );
        }

        [Test]
        public void GetEntryTechsOfEra_AndEraHasPreviousEra_ReturnsOnlyTechsOfEra_WithPrereqsFromThePreviousEra() {
            var ancientTechs = new List<ITechDefinition>() {
                BuildTech("Tech One", era: TechnologyEra.Ancient),
                BuildTech("Tech Two", era: TechnologyEra.Ancient),
            };

            BuildTech("Tech Three", prerequisities: new List<ITechDefinition>());

            var entryTechs = new List<ITechDefinition>() {                
                BuildTech("Tech Four", era: TechnologyEra.Classical, prerequisities: new List<ITechDefinition>() { ancientTechs[0] }),
                BuildTech("Tech Five", era: TechnologyEra.Classical, prerequisities: new List<ITechDefinition>() { ancientTechs[1] }),
                BuildTech("Tech Six",  era: TechnologyEra.Classical, prerequisities: new List<ITechDefinition>() { ancientTechs[0], ancientTechs[1] })
            };

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(entryTechs, techCanon.GetEntryTechsOfEra(TechnologyEra.Classical));
        }

        [Test]
        public void GetEntryTechsOfEra_AndEraHasNoPreviousEra_ReturnsOnlyTechsOfEra_WithNoPrerequisites() {
            var entryTechs = new List<ITechDefinition>() {
                BuildTech("Tech One", era: TechnologyEra.Ancient),
                BuildTech("Tech Two", era: TechnologyEra.Ancient),
            };

            BuildTech("Tech Four", era: TechnologyEra.Ancient, prerequisities: new List<ITechDefinition>() { entryTechs[0] });
            BuildTech("Tech Five", era: TechnologyEra.Ancient, prerequisities: new List<ITechDefinition>() { entryTechs[1] });
            BuildTech("Tech Six",  era: TechnologyEra.Ancient, prerequisities: new List<ITechDefinition>() { entryTechs[0], entryTechs[1] });

            var techCanon = Container.Resolve<TechCanon>();

            CollectionAssert.AreEquivalent(entryTechs, techCanon.GetEntryTechsOfEra(TechnologyEra.Ancient));
        }

        [Test]
        public void GetEraOfCiv_GetsMostAdvancedEraAmongDiscoveredTechs() {
            var techOne = BuildTech("Tech One",   era: TechnologyEra.Ancient);
            var techTwo = BuildTech("Tech Two",   era: TechnologyEra.Classical);
                          BuildTech("Tech Three", era: TechnologyEra.Medieval);

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.SetTechAsDiscoveredForCiv(techOne, civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo, civ);

            Assert.AreEqual(TechnologyEra.Classical, techCanon.GetEraOfCiv(civ));
        }

        [Test]
        public void GetDiscoveredPostrequisiteTechs_GetsAllDiscoveredTechsWhosePrereqChainContainsTech() {
            var techOne   = BuildTech("Tech One");
            var techTwo   = BuildTech("Tech Two",   prerequisities: new List<ITechDefinition>() { techOne            });
            var techThree = BuildTech("Tech Three", prerequisities: new List<ITechDefinition>() { techTwo            });
            var techFour  = BuildTech("Tech Four",  prerequisities: new List<ITechDefinition>() { techTwo, techThree });
                            BuildTech("Tech Five",  prerequisities: new List<ITechDefinition>() { techOne            });

            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.SetTechAsDiscoveredForCiv(techOne,   civ);
            techCanon.SetTechAsDiscoveredForCiv(techTwo,   civ);
            techCanon.SetTechAsDiscoveredForCiv(techThree, civ);
            techCanon.SetTechAsDiscoveredForCiv(techFour,  civ);

            CollectionAssert.AreEquivalent(
                new List<ITechDefinition>() { techTwo, techThree, techFour },
                techCanon.GetDiscoveredPostrequisiteTechs(techOne, civ)
            );
        }

        [Test]
        public void AddFreeTechToCiv_ValueReflectedInGetFreeTechsForCiv() {
            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.AddFreeTechToCiv(civ);

            Assert.AreEqual(1, techCanon.GetFreeTechsForCiv(civ));
        }

        [Test]
        public void RemoveFreeTechFromCiv_ValueReflectedInGetFreeTechsForCiv() {
            var civ = BuildCivilization();

            var techCanon = Container.Resolve<TechCanon>();

            techCanon.RemoveFreeTechFromCiv(civ);

            Assert.AreEqual(-1, techCanon.GetFreeTechsForCiv(civ));
        }

        #endregion

        #region utilities

        private ITechDefinition BuildTech(
            string name,
            List<ITechDefinition>       prerequisities = null,
            List<IBuildingTemplate>     buildings      = null,
            List<IUnitTemplate>         units          = null,
            List<IAbilityDefinition>    abilities      = null,
            List<IResourceDefinition>   resources      = null,
            List<IPolicyTreeDefinition> policyTrees    = null,
            List<IImprovementTemplate>  improvements   = null,
            TechnologyEra               era            = TechnologyEra.Ancient
        ){
            var mockTech = new Mock<ITechDefinition>();
            mockTech.Name = name;

            mockTech.Setup(tech => tech.Name).Returns(name);
            mockTech.Setup(tech => tech.Era) .Returns(era);

            mockTech.Setup(tech => tech.Prerequisites)      .Returns(prerequisities   != null ? prerequisities : new List<ITechDefinition>());
            mockTech.Setup(tech => tech.BuildingsEnabled)   .Returns(buildings        != null ? buildings      : new List<IBuildingTemplate>());
            mockTech.Setup(tech => tech.UnitsEnabled)       .Returns(units            != null ? units          : new List<IUnitTemplate>());
            mockTech.Setup(tech => tech.AbilitiesEnabled)   .Returns(abilities        != null ? abilities      : new List<IAbilityDefinition>());
            mockTech.Setup(tech => tech.RevealedResources)  .Returns(resources        != null ? resources      : new List<IResourceDefinition>());
            mockTech.Setup(tech => tech.PolicyTreesEnabled) .Returns(policyTrees      != null ? policyTrees    : new List<IPolicyTreeDefinition>());
            mockTech.Setup(tech => tech.ImprovementsEnabled).Returns(improvements     != null ? improvements   : new List<IImprovementTemplate>());            

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

        private IResourceDefinition BuildResourceDefinition() {
            var newResource = new Mock<IResourceDefinition>().Object;

            AvailableResources.Add(newResource);

            return newResource;
        }

        private IPolicyTreeDefinition BuildPolicyTree() {
            return new Mock<IPolicyTreeDefinition>().Object;
        }

        private IImprovementTemplate BuildImprovementTemplate() {
            var newImprovement = new Mock<IImprovementTemplate>().Object;

            AvailableImprovements.Add(newImprovement);

            return newImprovement;
        }

        #endregion

        #endregion

    }

}
