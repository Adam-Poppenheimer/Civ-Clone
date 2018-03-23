using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Cities {

    public class UnemploymentLogic : IUnemploymentLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IHexCell>  CellPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnemploymentLogic(
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            CellPossessionCanon     = cellPossessionCanon;
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnemploymentLogic

        public int GetUnemployedPeopleInCity(ICity city) {
            int occupiedCells = CellPossessionCanon.GetPossessionsOfOwner(city).Where(tile => tile.WorkerSlot.IsOccupied).Count();

            int occupiedBuildingSlots = 0;
            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(city)) {
                occupiedBuildingSlots += building.Slots.Where(slot => slot.IsOccupied).Count();
            }

            int unemployedPeople = city.Population - (occupiedCells + occupiedBuildingSlots);
            if(unemployedPeople < 0) {
                throw new NegativeUnemploymentException("This city has more occupied slots than it has people");
            }
            return unemployedPeople;
        }

        #endregion

        #endregion

    }

}
