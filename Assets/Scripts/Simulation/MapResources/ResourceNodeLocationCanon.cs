using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapResources {

    public class ResourceNodeLocationCanon : PossessionRelationship<IHexCell, IResourceNode> {

        #region instance fields and properties

        private ResourceSignals ResourceSignals;

        #endregion

        #region constructors

        [Inject]
        public ResourceNodeLocationCanon(ResourceSignals resourceSignals, HexCellSignals cellSignals) {
            ResourceSignals = resourceSignals;

            resourceSignals.NodeBeingDestroyed.Subscribe(OnNodeBeingDestroyed);

            cellSignals.MapBeingClearedSignal.Subscribe(unit => Clear(false));
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IHexCell, IResourceNode>

        protected override bool IsPossessionValid(IResourceNode possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionEstablished(IResourceNode possession, IHexCell newOwner) {
            ResourceSignals.NodeAddedToLocation.OnNext(new Tuple<IResourceNode, IHexCell>(possession, newOwner));
        }

        protected override void DoOnPossessionBroken(IResourceNode possession, IHexCell oldOwner) {
            ResourceSignals.NodeRemovedFromLocation.OnNext(new Tuple<IResourceNode, IHexCell>(possession, oldOwner));
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
