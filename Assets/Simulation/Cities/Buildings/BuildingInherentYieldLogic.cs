﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Cities.Buildings {

    //Only returns yield unassociated with any cells. Yield that buildings add to
    //cells are added to the cell itself through ICellYieldLogic and
    public class BuildingInherentYieldLogic : IBuildingInherentYieldLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public BuildingInherentYieldLogic(IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon) {
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IBuildingResourceLogic

        public YieldSummary GetYieldOfBuilding(IBuilding building) {
            var retval = building.Template.StaticYield;

            var buildingOwner = BuildingPossessionCanon.GetOwnerOfPossession(building);

            retval += building.Template.BonusYieldPerPopulation * buildingOwner.Population;

            int occupiedSlots = building.Slots.Where(slot => slot.IsOccupied).Count();

            retval += building.Template.SlotYield * occupiedSlots;

            return retval;
        }

        #endregion

        #endregion

    }

}
