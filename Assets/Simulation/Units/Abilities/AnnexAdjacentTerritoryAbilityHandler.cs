using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Abilities {

    public class AnnexAdjacentTerritoryAbilityHandler : IAbilityHandler {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<ICity, IHexCell>      CellPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private ICivilizationTerritoryLogic                   CivTerritoryLogic;
        private IHexGrid                                      Grid;
        private IUnitPositionCanon                            UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public AnnexAdjacentTerritoryAbilityHandler(
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IHexCell>      cellPossessionCanon,
            IPossessionRelationship<IHexCell, ICity>      cityLocationCanon,
            ICivilizationTerritoryLogic civTerritoryLogic, IHexGrid grid,
            IUnitPositionCanon unitPositionCanon
        ) {
            UnitPossessionCanon = unitPossessionCanon;
            CityPossessionCanon = cityPossessionCanon;
            CellPossessionCanon = cellPossessionCanon;
            CityLocationCanon   = cityLocationCanon;
            CivTerritoryLogic   = civTerritoryLogic;
            Grid                = grid;
            UnitPositionCanon   = unitPositionCanon;
        }

        #endregion 

        #region instance methods

        #region from IAbilityHandler

        public bool CanHandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

            var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

            bool hasRightType = command.Type == AbilityCommandType.AnnexAdjacentTerritory;

            bool unitInOwnerTerritory = CivTerritoryLogic.GetCivClaimingCell(unitLocation) == unitOwner;

            bool hasAnyCities = CityPossessionCanon.GetPossessionsOfOwner(unitOwner).Any();

            return hasRightType && unitInOwnerTerritory && hasAnyCities;
        }

        public void HandleCommandOnUnit(AbilityCommandRequest command, IUnit unit) {
            if(CanHandleCommandOnUnit(command, unit)) {
                var unitOwner = UnitPossessionCanon.GetOwnerOfPossession(unit);

                var unitLocation = UnitPositionCanon.GetOwnerOfPossession(unit);

                var nearestCity = GetNearestDomesticCity(unitLocation, unitOwner);

                foreach(var nearbyCell in Grid.GetNeighbors(unitLocation)) {
                    if(CellPossessionCanon.CanChangeOwnerOfPossession(nearbyCell, nearestCity)) {
                        CellPossessionCanon.ChangeOwnerOfPossession(nearbyCell, nearestCity);
                    }
                }
            }else {
                throw new InvalidOperationException("Command cannot be handled");
            }
        }

        #endregion

        private ICity GetNearestDomesticCity(IHexCell location, ICivilization domesticCiv) {
            int shortestDistance = int.MaxValue;
            ICity nearestCity = null;

            foreach(var domesticCity in CityPossessionCanon.GetPossessionsOfOwner(domesticCiv)) {
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(domesticCity);

                int distanceTo = Grid.GetDistance(location, cityLocation);

                if(distanceTo < shortestDistance) {
                    nearestCity = domesticCity;
                    shortestDistance = distanceTo;
                }
            }

            return nearestCity;
        }

        #endregion
        
    }

}
