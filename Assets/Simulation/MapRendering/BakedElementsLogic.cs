using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.MapRendering {

    public class BakedElementsLogic : IBakedElementsLogic {

        #region instance fields and properties

        private IRiverCanon                 RiverCanon;
        private IImprovementLocationCanon   ImprovementLocationCanon;
        private ICivilizationTerritoryLogic CivTerritoryLogic;

        #endregion

        #region constructors

        [Inject]
        public BakedElementsLogic(
            IRiverCanon riverCanon, IImprovementLocationCanon improvementLocationCanon,
            ICivilizationTerritoryLogic civTerritoryLogic
        ) {
            RiverCanon               = riverCanon;
            ImprovementLocationCanon = improvementLocationCanon;
            CivTerritoryLogic        = civTerritoryLogic;
        }

        #endregion

        #region instance methods

        #region from IBakedElementsLogic

        public BakedElementFlags GetBakedElementsInCells(IEnumerable<IHexCell> cells) {
            if(cells == null) {
                throw new ArgumentNullException("cells");
            }

            var retval = BakedElementFlags.None;

            foreach(var cell in cells) {
                if(cell.Feature == CellFeature.Oasis) {
                    retval |= BakedElementFlags.SimpleLand | BakedElementFlags.SimpleWater;
                }

                if(RiverCanon.HasRiver(cell)) {
                    retval |= BakedElementFlags.Riverbanks;
                }

                if(cell.HasRoads) {
                    retval |= BakedElementFlags.Roads;
                }

                foreach(var improvement in ImprovementLocationCanon.GetPossessionsOfOwner(cell)) {
                    if(improvement.Template.ProducesFarmland) {
                        retval |= BakedElementFlags.Farmland;
                        break;
                    }
                }

                if(CivTerritoryLogic.GetCivClaimingCell(cell) != null) {
                    retval |= BakedElementFlags.Culture;
                }

                if(cell.Vegetation == CellVegetation.Marsh) {
                    retval |= BakedElementFlags.SimpleWater;
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
