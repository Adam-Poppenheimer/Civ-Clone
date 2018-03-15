using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Simulation.HexMap {

    public class CellResourceLogic : ICellResourceLogic {

        #region instance fields and properties

        private IHexGridConfig Config;

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;

        private IImprovementLocationCanon ImprovementLocationCanon;

        private IImprovementYieldLogic ImprovementYieldLogic;

        private IPossessionRelationship<ICity, IHexCell> CellPossessionCanon;

        private IPossessionRelationship<ICity, IBuilding> BuildingPossessionCanon;

        #endregion

        #region constructors

        [Inject]
        public CellResourceLogic(IHexGridConfig config,
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IImprovementLocationCanon improvementLocationCanon,
            IImprovementYieldLogic improvementYieldLogic,
            IPossessionRelationship<ICity, IHexCell> cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding> buildingPossessionCanon
        ){
            Config                   = config;
            NodePositionCanon        = nodePositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            ImprovementYieldLogic    = improvementYieldLogic;
            CellPossessionCanon      = cellPossessionCanon;
            BuildingPossessionCanon  = buildingPossessionCanon;
        }

        #endregion

        #region instance methods

        #region from ITileResourceLogic

        public ResourceSummary GetYieldOfCell(IHexCell cell) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            ResourceSummary retval;

            if(cell.Feature != TerrainFeature.None) {
                retval = Config.FeatureYields[(int)cell.Feature];
            }else if(cell.Shape != TerrainShape.Flatlands) {
                retval = Config.ShapeYields[(int)cell.Shape];
            }else {
                retval = Config.TerrainYields[(int)cell.Terrain];
            }

            var nodeAtLocation = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(nodeAtLocation != null) {
                retval += nodeAtLocation.Resource.BonusYieldBase;
            }

            retval += GetContributionFromBuildings(cell, nodeAtLocation);

            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();
            if(improvementOnCell != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovement(improvementOnCell);
            }

            return retval;
        }

        #endregion

        private ResourceSummary GetContributionFromBuildings(IHexCell cell, IResourceNode nodeAtLocation) {
            var retval = ResourceSummary.Empty;

            var cityOwningCell = CellPossessionCanon.GetOwnerOfPossession(cell);

            if(cityOwningCell != null) {
                foreach(var building in BuildingPossessionCanon.GetPossessionsOfOwner(cityOwningCell)) {

                    foreach(var resourceModification in building.Template.ResourceYieldModifications) {
                        if(resourceModification.Resource == nodeAtLocation.Resource) {
                            retval += resourceModification.BonusYield;
                        }
                    }

                    foreach(var cellModification in building.Template.CellYieldModifications) {
                        if( cellModification.PropertyConsidered == CellPropertyType.Terrain &&
                            cell.Terrain == cellModification.TerrainRequired
                        ) {
                            retval += cellModification.BonusYield;

                        }else if( 
                            cellModification.PropertyConsidered == CellPropertyType.Shape &&
                            cell.Shape == cellModification.ShapeRequired
                        ){
                            retval += cellModification.BonusYield;

                        }else if(
                            cellModification.PropertyConsidered == CellPropertyType.Feature &&
                            cell.Feature == cellModification.FeatureRequired
                        ) {
                            retval += cellModification.BonusYield;

                        }else if(
                            cellModification.PropertyConsidered == CellPropertyType.CellIsUnderwater &&
                            cell.IsUnderwater == cellModification.MustBeUnderwater
                        ) {
                            retval += cellModification.BonusYield;
                        }
                    }
                }
            }

            return retval;
        }

        #endregion
        
    }

}
