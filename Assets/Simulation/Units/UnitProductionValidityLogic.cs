using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class UnitProductionValidityLogic : IUnitProductionValidityLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IEnumerable<IUnitTemplate> AvailableUnitTemplates;

        #endregion

        #region constructors

        [Inject]
        public UnitProductionValidityLogic(IUnitPositionCanon unitPositionCanon,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates
        ){
            UnitPositionCanon = unitPositionCanon;
            AvailableUnitTemplates = availableUnitTemplates;
        }

        #endregion

        #region instance methods

        #region from IUnitProductionValidityLogic

        public IEnumerable<IUnitTemplate> GetTemplatesValidForCity(ICity city) {
            return AvailableUnitTemplates.Where(template => IsTemplateValidForCity(template, city));
        }

        public bool IsTemplateValidForCity(IUnitTemplate template, ICity city) {
            return UnitPositionCanon.CanPlaceUnitOfTypeAtLocation(template.Type, city.Location, false);
        }

        #endregion

        #endregion
        
    }

}
