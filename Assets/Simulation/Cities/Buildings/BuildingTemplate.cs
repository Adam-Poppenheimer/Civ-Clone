﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingTemplate.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Building")]
    public class BuildingTemplate : ScriptableObject, IBuildingTemplate {

        #region instance fields and properties

        #region from IBuildingTemplate

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        public BuildingType Type {
            get { return _type; }
        }
        [SerializeField] private BuildingType _type = BuildingType.Normal;

        /// <inheritdoc/>
        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost;

        /// <inheritdoc/>
        public int Maintenance {
            get { return _maintenance; }
        }
        [SerializeField] private int _maintenance;

        public int SlotCount {
            get { return _slotCount; }
        }
        [SerializeField] private int _slotCount;
        
        public YieldSummary SlotYield {
            get { return _slotYield; }
        }
        [SerializeField] private YieldSummary _slotYield;

        /// <inheritdoc/>
        public YieldSummary StaticYield {
            get { return _staticYield; }
        }
        [SerializeField] private YieldSummary _staticYield;

        /// <inheritdoc/>
        public YieldSummary CivilizationYieldModifier {
            get { return _civilizationYieldModifier; }
        }
        [SerializeField] private YieldSummary _civilizationYieldModifier;

        /// <inheritdoc/>
        public YieldSummary CityYieldModifier {
            get { return _cityYieldModifier; }
        }
        [SerializeField] private YieldSummary _cityYieldModifier;

        public int LocalHappiness {
            get { return _localHappiness; }
        }
        [SerializeField] private int _localHappiness;

        public int GlobalHappiness {
            get { return _globalHappiness; }
        }
        [SerializeField] private int _globalHappiness;

        public int Unhappiness {
            get { return _unhappiness; }
        }
        [SerializeField] private int _unhappiness;

        public IEnumerable<IResourceDefinition> ResourcesConsumed {
            get { return _resourcesConsumed.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _resourcesConsumed;

        public IEnumerable<IResourceYieldModificationData> ResourceYieldModifications {
            get { return _resourceYieldModifications.Cast<IResourceYieldModificationData>(); }
        }
        [SerializeField] private List<ResourceYieldModificationData> _resourceYieldModifications;

        public IEnumerable<ICellYieldModificationData> CellYieldModifications {
            get { return _cellYieldModifications.Cast<ICellYieldModificationData>(); }
        }
        [SerializeField] private List<CellYieldModificationData> _cellYieldModifications;

        public float FoodStockpilePreservationBonus { 
            get { return _foodStockpilePreservationBonus; }
        }
        [SerializeField] private float _foodStockpilePreservationBonus;

        public int CityCombatStrengthBonus { 
            get { return _cityCombatStrengthBonus; }
        }
        [SerializeField] private int _cityCombatStrengthBonus;

        public int CityMaxHitpointBonus { 
            get { return _cityMaxHitpointBonus; }
        }
        [SerializeField] private int _cityMaxHitpointBonus;

        public YieldSummary BonusYieldPerPopulation {
            get { return _bonusYieldPerPopulation; }
        }
        [SerializeField] private YieldSummary _bonusYieldPerPopulation;

        public IEnumerable<IBuildingTemplate> PrerequisiteBuildings {
            get { return _prerequisiteBuildings.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _prerequisiteBuildings;

        public IEnumerable<IResourceDefinition> PrerequisiteResourcesNearCity {
            get { return _prerequisiteResourcesNearCity.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _prerequisiteResourcesNearCity;

        public IEnumerable<IImprovementTemplate> PrerequisiteImprovementsNearCity {
            get { return _prerequisiteImprovementsNearCity.Cast<IImprovementTemplate>(); }
        }
        [SerializeField] private List<ImprovementTemplate> _prerequisiteImprovementsNearCity;

        public IEnumerable<IBuildingTemplate> GlobalPrerequisiteBuildings {
            get { return _globalPrerequisiteBuildings.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _globalPrerequisiteBuildings;

        public float LandUnitProductionBonus {
            get { return _landUnitProductionBonus; }
        }
        [SerializeField] private float _landUnitProductionBonus;

        public float MountedUnitProductionBonus {
            get { return _mountedUnitProductionBonus; }
        }
        [SerializeField] private float _mountedUnitProductionBonus;

        public bool RequiresAdjacentRiver {
            get { return _requiresAdjacentRiver; }
        }
        [SerializeField] private bool _requiresAdjacentRiver;

        public bool RequiresCoastalCity {
            get { return _requiresCoastalCity; }
        }
        [SerializeField] private bool _requiresCoastalCity;

        public bool ProvidesOverseaConnection {
            get { return _providesOverseaConnection; }
        }
        [SerializeField] private bool _providesOverseaConnection;

        public int BonusExperience {
            get { return _bonusExperience; }
        }
        [SerializeField] private int _bonusExperience;

        public bool CanBeConstructed {
            get { return _canBeConstructed; }
        }
        [SerializeField] private bool _canBeConstructed = true;

        #endregion

        #endregion
        
    }

}
