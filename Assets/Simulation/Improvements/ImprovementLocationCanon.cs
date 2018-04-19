using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementLocationCanon : PossessionRelationship<IHexCell, IImprovement>, IImprovementLocationCanon {

        #region instance fields and properties

        private ImprovementSignals Signals;

        #endregion

        #region constructors

        public ImprovementLocationCanon(ImprovementSignals signals) {
            Signals = signals;

            signals.ImprovementBeingDestroyedSignal.Subscribe(OnImprovementBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IImprovement>

        protected override bool IsPossessionValid(IImprovement possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionBeingBroken(IImprovement possession, IHexCell oldOwner) {
            Signals.ImprovementBeingRemovedFromLocationSignal.OnNext(new Tuple<IImprovement, IHexCell>(possession, oldOwner));
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
