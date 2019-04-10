using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Rendering;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class HexSubMesh : MonoBehaviour {

        #region internal types

        public class Pool : MonoMemoryPool<HexRenderingData, HexSubMesh> {

            protected override void OnDespawned(HexSubMesh item) {
                item.Clear();
                base.OnDespawned(item);
            }

            protected override void Reinitialize(HexRenderingData rendererData, HexSubMesh item) {
                item.Initialize(rendererData);

                item.transform.localPosition = Vector3.zero;
            }

        }

        #endregion

        #region instance fields and properties

        public bool UseCollider {
            get { return MeshCollider.enabled;  }
            set { MeshCollider.enabled = value; }
        }

        public Mesh Mesh {
            get { return MeshFilter.sharedMesh; }
        }

        [SerializeField] private MeshFilter   MeshFilter;
        [SerializeField] private MeshRenderer MeshRenderer;
        [SerializeField] private MeshCollider MeshCollider;




        private IMemoryPool<Mesh> MeshPool;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(IMemoryPool<Mesh> meshPool) {
            MeshPool = meshPool;
        }

        public void Initialize(HexRenderingData rendererData) {
            MeshFilter.sharedMesh = MeshCollider.sharedMesh = MeshPool.Spawn();

            MeshRenderer.sharedMaterial    = rendererData.Material;
            MeshRenderer.shadowCastingMode = rendererData.ShadowCastingMode;
            MeshRenderer.receiveShadows    = rendererData.ReceiveShadows;
        }

        public void Clear() {
            MeshPool.Despawn(MeshFilter.sharedMesh);

            MeshFilter.sharedMesh = MeshCollider.sharedMesh = null;

            MeshCollider.enabled = false;
        }

        public void OverrideMaterial(Material newMaterial) {
            MeshRenderer.sharedMaterial = newMaterial;
        }

        #endregion

    }

}
