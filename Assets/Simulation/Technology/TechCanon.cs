using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.MapResources;
using Assets.Simulation.SocialPolicies;

using UnityCustomUtilities.Extensions;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    public class TechCanon : ITechCanon {

        #region instance fields and properties

        #region from ITechCanon

        public ReadOnlyCollection<ITechDefinition> AvailableTechs {
            get { return _availableTechs.AsReadOnly(); }
        }
        private List<ITechDefinition> _availableTechs;

        public bool IgnoreResourceVisibility { get; set; }

        #endregion

        private DictionaryOfLists<ICivilization, ITechDefinition> TechsResearchedByCiv =
            new DictionaryOfLists<ICivilization, ITechDefinition>();

        private Dictionary<ICivilization, Dictionary<ITechDefinition, int>> ProgressByCivAndTech =
            new Dictionary<ICivilization, Dictionary<ITechDefinition, int>>();




        private IEnumerable<IAbilityDefinition>   AvailableAbilities;
        private IEnumerable<IResourceDefinition>  AvailableResources;
        private IEnumerable<IImprovementTemplate> AvailableImprovements;

        #endregion

        #region constructors

        [Inject]
        public TechCanon(
            [Inject(Id = "Available Techs")] List<ITechDefinition> availableTechs,
            [Inject(Id = "Available Abilities")] IEnumerable<IAbilityDefinition> availableAbilities,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources,
            [Inject(Id = "Available Improvement Templates")] IEnumerable<IImprovementTemplate> availableImprovements,
            CivilizationSignals civSignals
        ){
            _availableTechs       = availableTechs;
            AvailableAbilities    = availableAbilities;
            AvailableResources    = availableResources;
            AvailableImprovements = availableImprovements;

            civSignals.CivilizationBeingDestroyedSignal.Subscribe(OnCivilizationBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from ITechCanon

        public IEnumerable<IImprovementTemplate> GetAvailableImprovementsFromTechs(IEnumerable<ITechDefinition> techs) {
            var excludedTechs = AvailableTechs.Except(techs);
            var excludedImprovements = excludedTechs.SelectMany(tech => tech.ImprovementsEnabled);

            return AvailableImprovements.Except(excludedImprovements);
        }

        public IEnumerable<IBuildingTemplate> GetAvailableBuildingsFromTechs(IEnumerable<ITechDefinition> techs) {
            return techs.SelectMany(tech => tech.BuildingsEnabled);
        }

        public IEnumerable<IResourceDefinition> GetVisibleResourcesFromTechs(IEnumerable<ITechDefinition> techs) {
            var excludedTechs = AvailableTechs.Except(techs);
            var excludedResources = excludedTechs.SelectMany(tech => tech.RevealedResources);

            return AvailableResources.Except(excludedResources);
        }

        public List<ITechDefinition> GetPrerequisiteChainToResearchTech(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var prerequisiteChain = new List<ITechDefinition>();

            if(!IsTechDiscoveredByCiv(tech, civilization)) {
                GetPrerequisiteChainToResearchTech_Recurs(tech, civilization, prerequisiteChain);
            }
            
            return prerequisiteChain;
        }

        private void GetPrerequisiteChainToResearchTech_Recurs(
            ITechDefinition tech, ICivilization civilization,
            List<ITechDefinition> prerequisiteChain
        ) {
            if(prerequisiteChain.Contains(tech)) {
                return;
            }

            foreach(var prerequisite in tech.Prerequisites) {
                if(prerequisiteChain.Contains(prerequisite)) {
                    continue;

                } if(IsTechAvailableToCiv(prerequisite, civilization)) {
                    prerequisiteChain.Add(prerequisite);

                }else if(!IsTechDiscoveredByCiv(prerequisite, civilization)) {
                    GetPrerequisiteChainToResearchTech_Recurs(prerequisite, civilization, prerequisiteChain);
                }
            }

            prerequisiteChain.Add(tech);
        }

        public IEnumerable<IBuildingTemplate> GetResearchedBuildings(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var retval = new HashSet<IBuildingTemplate>();

            foreach(var tech in GetTechsDiscoveredByCiv(civilization)) {
                foreach(var building in tech.BuildingsEnabled) {
                    retval.Add(building);
                }
            }

            return retval;
        }

        public IEnumerable<IUnitTemplate> GetResearchedUnits(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var retval = new HashSet<IUnitTemplate>();

            foreach(var tech in GetTechsDiscoveredByCiv(civilization)) {
                foreach(var unit in tech.UnitsEnabled) {
                    retval.Add(unit);
                }
            }

            return retval;
        }

        public IEnumerable<IAbilityDefinition> GetResearchedAbilities(ICivilization civilization) {
            var retval = new HashSet<IAbilityDefinition>(AvailableAbilities);

            foreach(var tech in AvailableTechs) {
                if(!IsTechDiscoveredByCiv(tech, civilization)) {
                    foreach(var ability in tech.AbilitiesEnabled) {
                        retval.Remove(ability);
                    }
                }
            }

            return retval;
        }

        public IEnumerable<IResourceDefinition> GetResourcesVisibleToCiv(ICivilization civilization) {
            var retval = new HashSet<IResourceDefinition>(AvailableResources);

            if(!IgnoreResourceVisibility) {
                foreach(var tech in AvailableTechs) {
                    if(!IsTechDiscoveredByCiv(tech, civilization)) {
                        foreach(var resource in tech.RevealedResources) {
                            retval.Remove(resource);
                        }
                    }
                }
            }

            return retval;
        }

        public IEnumerable<IPolicyTreeDefinition> GetResearchedPolicyTrees(ICivilization civilization) {
            return new HashSet<IPolicyTreeDefinition>(
                GetTechsDiscoveredByCiv(civilization).SelectMany(tech => tech.PolicyTreesEnabled)
            );
        }

        public bool IsBuildingResearchedForCiv(IBuildingTemplate template, ICivilization civilization) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetResearchedBuildings(civilization).Contains(template);
        }

        public bool IsUnitResearchedForCiv(IUnitTemplate template, ICivilization civilization) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetResearchedUnits(civilization).Contains(template);
        }

        public bool IsAbilityResearchedForCiv(IAbilityDefinition ability, ICivilization civilization) {
            return GetResearchedAbilities(civilization).Contains(ability);
        }

        public bool IsResourceVisibleToCiv(IResourceDefinition resource, ICivilization civilization) {
            return GetResourcesVisibleToCiv(civilization).Contains(resource);
        }

        public IEnumerable<ITechDefinition> GetTechsDiscoveredByCiv(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return TechsResearchedByCiv[civilization];
        }

        public IEnumerable<ITechDefinition> GetTechsAvailableToCiv(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var techsDiscovered = GetTechsDiscoveredByCiv(civilization);
            
            return AvailableTechs.Except(techsDiscovered).Where(delegate(ITechDefinition tech) {
                foreach(var prerequisite in tech.Prerequisites) {
                    if(!techsDiscovered.Contains(prerequisite)) {
                        return false;
                    }
                }
                return true;
            });

        }

        public bool IsTechDiscoveredByCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsDiscoveredByCiv(civilization).Contains(tech);
        }

        public bool IsTechAvailableToCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsAvailableToCiv(civilization).Contains(tech);
        }

        public int GetProgressOnTechByCiv(ITechDefinition tech, ICivilization civilization) {
            if(!ProgressByCivAndTech.ContainsKey(civilization)) {
                ProgressByCivAndTech[civilization] = new Dictionary<ITechDefinition, int>();
            }

            var civProgressDict = ProgressByCivAndTech[civilization];

            if(!civProgressDict.ContainsKey(tech)) {
                civProgressDict[tech] = 0;
            }

            return civProgressDict[tech];
        }

        public void SetProgressOnTechByCiv(ITechDefinition tech, ICivilization civilization, int newProgress) {
            if(!ProgressByCivAndTech.ContainsKey(civilization)) {
                ProgressByCivAndTech[civilization] = new Dictionary<ITechDefinition, int>();
            }

            var civProgressDict = ProgressByCivAndTech[civilization];

            civProgressDict[tech] = newProgress;
        }

        public void SetTechAsDiscoveredForCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            if(!IsTechDiscoveredByCiv(tech, civilization)) {
                TechsResearchedByCiv[civilization].Add(tech);
            }
        }

        public void SetTechAsUndiscoveredForCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            TechsResearchedByCiv[civilization].Remove(tech);
        }

        public IEnumerable<ITechDefinition> GetTechsOfEra(TechnologyEra era) {
            return AvailableTechs.Where(tech => tech.Era == era);
        }

        public TechnologyEra GetEraOfCiv(ICivilization civilization) {
            return (TechnologyEra)GetTechsDiscoveredByCiv(civilization).Select(tech => (int)tech.Era).Max();
        }

        #endregion

        private void OnCivilizationBeingDestroyed(ICivilization civ) {
            TechsResearchedByCiv.RemoveList(civ);
            ProgressByCivAndTech.Remove(civ);
        }

        #endregion

    }

}
