using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Simulation.Civilizations {

    public class CivilizationYieldLogic : ICivilizationYieldLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IYieldGenerationLogic                         ResourceYieldLogic;
        private IUnitMaintenanceLogic                         UnitMaintenanceLogic;

        #endregion

        #region constructors

        [Inject]
        public CivilizationYieldLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IYieldGenerationLogic resourceGenerationLogic,
            IUnitMaintenanceLogic unitMaintenanceLogic
        ){
            CityPossessionCanon  = cityPossessionCanon;
            ResourceYieldLogic   = resourceGenerationLogic;
            UnitMaintenanceLogic = unitMaintenanceLogic;
        }

        #endregion

        #region instance methods

        #region from ICivilizationYieldLogic

        public YieldSummary GetYieldOfCivilization(ICivilization civ) {
            YieldSummary retval = YieldSummary.Empty;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                retval += ResourceYieldLogic.GetTotalYieldForCity(city);
            }

            retval -= new YieldSummary(gold: UnitMaintenanceLogic.GetMaintenanceOfUnitsForCiv(civ));

            return retval;
        }

        #endregion

        #endregion
        
    }

}
