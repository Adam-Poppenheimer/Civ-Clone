using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Technology {

    public class TechCanon : ITechCanon {

        #region instance fields and properties

        #region from ITechCanon

        public ReadOnlyCollection<ITechDefinition> AvailableTechs {
            get { return _availableTechs.AsReadOnly(); }
        }
        private List<ITechDefinition> _availableTechs;

        #endregion

        private DictionaryOfLists<ICivilization, ITechDefinition> TechsResearchedByCiv =
            new DictionaryOfLists<ICivilization, ITechDefinition>();

        private Dictionary<ICivilization, Dictionary<ITechDefinition, int>> ProgressByCivAndTech =
            new Dictionary<ICivilization, Dictionary<ITechDefinition, int>>();




        private IEnumerable<IAbilityDefinition> AvailableAbilities;

        #endregion

        #region constructors

        [Inject]
        public TechCanon(
            [Inject(Id = "Available Techs")] List<ITechDefinition> availableTechs,
            [Inject(Id = "Available Abilities")] IEnumerable<IAbilityDefinition> availableAbilities
        ){
            _availableTechs = availableTechs;

            AvailableAbilities = availableAbilities;
        }

        #endregion

        #region instance methods

        #region from ITechCanon

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
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var retval = new HashSet<IAbilityDefinition>();

            var requiresTechDict = new Dictionary<IAbilityDefinition, bool>();

            foreach(var tech in AvailableTechs) {
                if(IsTechDiscoveredByCiv(tech, civilization)) {
                    foreach(var ability in tech.AbilitiesEnabled) {
                        retval.Add(ability);
                    }
                }else {
                    foreach(var ability in tech.AbilitiesEnabled) {
                        requiresTechDict[ability] = true;
                    }
                }                
            }

            foreach(var ability in AvailableAbilities) {
                if(!requiresTechDict.ContainsKey(ability)) {
                    retval.Add(ability);
                }
            }

            return retval;
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

        public IEnumerable<ITechDefinition> GetTechsDiscoveredByCiv(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return TechsResearchedByCiv[civilization];
        }

        public bool IsBuildingResearchedForCiv(IBuildingTemplate template, ICivilization civilization) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetResearchedBuildings(civilization).Contains(template);
        }

        public bool IsTechAvailableToCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsAvailableToCiv(civilization).Contains(tech);
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

        public bool IsTechDiscoveredByCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsDiscoveredByCiv(civilization).Contains(tech);
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

        #endregion

        #endregion

    }

}
