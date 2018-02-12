using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Units {

    public class UnitProductionValidityLogic : IUnitProductionValidityLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IResourceAssignmentCanon ResourceAssignmentCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IEnumerable<IUnitTemplate> AvailableUnitTemplates;

        #endregion

        #region constructors

        [Inject]
        public UnitProductionValidityLogic(IUnitPositionCanon unitPositionCanon,
            IResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates
        ){
            UnitPositionCanon       = unitPositionCanon;
            ResourceAssignmentCanon = resourceAssignmentCanon;
            CityPossessionCanon     = cityPossessionCanon;
            AvailableUnitTemplates  = availableUnitTemplates;
        }

        #endregion

        #region instance methods

        #region from IUnitProductionValidityLogic

        public IEnumerable<IUnitTemplate> GetTemplatesValidForCity(ICity city) {
            return AvailableUnitTemplates.Where(template => IsTemplateValidForCity(template, city));
        }

        public bool IsTemplateValidForCity(IUnitTemplate template, ICity city) {
            if(!UnitPositionCanon.CanPlaceUnitOfTypeAtLocation(template.Type, city.Location, false)) {
                return false;
            }

            var cityOwner = CityPossessionCanon.GetOwnerOfPossession(city);
            foreach(var resource in template.RequiredResources) {
                if(ResourceAssignmentCanon.GetFreeCopiesOfResourceForCiv(resource, cityOwner) <= 0) {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #endregion
        
    }

}
