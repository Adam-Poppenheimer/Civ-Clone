using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.Civilizations {

    public class ConnectionPathCostLogic : IConnectionPathCostLogic {

        #region instance fields and properties
        
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICity, IBuilding>     BuildingPossessionCanon;
        private ICivilizationTerritoryLogic                   CivTerritoryLogic;

        #endregion

        #region constructors

        [Inject]
        public ConnectionPathCostLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity>      cityLocationCanon,
            IPossessionRelationship<ICity, IBuilding>     buildingPossessionCanon,
            ICivilizationTerritoryLogic                   civTerritoryLogic
        ) {
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
            CivTerritoryLogic       = civTerritoryLogic;
        }

        #endregion

        #region instance methods

        #region from IConnectionPathCostLogic

        public Func<IHexCell, IHexCell, float> BuildCivConnectionPathCostFunction(ICivilization civOne, ICivilization civTwo) {
            return delegate(IHexCell fromCell, IHexCell toCell) {
                return IsMovementValidForConnection(fromCell, toCell, civOne, civTwo) ? 1 : -1;
            };
        }

        public Func<IHexCell, IHexCell, float> BuildCapitalConnectionPathCostFunction(ICivilization domesticCiv) {
            return (currentCell, nextCell) => (ConnectionFilter(currentCell, nextCell, domesticCiv) ? 1f : -1f);
        }

        #endregion

        private bool IsMovementValidForConnection(IHexCell fromCell, IHexCell toCell, ICivilization civOne, ICivilization civTwo) {
            if(toCell.Terrain.IsWater()) {

                if(fromCell.Terrain.IsWater()) {
                    return true;
                }

                var cityAtFromCell = CityLocationCanon.GetPossessionsOfOwner(fromCell).FirstOrDefault();
                if(cityAtFromCell == null) {
                    return false;
                }

                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(cityAtFromCell);
                if(cityOwner != civOne && cityOwner != civTwo) {
                    return false;
                }

                return IsCityValidHarbor(cityAtFromCell);

            }else if(toCell.HasRoads){

                return true;

            }else {
                var cityAtToCell = CityLocationCanon.GetPossessionsOfOwner(toCell).FirstOrDefault();
                if(cityAtToCell == null) {
                    return false;
                }

                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(cityAtToCell);
                if(cityOwner != civOne && cityOwner != civTwo) {
                    return false;
                }

                return !fromCell.Terrain.IsWater() || toCell.Terrain.IsWater() || IsCityValidHarbor(cityAtToCell);
            }
        }

        private bool IsCityValidHarbor(ICity city) {
            return BuildingPossessionCanon.GetPossessionsOfOwner(city).Any(
                building => building.Template.ProvidesOverseaConnection
            );
        }

        /*
         * We're searching for a valid path that a connection could take. Since we don't really
         * care about how long that connection is, we're just looking to see if the cell transition
         * could be part of some valid path.
         * 
         * City connections and occur along roads and across water and can transit through cities.
         * 
         * A pair of land cells forms a valid connection only if both cells are either a road or a city.
         * They can be of different types. Both of these cells must also belong to the domestic civ or
         * have no owner. Any cities must also belong to the domestic civ.
         * 
         * A pair of water cells forms a valid connection as long as both cells belong to the domestic civ
         * or have no owner. Shallow, deep, and fresh water have no adverse effects on city connectivity.
         * 
         * If the current cell is water and the next cell is land, the connection is only valid if
         * the next cell has a city with a building that provides an overseas connection. That city
         * must belong to the domestic civ.
         * 
         * If the current cell is land and the next cell is water, the connection is only valid if
         * the current cell has a city with a building that provides an overseas connection. That city
         * must belong to the domestic civ.
         * 
         */
        private bool ConnectionFilter(IHexCell currentCell, IHexCell nextCell, ICivilization domesticCiv) {
            var currentOwner = CivTerritoryLogic.GetCivClaimingCell(currentCell);
            var nextOwner    = CivTerritoryLogic.GetCivClaimingCell(nextCell);

            //All cases are invalid if either of the cells
            if( (currentOwner != null && currentOwner != domesticCiv) ||
                (nextOwner    != null && nextOwner    != domesticCiv)
            ) {
                return false;
            }

            if(currentCell.Terrain.IsWater()) {
                if(nextCell.Terrain.IsWater()) {
                    //Both cells are water
                    return true;

                }else {
                    //CurrentCell is water and NextCell is land
                    return HasDomesticPortCity(nextCell, domesticCiv);
                }
            }else if(nextCell.Terrain.IsWater()) {
                //CurrentCell is land and NextCell is water
                return HasDomesticPortCity(currentCell, domesticCiv);

            }else {
                //Both cells are land
                return HasRoadOrDomesticCity(currentCell, domesticCiv)
                    && HasRoadOrDomesticCity(nextCell,    domesticCiv);
            }
        }

        private bool HasDomesticPortCity(IHexCell cell, ICivilization domesticCiv) {
            var cityAtCell = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            if(cityAtCell != null && CityPossessionCanon.GetOwnerOfPossession(cityAtCell) == domesticCiv) {
                var buildingsInCity = BuildingPossessionCanon.GetPossessionsOfOwner(cityAtCell);

                return buildingsInCity.Any(building => building.Template.ProvidesOverseaConnection);

            }else {
                return false;
            }
        }

        private bool HasRoadOrDomesticCity(IHexCell cell, ICivilization domesticCiv) {
            if(cell.HasRoads) {
                return true;
            }else {
                var cityAtCell = CityLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                return cityAtCell != null && CityPossessionCanon.GetOwnerOfPossession(cityAtCell) == domesticCiv;
            }
        }

        #endregion

    }

}
