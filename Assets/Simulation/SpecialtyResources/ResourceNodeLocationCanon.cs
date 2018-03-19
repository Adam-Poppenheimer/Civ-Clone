using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceNodeLocationCanon : PossessionRelationship<IHexCell, IResourceNode> {

        #region constructors

        [Inject]
        public ResourceNodeLocationCanon(SpecialtyResourceSignals resourceSignals) {
            resourceSignals.ResourceNodeBeingDestroyedSignal.Subscribe(OnNodeBeingDestroyed);
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IHexCell, IResourceNode>

        protected override bool IsPossessionValid(IResourceNode possession, IHexCell owner) {
            return owner == null || GetPossessionsOfOwner(owner).Count() == 0;
        }

        protected override void DoOnPossessionBroken(IResourceNode possession, IHexCell oldOwner) {
            oldOwner.RefreshSlot();
        }

        protected override void DoOnPossessionEstablished(IResourceNode possession, IHexCell newOwner) {
            newOwner.RefreshSlot();
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
