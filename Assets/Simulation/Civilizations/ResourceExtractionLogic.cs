using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Civilizations {

    public class ResourceExtractionLogic : IResourceExtractionLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity>    CityPossessionCanon;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private ITechCanon                                       TechCanon;

        #endregion

        #region constructors

        [Inject]
        public ResourceExtractionLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IImprovementLocationCanon improvementLocationCanon,
            ITechCanon techCanon
        ){
            CityPossessionCanon      = cityPossessionCanon;
            CellPossessionCanon      = cellPossessionCanon;
            NodePositionCanon        = nodePositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            TechCanon                = techCanon;
        }

        #endregion

        #region instance methods

        #region from IResourceExtractionLogic

        public int GetExtractedCopiesOfResourceForCiv(IResourceDefinition resource, ICivilization civ) {
            int retval = 0;

            if(!TechCanon.IsResourceVisibleToCiv(resource, civ)) {
                return 0;
            }

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                    var nodeOnCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                    if( nodeOnCell != null && nodeOnCell.Resource == resource){
                        var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                        if(resource.Extractor == null || (
                            improvementAtLocation != null && improvementAtLocation.Template == resource.Extractor &&
                            improvementAtLocation.IsConstructed && !improvementAtLocation.IsPillaged
                        )) {
                            if(resource.Type == MapResources.ResourceType.Strategic) {
                                retval += nodeOnCell.Copies;
                            }else if(resource.Type == MapResources.ResourceType.Luxury){
                                retval++;
                            }
                        }
                    }
                }
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
