using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Technology {

    public interface ITechCanon {

        #region properties

        ReadOnlyCollection<ITechDefinition> AllTechs { get; }

        #endregion

        #region methods

        IEnumerable<IBuildingTemplate>    GetResearchedBuildings   (ICivilization civilization);
        IEnumerable<IUnitTemplate>        GetResearchedUnits       (ICivilization civilization);
        IEnumerable<IImprovementTemplate> GetResearchedImprovements(ICivilization civilization);

        bool IsBuildingResearchedForCiv   (IBuildingTemplate template,    ICivilization civilization);
        bool IsUnitResearchedForCiv       (IUnitTemplate template,        ICivilization civilization);
        bool IsImprovementResearchedForCiv(IImprovementTemplate template, ICivilization civilization);

        IEnumerable<ITechDefinition> GetTechsDiscoveredByCiv(ICivilization civilization);
        IEnumerable<ITechDefinition> GetTechsAvailableToCiv (ICivilization civilization);

        bool IsTechDiscoveredByCiv(ITechDefinition tech, ICivilization civilization);
        bool IsTechAvailableToCiv (ITechDefinition tech, ICivilization civilization);
        
        List<ITechDefinition> GetPrerequisiteChainToResearchTech(ITechDefinition tech, ICivilization civilization);

        void SetTechAsResearchedForCiv(ITechDefinition tech, ICivilization civilization);

        #endregion

    }

}
