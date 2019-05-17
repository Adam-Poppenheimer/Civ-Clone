using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities.Buildings {

    /// <summary>
    /// The standard implementation for IBuildingTemplate.
    /// </summary>
    [CreateAssetMenu(menuName = "Civ Clone/Cities/Building")]
    public class BuildingTemplate : ScriptableObject, IBuildingTemplate {

        #region instance fields and properties

        #region from IBuildingTemplate

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description = null;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon = null;

        public BuildingType Type {
            get { return _type; }
        }
        [SerializeField] private BuildingType _type = BuildingType.Normal;

        /// <inheritdoc/>
        public int ProductionCost {
            get { return _productionCost; }
        }
        [SerializeField] private int _productionCost = 0;

        /// <inheritdoc/>
        public int Maintenance {
            get { return _maintenance; }
        }
        [SerializeField] private int _maintenance = 0;

        public int SpecialistCount {
            get { return _specialistCount; }
        }
        [SerializeField] private int _specialistCount = 0;
        
        public ISpecialistDefinition Specialist {
            get { return _specialist; }
        }
        [SerializeField] private SpecialistDefinition _specialist = null;

        /// <inheritdoc/>
        public YieldSummary StaticYield {
            get { return _staticYield; }
        }
        [SerializeField] private YieldSummary _staticYield = YieldSummary.Empty;

        /// <inheritdoc/>
        public YieldSummary CivilizationYieldModifier {
            get { return _civilizationYieldModifier; }
        }
        [SerializeField] private YieldSummary _civilizationYieldModifier = YieldSummary.Empty;

        /// <inheritdoc/>
        public YieldSummary CityYieldModifier {
            get { return _cityYieldModifier; }
        }
        [SerializeField] private YieldSummary _cityYieldModifier = YieldSummary.Empty;

        public int LocalHappiness {
            get { return _localHappiness; }
        }
        [SerializeField] private int _localHappiness = 0;

        public int GlobalHappiness {
            get { return _globalHappiness; }
        }
        [SerializeField] private int _globalHappiness = 0;

        public int Unhappiness {
            get { return _unhappiness; }
        }
        [SerializeField] private int _unhappiness = 0;

        public IEnumerable<IResourceDefinition> ResourcesConsumed {
            get { return _resourcesConsumed.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _resourcesConsumed = null;

        public IEnumerable<IResourceYieldModificationData> ResourceYieldModifications {
            get { return _resourceYieldModifications.Cast<IResourceYieldModificationData>(); }
        }
        [SerializeField] private List<ResourceYieldModificationData> _resourceYieldModifications = null;

        public IEnumerable<ICellYieldModificationData> CellYieldModifications {
            get {
                if(_castCellYieldModifications == null) {
                    _castCellYieldModifications = _cellYieldModifications.Cast<ICellYieldModificationData>();
                }

                return _castCellYieldModifications;
            }
        }
        private IEnumerable<ICellYieldModificationData> _castCellYieldModifications;
        [SerializeField] private List<CellYieldModificationData> _cellYieldModifications = null;

        public float FoodStockpilePreservationBonus { 
            get { return _foodStockpilePreservationBonus; }
        }
        [SerializeField] private float _foodStockpilePreservationBonus = 0f;

        public int CityCombatStrengthBonus { 
            get { return _cityCombatStrengthBonus; }
        }
        [SerializeField] private int _cityCombatStrengthBonus = 0;

        public int CityMaxHitpointBonus { 
            get { return _cityMaxHitpointBonus; }
        }
        [SerializeField] private int _cityMaxHitpointBonus = 0;

        public YieldSummary BonusYieldPerPopulation {
            get { return _bonusYieldPerPopulation; }
        }
        [SerializeField] private YieldSummary _bonusYieldPerPopulation = YieldSummary.Empty;

        public IEnumerable<IBuildingTemplate> PrerequisiteBuildings {
            get { return _prerequisiteBuildings.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _prerequisiteBuildings = null;

        public IEnumerable<IResourceDefinition> PrerequisiteResourcesNearCity {
            get { return _prerequisiteResourcesNearCity.Cast<IResourceDefinition>(); }
        }
        [SerializeField] private List<ResourceDefinition> _prerequisiteResourcesNearCity = null;

        public IEnumerable<IImprovementTemplate> PrerequisiteImprovementsNearCity {
            get { return _prerequisiteImprovementsNearCity.Cast<IImprovementTemplate>(); }
        }
        [SerializeField] private List<ImprovementTemplate> _prerequisiteImprovementsNearCity = null;

        public IEnumerable<IBuildingTemplate> GlobalPrerequisiteBuildings {
            get { return _globalPrerequisiteBuildings.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _globalPrerequisiteBuildings = null;

        public bool RequiresAdjacentRiver {
            get { return _requiresAdjacentRiver; }
        }
        [SerializeField] private bool _requiresAdjacentRiver = false;

        public bool RequiresCoastalCity {
            get { return _requiresCoastalCity; }
        }
        [SerializeField] private bool _requiresCoastalCity = false;

        public bool ProvidesOverseaConnection {
            get { return _providesOverseaConnection; }
        }
        [SerializeField] private bool _providesOverseaConnection = false;

        public int BonusExperience {
            get { return _bonusExperience; }
        }
        [SerializeField] private int _bonusExperience = 0;

        public bool CanBeConstructed {
            get { return _canBeConstructed; }
        }
        [SerializeField] private bool _canBeConstructed = true;

        public IEnumerable<IPromotion> GlobalPromotions {
            get { return _globalPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _globalPromotions = null;

        public IEnumerable<IPromotion> LocalPromotions {
            get { return _localPromotions.Cast<IPromotion>(); }
        }
        [SerializeField] private List<Promotion> _localPromotions = null;

        public bool ProvidesFreeTech {
            get { return _providesFreeTech; }
        }
        [SerializeField] private bool _providesFreeTech = false;

        public IProductionModifier ProductionModifier {
            get { return _productionModifier; }
        }
        [SerializeField] private ProductionModifier _productionModifier = null;

        public IEnumerable<IUnitTemplate> FreeUnits {
            get { return _freeUnits.Cast<IUnitTemplate>(); }
        }
        [SerializeField] private List<UnitTemplate> _freeUnits = null;

        public IEnumerable<IBuildingTemplate> FreeBuildings {
            get { return _freeBuildings.Cast<IBuildingTemplate>(); }
        }
        [SerializeField] private List<BuildingTemplate> _freeBuildings = null;

        public float LocalBorderExpansionModifier {
            get { return _localBorderExpansionModifier; }
        }
        [SerializeField] private float _localBorderExpansionModifier = 0f;

        public float GlobalBorderExpansionModifier {
            get { return _globalBorderExpansionModifier; }
        }
        [SerializeField] private float _globalBorderExpansionModifier = 0f;

        public int FreeGreatPeople {
            get { return _freeGreatPeople; }
        }
        [SerializeField] private int _freeGreatPeople = 0;

        public float LocalGrowthModifier {
            get { return _localGrowthModifier; }
        }
        [SerializeField] private float _localGrowthModifier = 0f;

        public float GlobalGrowthModifier {
            get { return _globalGrowthModifier; }
        }
        [SerializeField] private float _globalGrowthModifier = 0f;

        public float GlobalImprovementSpeedModifier {
            get { return _globalImprovementSpeedModifier; }
        }
        [SerializeField] private float _globalImprovementSpeedModifier = 0f;

        public float GreatMilitaryGainSpeedModifier {
            get { return _greatMilitaryGainSpeedModifier; }
        }
        [SerializeField] private float _greatMilitaryGainSpeedModifier = 0f;

        public float GoldenAgeLengthModifier {
            get { return _goldenAgeLengthModifier; }
        }
        [SerializeField] private float _goldenAgeLengthModifier = 0f;

        public bool StartsGoldenAge {
            get { return _startsGoldenAge; }
        }
        [SerializeField] private bool _startsGoldenAge = false;

        #endregion

        #endregion
        
    }

}
