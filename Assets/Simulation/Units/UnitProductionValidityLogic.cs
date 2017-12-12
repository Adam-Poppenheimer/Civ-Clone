using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public class UnitProductionValidityLogic : IUnitProductionValidityLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        #endregion

        #region constructors

        [Inject]
        public UnitProductionValidityLogic(IUnitPositionCanon unitPositionCanon) {
            UnitPositionCanon = unitPositionCanon;
        }

        #endregion

        #region instance methods

        #region from IUnitProductionValidityLogic

        public bool IsTemplateValidForProductionInCity(IUnitTemplate template, ICity city) {
            return UnitPositionCanon.CanPlaceUnitOfTypeAtLocation(template.Type, city.Location);
        }

        #endregion

        #endregion
        
    }

}
