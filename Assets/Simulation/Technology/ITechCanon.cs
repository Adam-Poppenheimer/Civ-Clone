﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Units.Abilities;

namespace Assets.Simulation.Technology {

    public interface ITechCanon {

        #region properties

        ReadOnlyCollection<ITechDefinition> AvailableTechs { get; }

        bool IgnoreResourceVisibility { get; set; }

        #endregion

        #region methods

        IEnumerable<IBuildingTemplate>            GetResearchedBuildings(ICivilization civilization);
        IEnumerable<IUnitTemplate>                GetResearchedUnits    (ICivilization civilization);
        IEnumerable<IAbilityDefinition>           GetResearchedAbilities(ICivilization civilization);
        IEnumerable<ISpecialtyResourceDefinition> GetResourcesVisibleToCiv   (ICivilization civilization);

        bool IsBuildingResearchedForCiv(IBuildingTemplate            template, ICivilization civilization);
        bool IsUnitResearchedForCiv    (IUnitTemplate                template, ICivilization civilization);
        bool IsAbilityResearchedForCiv (IAbilityDefinition           ability,  ICivilization civilization);
        bool IsResourceVisibleToCiv    (ISpecialtyResourceDefinition resource, ICivilization civilization);

        IEnumerable<ITechDefinition> GetTechsDiscoveredByCiv(ICivilization civilization);
        IEnumerable<ITechDefinition> GetTechsAvailableToCiv (ICivilization civilization);

        bool IsTechDiscoveredByCiv(ITechDefinition tech, ICivilization civilization);
        bool IsTechAvailableToCiv (ITechDefinition tech, ICivilization civilization);
        
        List<ITechDefinition> GetPrerequisiteChainToResearchTech(ITechDefinition tech, ICivilization civilization);

        int  GetProgressOnTechByCiv(ITechDefinition tech, ICivilization civilization);
        void SetProgressOnTechByCiv(ITechDefinition tech, ICivilization civilization, int newProgress);

        void SetTechAsDiscoveredForCiv  (ITechDefinition tech, ICivilization civilization);
        void SetTechAsUndiscoveredForCiv(ITechDefinition tech, ICivilization civilization);

        #endregion

    }

}
