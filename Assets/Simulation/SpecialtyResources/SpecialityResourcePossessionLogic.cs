using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.SpecialtyResources {

    public class SpecialtyResourcePossessionLogic : ISpecialtyResourcePossessionLogic {

        #region instance fields and properties

        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private IEnumerable<ISpecialtyResourceDefinition> AvailableResourceDefinitions;

        #endregion

        #region constructors

        [Inject]
        public SpecialtyResourcePossessionLogic(
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon, 
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon, 
            IImprovementLocationCanon improvementLocationCanon,
            [Inject(Id = "Available Specialty Resources")] IEnumerable<ISpecialtyResourceDefinition> availableResourceDefinitions
        ){
            CityPossessionCanon      = cityPossessionCanon;
            CellPossessionCanon      = cellPossessionCanon;
            NodePositionCanon        = nodePositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            AvailableResourceDefinitions   = availableResourceDefinitions;
        }

        #endregion

        #region instance methods

        #region from ISpecialtyResourcePossessionCanon

        public int GetCopiesOfResourceBelongingToCiv(ISpecialtyResourceDefinition resource, ICivilization civ) {
            if(resource == null) {
                throw new ArgumentNullException("resource");
            }else if(civ == null) {
                throw new ArgumentNullException("civ");
            }

            int retval = 0;

            foreach(var city in CityPossessionCanon.GetPossessionsOfOwner(civ)) {
                foreach(var cell in CellPossessionCanon.GetPossessionsOfOwner(city)) {
                    var nodeOnCell = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                    if( nodeOnCell != null && nodeOnCell.Resource == resource){
                        var improvementAtLocation = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

                        if( resource.Extractor == null ||
                            (improvementAtLocation != null && improvementAtLocation.Template == resource.Extractor)
                        ) {
                            retval += nodeOnCell.Copies;
                        }
                    }
                }
            }

            return retval;
        }

        public Dictionary<ISpecialtyResourceDefinition, int> GetFullResourceSummaryForCiv(ICivilization civ) {
            var retval = new Dictionary<ISpecialtyResourceDefinition, int>();

            foreach(var resourceDefinition in AvailableResourceDefinitions) {
                retval[resourceDefinition] = GetCopiesOfResourceBelongingToCiv(resourceDefinition, civ);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
