using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.Units.Promotions {

    public class StartingExperienceLogic : IStartingExperienceLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public StartingExperienceLogic(
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            BuildingPossessionCanon = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from IStartingExperienceLogic

        public int GetStartingExperienceForUnit(IUnit unit, ICity producingCity) {
            int retval = 0;

            foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(producingCity)) {
                retval += building.Template.BonusExperience;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
