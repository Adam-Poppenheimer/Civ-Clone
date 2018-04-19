using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;

namespace Assets.Simulation.SpecialtyResources {

    public class ExtractionValidityLogic : IExtractionValidityLogic {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> ResourceNodeLocationCanon;
        private IPossessionRelationship<ICity, IHexCell>         CityTerritoryCanon;
        private IPossessionRelationship<IHexCell, IImprovement>  ImprovementLocationCanon;
        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private ITechCanon                                       TechCanon;

        #endregion

        #region constructors

        [Inject]
        public ExtractionValidityLogic(
            IPossessionRelationship<IHexCell, IResourceNode> resourceNodeLocationCanon,
            IPossessionRelationship<ICity, IHexCell>         cityTerritoryCanon,
            IPossessionRelationship<IHexCell, IImprovement>  improvementLocationCanon,
            IPossessionRelationship<ICivilization, ICity>    cityPossessionCanon,
            ITechCanon                                       techCanon
        ){
            ResourceNodeLocationCanon = resourceNodeLocationCanon;
            CityTerritoryCanon        = cityTerritoryCanon;
            ImprovementLocationCanon  = improvementLocationCanon;
            CityPossessionCanon       = cityPossessionCanon;
            TechCanon                 = techCanon;
        }

        #endregion

        #region instance methods

        #region from IExtractionValidityLogic

        public bool IsNodeValidForExtraction(IResourceNode node) {
            var nodeLocation = ResourceNodeLocationCanon.GetOwnerOfPossession(node);

            if(nodeLocation == null) {
                return false;
            }

            var improvementAt = ImprovementLocationCanon.GetPossessionsOfOwner(nodeLocation).FirstOrDefault();

            if(improvementAt == null || !improvementAt.IsConstructed || node.Resource.Extractor != improvementAt.Template) {
                return false;
            }

            var cityOwning = CityTerritoryCanon.GetOwnerOfPossession(nodeLocation);

            if(cityOwning == null) {
                return false;
            }

            var civOwning = CityPossessionCanon.GetOwnerOfPossession(cityOwning);

            return TechCanon.IsResourceVisibleToCiv(node.Resource, civOwning);
        }

        #endregion

        #endregion
        
    }

}
