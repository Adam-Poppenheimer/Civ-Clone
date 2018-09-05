using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;
using Assets.Simulation.Cities.Buildings;
using UnityEngine;

namespace Assets.Simulation.MapGeneration {

    public class YieldEstimator : IYieldEstimator {

        #region internal types

        private class HypotheticalImprovement : IImprovement {

            #region instance fields and properties

            public bool IsConstructed { get { return true;  } }
            public bool IsPillaged    { get { return false; } }

            public bool IsReadyToConstruct { get; set; }

            public IImprovementTemplate Template { get; set; }

            public Transform transform { get; set; }

            public int WorkInvested { get; set; }

            #endregion

            #region constructors

            public HypotheticalImprovement(IImprovementTemplate template) {
                Template = template;
            }

            #endregion

            #region instance methods

            public void Construct() {
                throw new NotImplementedException();
            }

            public void Destroy() {
                throw new NotImplementedException();
            }

            public void Pillage() {
                throw new NotImplementedException();
            }

            #endregion

        }

        #endregion

        #region static fields and properties

        private static YieldSummary OneScience = new YieldSummary(science: 1);

        #endregion

        #region instance fields and properties

        private IInherentCellYieldLogic                          InherentYieldLogic;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private IResourceNodeYieldLogic                          NodeYieldLogic;
        private IImprovementYieldLogic                           ImprovementYieldLogic;
        private IFreshWaterCanon                                 FreshWaterCanon;
        private ICellYieldFromBuildingsLogic                     YieldFromBuildingsLogic;
        private IImprovementEstimator                            ImprovementEstimator;

        private IEnumerable<IResourceDefinition> AvailableResources;
        private IEnumerable<IBuildingTemplate>   AvailableBuildings;

        #endregion

        #region constructors

        [Inject]
        public YieldEstimator(
            IInherentCellYieldLogic                          inherentYieldLogic,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon,
            IResourceNodeYieldLogic                          nodeYieldLogic,
            IImprovementYieldLogic                           improvementYieldLogic,
            IFreshWaterCanon                                 freshWaterCanon,
            ICellYieldFromBuildingsLogic                     yieldFromBuildingsLogic,
            IImprovementEstimator                            improvementEstimator,
            
            List<IBuildingTemplate> availableBuildings,
            [Inject(Id = "Available Resources")] IEnumerable<IResourceDefinition> availableResources
        ) {
            InherentYieldLogic      = inherentYieldLogic;
            NodeLocationCanon       = nodeLocationCanon;
            NodeYieldLogic          = nodeYieldLogic;
            ImprovementYieldLogic   = improvementYieldLogic;
            FreshWaterCanon         = freshWaterCanon;
            YieldFromBuildingsLogic = yieldFromBuildingsLogic;
            ImprovementEstimator    = improvementEstimator;

            AvailableResources = availableResources;
            AvailableBuildings = availableBuildings;
        }

        #endregion

        #region instance methods

        #region from CellYieldEstimator

        public YieldSummary GetYieldEstimateForCell(
            IHexCell cell, IEnumerable<ITechDefinition> availableTechs
        ) {
            var retval = YieldSummary.Empty;

            var nodeAtLocation = NodeLocationCanon.GetPossessionsOfOwner(cell).FirstOrDefault();

            var expectedImprovement = ImprovementEstimator.GetExpectedImprovementForCell(cell, nodeAtLocation);

            if(nodeAtLocation != null) {
                retval += NodeYieldLogic.GetYieldFromNode(
                    nodeAtLocation, AvailableResources, new HypotheticalImprovement(expectedImprovement)
                );
            }
            
            bool clearVegetation = false;
            if(expectedImprovement != null) {
                retval += ImprovementYieldLogic.GetYieldOfImprovementTemplate(
                    expectedImprovement, nodeAtLocation, AvailableResources, availableTechs,
                    FreshWaterCanon.HasAccessToFreshWater(cell)
                );

                clearVegetation = expectedImprovement.ClearsVegetationWhenBuilt;
            }

            retval += InherentYieldLogic.GetInherentCellYield(cell, clearVegetation);

            retval += YieldFromBuildingsLogic.GetBonusCellYieldFromBuildings(cell, AvailableBuildings);

            return retval + OneScience;               
        }

        public YieldSummary GetYieldEstimateForResource(IResourceDefinition resource) {
            return resource.BonusYieldBase + resource.BonusYieldWhenImproved;
        }

        #endregion

        #endregion

    }

}
