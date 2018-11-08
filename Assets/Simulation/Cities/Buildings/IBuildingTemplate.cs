using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Units.Promotions;

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

        BuildingType Type { get; }

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
        YieldSummary StaticYield { get; }

        int SlotCount { get; }

        YieldSummary SlotYield { get; }

        /// <summary>
        /// The yield modifier the building provides to the civilization of the city it's in.
        /// </summary>
        YieldSummary CivilizationYieldModifier { get; }

        /// <summary>
        /// The yield modifier the building provides to the city it's in.
        /// </summary>
        YieldSummary CityYieldModifier { get; }

        int LocalHappiness { get; }

        int GlobalHappiness { get; }

        int Unhappiness { get; }

        IEnumerable<IResourceDefinition> ResourcesConsumed { get; }

        IEnumerable<IResourceYieldModificationData> ResourceYieldModifications { get; }

        IEnumerable<ICellYieldModificationData> CellYieldModifications { get; }

        float FoodStockpilePreservationBonus { get; }

        int CityCombatStrengthBonus { get; }

        int CityMaxHitpointBonus { get; }

        YieldSummary BonusYieldPerPopulation { get; }

        IEnumerable<IBuildingTemplate> PrerequisiteBuildings { get; }

        IEnumerable<IResourceDefinition> PrerequisiteResourcesNearCity { get; }

        IEnumerable<IImprovementTemplate> PrerequisiteImprovementsNearCity { get; }

        IEnumerable<IBuildingTemplate> GlobalPrerequisiteBuildings { get; }

        float LandUnitProductionBonus { get; }

        float MountedUnitProductionBonus { get; }

        bool RequiresAdjacentRiver { get; }

        bool RequiresCoastalCity { get; }

        bool ProvidesOverseaConnection { get; }

        int BonusExperience { get; }

        bool CanBeConstructed { get; }

        IEnumerable<IPromotion> GlobalPromotions { get; }

        #endregion

    }

}
