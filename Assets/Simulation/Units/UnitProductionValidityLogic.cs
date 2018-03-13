using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class UnitProductionValidityLogic : IUnitProductionValidityLogic {

        #region instance fields and properties

        private IUnitPositionCanon UnitPositionCanon;

        private IResourceAssignmentCanon ResourceAssignmentCanon;

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        private IEnumerable<IUnitTemplate> AvailableUnitTemplates;

        #endregion

        #region constructors

        [Inject]
        public UnitProductionValidityLogic(IUnitPositionCanon unitPositionCanon,
            IResourceAssignmentCanon resourceAssignmentCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            [Inject(Id = "Available Unit Templates")] IEnumerable<IUnitTemplate> availableUnitTemplates
        ){
            UnitPositionCanon       = unitPositionCanon;
            ResourceAssignmentCanon = resourceAssignmentCanon;
            CityPossessionCanon     = cityPossessionCanon;
            CityLocationCanon       = cityLocationCanon;
            AvailableUnitTemplates  = availableUnitTemplates;
        }

        #endregion

        #region instance methods

        #region from IUnitProductionValidityLogic

        public IEnumerable<IUnitTemplate> GetTemplatesValidForCity(ICity city) {
            return AvailableUnitTemplates.Where(template => IsTemplateValidForCity(template, city));
        }

        public bool IsTemplateValidForCity(IUnitTemplate template, ICity city) {
            var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

            if(!UnitPositionCanon.CanPlaceUnitTemplateAtLocation(template, cityLocation, false)) {
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
