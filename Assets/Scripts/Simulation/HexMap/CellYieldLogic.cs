using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;

namespace Assets.Simulation.HexMap {

    public class CellYieldLogic : ICellYieldLogic {

        #region instance fields and properties

        private IPossessionRelationship<IHexCell, IResourceNode> NodePositionCanon;
        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private IPossessionRelationship<ICity, IHexCell>         CellPossessionCanon;
        private IPossessionRelationship<ICity, IBuilding>        BuildingPossessionCanon;
        private ITechCanon                                       TechCanon;
        private IFreshWaterLogic                                 FreshWaterCanon;
        private IGoldenAgeCanon                                  GoldenAgeCanon;
        private ICivilizationConfig                              CivConfig;

        private IInherentCellYieldLogic                          InherentYieldLogic;
        private IResourceNodeYieldLogic                          NodeYieldLogic;
        private IImprovementYieldLogic                           ImprovementYieldLogic;
        private ICellYieldFromBuildingsLogic                     BuildingYieldLogic;
        

        #endregion

        #region constructors

        [Inject]
        public CellYieldLogic(
            IPossessionRelationship<IHexCell, IResourceNode> nodePositionCanon,
            IImprovementLocationCanon                        improvementLocationCanon,
            IPossessionRelationship<ICity, IHexCell>         cellPossessionCanon,
            IPossessionRelationship<ICity, IBuilding>        buildingPossessionCanon,
            ITechCanon                                       techCanon,
            IFreshWaterLogic                                 freshWaterCanon,
            IGoldenAgeCanon                                  goldenAgeCanon,
            ICivilizationConfig                              civConfig,

            IInherentCellYieldLogic                          inherentYieldLogic,
            IResourceNodeYieldLogic                          nodeYieldLogic,
            IImprovementYieldLogic                           improvementYieldLogic,
            ICellYieldFromBuildingsLogic                     buildingYieldLogic
        ){
            NodePositionCanon        = nodePositionCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            CellPossessionCanon      = cellPossessionCanon;
            BuildingPossessionCanon  = buildingPossessionCanon;
            TechCanon                = techCanon;
            FreshWaterCanon          = freshWaterCanon;
            GoldenAgeCanon           = goldenAgeCanon;
            CivConfig                = civConfig;

            InherentYieldLogic       = inherentYieldLogic;
            NodeYieldLogic           = nodeYieldLogic;
            ImprovementYieldLogic    = improvementYieldLogic;
            BuildingYieldLogic       = buildingYieldLogic;
        }

        #endregion

        #region instance methods

        #region from ICellResourceLogic

        public YieldSummary GetYieldOfCell(IHexCell cell, ICivilization perspectiveCiv) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            YieldSummary retval = YieldSummary.Empty;
            ICity cityOwningCell = CellPossessionCanon.GetOwnerOfPossession(cell);

            retval += InherentYieldLogic.GetInherentCellYield(cell, false);

            var nodeAtLocation = NodePositionCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var visibleResources = TechCanon.GetResourcesVisibleToCiv(perspectiveCiv);

            var improvementOnCell = ImprovementLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var discoveredTechs = TechCanon.GetTechsDiscoveredByCiv(perspectiveCiv);

            bool hasFreshWater = FreshWaterCanon.HasAccessToFreshWater(cell);

            if(cityOwningCell != null) {
                var buildingsOnCell = BuildingPossessionCanon.GetPossessionsOfOwner(cityOwningCell)
                                                             .Select(building => building.Template);

                retval += BuildingYieldLogic.GetBonusCellYieldFromBuildings(cell, buildingsOnCell);
            }

            if(nodeAtLocation != null) {
                retval += NodeYieldLogic.GetYieldFromNode(nodeAtLocation, visibleResources, improvementOnCell);
            }

            if(improvementOnCell != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovement(
                    improvementOnCell, nodeAtLocation, visibleResources, discoveredTechs, hasFreshWater
                );
            }

            if(perspectiveCiv != null && GoldenAgeCanon.IsCivInGoldenAge(perspectiveCiv) && retval[YieldType.Gold] > 0f) {
                retval[YieldType.Gold] += CivConfig.GoldenAgeBonusGoldOnCells;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
