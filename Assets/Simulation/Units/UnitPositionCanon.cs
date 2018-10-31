using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class UnitPositionCanon : PossessionRelationship<IHexCell, IUnit>, IUnitPositionCanon {

        #region instance fields and properties

        private UnitSignals                                   Signals;
        private IHexMapSimulationConfig                       HexSimulationConfig;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitPositionCanon(
            UnitSignals signals, IHexMapSimulationConfig hexSimulationConfig,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            HexCellSignals cellSignals
        ){
            Signals             = signals;
            HexSimulationConfig = hexSimulationConfig;
            UnitPossessionCanon = unitPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            CityPossessionCanon = cityPossessionCanon;

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IUnit>

        protected override bool IsPossessionValid(IUnit unit, IHexCell location) {
             return CanPlaceUnitAtLocation(unit, location, false);
        }

        protected override void DoOnPossessionBroken(IUnit possession, IHexCell oldOwner) {
            Signals.LeftLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, oldOwner));
        }

        protected override void DoOnPossessionEstablished(IUnit possession, IHexCell newOwner) {
            if(newOwner == null) {
                return;
            }

            Signals.EnteredLocationSignal.OnNext(new Tuple<IUnit, IHexCell>(possession, newOwner));
        }

        #endregion

        #region from IUnitPositionCanon

        public bool CanPlaceUnitAtLocation(IUnit unit, IHexCell location, bool isMeleeAttacking) {
            if(location == null) {
                return true;
            }

            isMeleeAttacking &= unit.Type != UnitType.Civilian;

            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            if(IsCellImpassableFor(unit.MovementSummary, location, unitOwner, isMeleeAttacking)) {
                return false;

            }else if(CellHasDomesticUnits(location, unitOwner)) {
                return !LocationHasUnitBlockingType(location, unit.Type);

            }else if(CellHasForeignUnits(location, unitOwner)) {
                return isMeleeAttacking;

            }else {
                return true;
            }
        }

        public bool CanPlaceUnitTemplateAtLocation(IUnitTemplate template, IHexCell location, ICivilization owner) {
            if(location == null) {
                return true;
            }

            if(IsCellImpassableFor(template.MovementSummary, location, owner, false)) {
                return false;

            }else if(CellHasDomesticUnits(location, owner)) {
                return !LocationHasUnitBlockingType(location, template.Type);

            }else if(CellHasForeignUnits(location, owner)) {
                return false;

            }else {
                return true;
            }
        }

        public float GetTraversalCostForUnit(IUnit unit, IHexCell currentCell, IHexCell nextCell, bool isMeleeAttacking) {
            if(!CanPlaceUnitAtLocation(unit, nextCell, isMeleeAttacking)) {
                return -1f;

            }else if(HasCityDomesticToUnit(nextCell, unit)) {
                return HexSimulationConfig.CityMoveCost;

            }else {
                return GetTraversalCost(unit, currentCell, nextCell);
            }
        }

        #endregion

        private float GetTraversalCost(IUnit unit, IHexCell currentCell, IHexCell nextCell) {
            int moveCost = HexSimulationConfig.GetBaseMoveCostOfTerrain(nextCell.Terrain);

            if(currentCell.HasRoads && nextCell.HasRoads) {
                return moveCost * HexSimulationConfig.RoadMoveCostMultiplier;

            }else if(
                unit.MovementSummary.DoesShapeConsumeFullMovement     (nextCell.Shape) ||
                unit.MovementSummary.DoesVegetationConsumeFullMovement(nextCell.Vegetation)
            ) {
                return unit.MaxMovement;

            }else if(
                !unit.MovementSummary.IsCostIgnoredOnTerrain   (nextCell.Terrain)    &&
                !unit.MovementSummary.IsCostIgnoredOnShape     (nextCell.Shape)      &&
                !unit.MovementSummary.IsCostIgnoredOnVegetation(nextCell.Vegetation)
            ) {
                moveCost += HexSimulationConfig.GetBaseMoveCostOfVegetation(nextCell.Vegetation);
                moveCost += HexSimulationConfig.GetBaseMoveCostOfShape     (nextCell.Shape);
                moveCost += HexSimulationConfig.GetBaseMoveCostOfFeature   (nextCell.Feature);
            }

            return moveCost;
        }

        private bool IsCellImpassableFor(
            IUnitMovementSummary movementSummary, IHexCell cell, ICivilization domesticCiv, bool isMeleeAttacking
        ) {
            if(HasCityOfOwner(cell, domesticCiv)) {
                return false;
            }

            if(HasForeignCity(cell, domesticCiv)) {
                return !isMeleeAttacking;
            }

            if(!cell.Terrain.IsWater() && !movementSummary.CanTraverseLand) {
                return true;

            }else if(cell.Terrain == CellTerrain.DeepWater && !movementSummary.CanTraverseDeepWater) {
                return true;

            }else if(cell.Terrain.IsWater() && cell.Terrain != CellTerrain.DeepWater && !movementSummary.CanTraverseShallowWater) {
                return true;
            }

            if(HexSimulationConfig.GetBaseMoveCostOfVegetation(cell.Vegetation) == -1) {
                return true;
            }

            if(HexSimulationConfig.GetBaseMoveCostOfShape(cell.Shape) == -1) {
                return true;
            }

            if(HexSimulationConfig.GetBaseMoveCostOfFeature(cell.Feature) == -1) {
                return true;
            }

            return false;
        }

        private bool CellHasDomesticUnits(IHexCell cell, ICivilization domesticCiv) {
            return GetPossessionsOfOwner(cell).Where(
                unit => UnitPossessionCanon.GetOwnerOfPossession(unit) == domesticCiv
            ).Any();
        }

        private bool CellHasForeignUnits(IHexCell cell, ICivilization domesticCiv) {
            return GetPossessionsOfOwner(cell).Where(
                unit => UnitPossessionCanon.GetOwnerOfPossession(unit) != domesticCiv
            ).Any();
        }

        private bool HasCityOfOwner(IHexCell cell, ICivilization owner) {
            return CityLocationCanon.GetPossessionsOfOwner(cell).Any(
                city => CityPossessionCanon.GetOwnerOfPossession(city) == owner
            );
        }

        private bool HasCityDomesticToUnit(IHexCell cell, IUnit unit) {
            var cityAtNext = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            var ownerOf = UnitPossessionCanon.GetOwnerOfPossession(unit);

            return cityAtNext != null && (ownerOf == CityPossessionCanon.GetOwnerOfPossession(cityAtNext));
        }

        private bool HasForeignCity(IHexCell cell, ICivilization domesticCiv) {
            return CityLocationCanon.GetPossessionsOfOwner(cell).Any(
                city => CityPossessionCanon.GetOwnerOfPossession(city) != domesticCiv
            );
        }

        private bool LocationHasUnitBlockingType(IHexCell location, UnitType type) {
            return GetPossessionsOfOwner(location).Where(unit => type.HasSameSupertypeAs(unit.Type)).Count() > 0;
        }

        #endregion

    }

}
