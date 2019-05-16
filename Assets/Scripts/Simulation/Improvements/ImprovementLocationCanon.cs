using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Improvements {

    public class ImprovementLocationCanon : PossessionRelationship<IHexCell, IImprovement>, IImprovementLocationCanon {

        #region instance fields and properties

        private ImprovementSignals ImprovementSignals;

        #endregion

        #region constructors

        public ImprovementLocationCanon(ImprovementSignals improvementSignals, HexCellSignals cellSignals) {
            ImprovementSignals = improvementSignals;

            improvementSignals.BeingDestroyed.Subscribe(OnImprovementBeingDestroyed);

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IMapTile, IImprovement>

        protected override bool IsPossessionValid(IImprovement possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionEstablished(IImprovement possession, IHexCell newOwner) {
            ImprovementSignals.AddedToLocation.OnNext(new Tuple<IImprovement, IHexCell>(possession, newOwner));
        }

        protected override void DoOnPossessionBroken(IImprovement possession, IHexCell oldOwner) {
            ImprovementSignals.RemovedFromLocation.OnNext(new Tuple<IImprovement, IHexCell>(possession, oldOwner));
        }

        #endregion

        #region from IImprovementLocationCanon

        public bool CanPlaceImprovementOfTemplateAtLocation(IImprovementTemplate template, IHexCell location) {
            return GetPossessionsOfOwner(location).Count() == 0;
        }

        #endregion

        private void OnImprovementBeingDestroyed(IImprovement improvement) {
            if(GetOwnerOfPossession(improvement) != null) {
                ChangeOwnerOfPossession(improvement, null);
            }
        }

        #endregion

    }

}
