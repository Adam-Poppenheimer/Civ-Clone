using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.SpecialtyResources {

    public class ResourceNode : MonoBehaviour, IResourceNode {

        #region instance fields and properties

        #region from IResourceNode

        public int Copies { get; set; }

        public ISpecialtyResourceDefinition Resource { get; set; }

        public bool IsVisible {
            get { return _isVisible; }
            set {
                if(_isVisible != value) {
                    _isVisible = value;
                    var nodeLocation = NodeLocationCanon.GetOwnerOfPossession(this);
                    if(nodeLocation != null) {
                        nodeLocation.RefreshSelfOnly();
                    }
                }
            }
        }
        private bool _isVisible = true;

        #endregion

        private IPossessionRelationship<IHexCell, IResourceNode> NodeLocationCanon;
        private SpecialtyResourceSignals                         Signals;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IPossessionRelationship<IHexCell, IResourceNode> nodeLocationCanon, 
            SpecialtyResourceSignals signals
        ){
            NodeLocationCanon = nodeLocationCanon;
            Signals           = signals;
        }

        #region Unity messages

        private void OnDestroy() {
            Signals.ResourceNodeBeingDestroyedSignal.OnNext(this);
        }

        #endregion

        #endregion

    }

}
