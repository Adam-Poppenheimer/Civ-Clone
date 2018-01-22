using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.Units;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Technology {

    public class TechCanon : ITechCanon {

        #region instance fields and properties

        #region from ITechCanon

        public ReadOnlyCollection<ITechDefinition> AllTechs {
            get { return _allTechs.AsReadOnly(); }
        }
        private List<ITechDefinition> _allTechs;

        #endregion

        private DictionaryOfLists<ICivilization, ITechDefinition> TechsResearchedByCiv =
            new DictionaryOfLists<ICivilization, ITechDefinition>();

        #endregion

        #region constructors

        [Inject]
        public TechCanon(
            [Inject(Id = "Available Techs")] List<ITechDefinition> availableTechs
        ){
            _allTechs = availableTechs;
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

        public IEnumerable<IImprovementTemplate> GetResearchedImprovements(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var retval = new HashSet<IImprovementTemplate>();

            foreach(var tech in GetTechsDiscoveredByCiv(civilization)) {
                foreach(var improvement in tech.ImprovementsEnabled) {
                    retval.Add(improvement);
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

        public IEnumerable<ITechDefinition> GetTechsAvailableToCiv(ICivilization civilization) {
            if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            var techsDiscovered = GetTechsDiscoveredByCiv(civilization);
            
            return AllTechs.Except(techsDiscovered).Where(delegate(ITechDefinition tech) {
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

        public bool IsImprovementResearchedForCiv(IImprovementTemplate template, ICivilization civilization) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetResearchedImprovements(civilization).Contains(template);
        }

        public bool IsTechAvailableToCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsAvailableToCiv(civilization).Contains(tech);
        }

        public bool IsTechDiscoveredByCiv(ITechDefinition tech, ICivilization civilization) {
            if(tech == null) {
                throw new ArgumentNullException("tech");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetTechsDiscoveredByCiv(civilization).Contains(tech);
        }

        public bool IsUnitResearchedForCiv(IUnitTemplate template, ICivilization civilization) {
            if(template == null) {
                throw new ArgumentNullException("template");
            }else if(civilization == null) {
                throw new ArgumentNullException("civilization");
            }

            return GetResearchedUnits(civilization).Contains(template);
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

        #endregion

        #endregion

    }

}
