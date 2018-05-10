using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The base interface for all building templates, which define what buildings are
    /// available for construction in various cities.
    /// </summary>
    public interface IBuildingTemplate {

        #region properties

        /// <summary>
        /// The name of the template.
        /// </summary>
        string name { get; }
        
        string Description { get; }

        Sprite Icon { get; }

        /// <summary>
        /// The production required to construct the template.
        /// </summary>
        int ProductionCost { get; }

        /// <summary>
        /// The maintenance, in gold per turn, of the building.
        /// </summary>
        int Maintenance { get; }

        /// <summary>
        /// The yield the building provides to its city just by existing.
        /// </summary>
        ResourceSummary StaticYield { get; }

        int SlotCount { get; }

        ResourceSummary SlotYield { get; }

        /// <summary>
        /// The yield modifier the building provides to the civilization of the city it's in.
        /// </summary>
        ResourceSummary CivilizationYieldModifier { get; }

        /// <summary>
        /// The yield modifier the building provides to the city it's in.
        /// </summary>
        ResourceSummary CityYieldModifier { get; }

        int LocalHappiness { get; }

        int GlobalHappiness { get; }

        int Unhappiness { get; }

        IEnumerable<ISpecialtyResourceDefinition> ResourcesConsumed { get; }

        IEnumerable<IResourceYieldModificationData> ResourceYieldModifications { get; }

        IEnumerable<ICellYieldModificationData> CellYieldModifications { get; }

        float FoodStockpilePreservationBonus { get; }

        int CityCombatStrengthBonus { get; }

        int CityMaxHitpointBonus { get; }

        ResourceSummary BonusYieldPerPopulation { get; }

        IEnumerable<IBuildingTemplate> PrerequisiteBuildings { get; }

        IEnumerable<ISpecialtyResourceDefinition> PrerequisiteResourcesNearCity { get; }

        IEnumerable<IImprovementTemplate> PrerequisiteImprovementsNearCity { get; }

        float LandUnitProductionBonus { get; }

        float MountedUnitProductionBonus { get; }

        bool RequiresAdjacentRiver { get; }

        bool RequiresCoastalCity { get; }

        bool ProvidesOverseaConnection { get; }

        int BonusExperience { get; }

        #endregion

    }

}
