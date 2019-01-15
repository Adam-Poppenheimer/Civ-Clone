using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.AI {

    public class ImprovementInfluenceSource : IInfluenceSource {

        #region instance fields and properties

        private IImprovementLocationCanon                        ImprovementLocationCanon;
        private IHexGrid                                         Grid;
        private ICivilizationTerritoryLogic                      CivTerritoryLogic;
        private IAIConfig                                        AIConfig;
        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public ImprovementInfluenceSource(
            IImprovementLocationCanon improvementLocationCanon, IHexGrid grid,
            ICivilizationTerritoryLogic civTerritoryLogic, IAIConfig aiConfig,
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon
        ) {
            ImprovementLocationCanon = improvementLocationCanon;
            Grid                     = grid;
            CivTerritoryLogic        = civTerritoryLogic;
            AIConfig                 = aiConfig;
            NodeLocationCanon        = nodeLocationCanon;
        }

        #endregion

        #region instance methods

        #region from IInfluenceSource

        public void ApplyToMaps(InfluenceMaps maps, ICivilization targetCiv) {
            if(maps == null) {
                throw new ArgumentNullException("maps");

            }else if(targetCiv == null) {
                throw new ArgumentNullException("targetCiv");
            }

            foreach(var nonDomesticCell in Grid.Cells.Where(cell => CivTerritoryLogic.GetCivClaimingCell(cell) != targetCiv)) {
                float pillageValue = 0f;

                if(nonDomesticCell.HasRoads) {
                    pillageValue += AIConfig.RoadPillageValue;
                }

                var improvementAt = ImprovementLocationCanon.GetPossessionsOfOwner(nonDomesticCell).FirstOrDefault();

                if(improvementAt != null && !improvementAt.IsPillaged) {
                    var nodeAt = NodeLocationCanon.GetPossessionsOfOwner(nonDomesticCell).FirstOrDefault();

                    if(nodeAt != null && nodeAt.Resource.Extractor == improvementAt.Template) {
                        pillageValue += AIConfig.ExtractingImprovementPillageValue;
                    }else {
                        pillageValue += AIConfig.NormalImprovementPillageValue;
                    }
                }

                maps.PillagingValue[nonDomesticCell.Index] = pillageValue;
            }
        }

        #endregion

        #endregion
        
    }

}
