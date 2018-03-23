using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementLocationCanon : PossessionRelationship<IHexCell, IImprovement>, IImprovementLocationCanon {

        #region constructors

        public ImprovementLocationCanon(ImprovementSignals signals) {
            signals.ImprovementBeingDestroyedSignal.Subscribe(OnImprovementBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IImprovement>

        protected override bool IsPossessionValid(IImprovement possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionBroken(IImprovement possession, IHexCell oldOwner) {
            
        }

        protected override void DoOnPossessionEstablished(IImprovement possession, IHexCell newOwner) {
            
        }

        #endregion

        #region from IImprovementLocationCanon

        public bool CanPlaceImprovementOfTemplateAtLocation(IImprovementTemplate template, IHexCell location) {
            return GetPossessionsOfOwner(location).Count() == 0;
        }

        #endregion

        private void OnImprovementBeingDestroyed(IImprovement improvement) {
            ChangeOwnerOfPossession(improvement, null);
        }

        #endregion

    }

}
