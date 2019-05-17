using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    [CreateAssetMenu(menuName = "Civ Clone/Map Rendering/Terrain Bake Config")]
    public class TerrainBakeConfig : ScriptableObject, ITerrainBakeConfig {

        #region instance fields and properties

        #region from ITerrainBakeConfig

        public int RenderTextureDepth {
            get { return _renderTextureDepth; }
        }
        [SerializeField] private int _renderTextureDepth = 0;

        public RenderTextureFormat RenderTextureFormat {
            get { return _renderTextureFormat; }
        }
        [SerializeField] private RenderTextureFormat _renderTextureFormat = RenderTextureFormat.ARGB32;


        public Shader TerrainBakeOcclusionShader {
            get { return _terrainBakeOcclusionShader; }
        }
        [SerializeField] private Shader _terrainBakeOcclusionShader = null;

        public BakedElementFlags BakeElementsHighRes {
            get { return _bakeElementsHighRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsHighRes = BakedElementFlags.Culture;

        public BakedElementFlags BakeElementsMediumRes {
            get { return _bakeElementsMediumRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsMediumRes = BakedElementFlags.Culture;

        public BakedElementFlags BakeElementsLowRes {
            get { return _bakeElementsLowRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsLowRes = BakedElementFlags.Culture;


        public Vector2 BakeTextureDimensionsHighRes {
            get { return _bakeTextureDimensionsHighRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsHighRes = Vector2.zero;

        public Vector2 BakeTextureDimensionsMediumRes {
            get { return _bakeTextureDimensionsMediumRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsMediumRes = Vector2.zero;

        public Vector2 BakeTextureDimensionsLowRes {
            get { return _bakeTextureDimensionsLowRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsLowRes = Vector2.zero;

        #endregion

        #endregion

    }

}
