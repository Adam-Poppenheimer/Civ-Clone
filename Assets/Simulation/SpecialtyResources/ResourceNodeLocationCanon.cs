using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceNodeLocationCanon : PossessionRelationship<IHexCell, IResourceNode> {

        #region instance fields and properties

        private SpecialtyResourceSignals Signals;

        #endregion

        #region constructors

        [Inject]
        public ResourceNodeLocationCanon(SpecialtyResourceSignals signals) {
            Signals = signals;

            signals.ResourceNodeBeingDestroyedSignal.Subscribe(OnNodeBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IHexCell, IResourceNode>

        protected override bool IsPossessionValid(IResourceNode possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionBeingBroken(IResourceNode possession, IHexCell oldOwner) {
            Signals.ResourceNodeBeingRemovedFromLocationSignal.OnNext(new Tuple<IResourceNode, IHexCell>(possession, oldOwner));
        }

        #endregion

        private void OnNodeBeingDestroyed(IResourceNode node) {
            if(GetOwnerOfPossession(node) != null) {
                ChangeOwnerOfPossession(node, null);
            }
        }

        #endregion

    }

}
