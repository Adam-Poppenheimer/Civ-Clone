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
        [SerializeField] private int _renderTextureDepth;

        public RenderTextureFormat RenderTextureFormat {
            get { return _renderTextureFormat; }
        }
        [SerializeField] private RenderTextureFormat _renderTextureFormat;


        public Shader TerrainBakeOcclusionShader {
            get { return _terrainBakeOcclusionShader; }
        }
        [SerializeField] private Shader _terrainBakeOcclusionShader;

        public BakedElementFlags BakeElementsHighRes {
            get { return _bakeElementsHighRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsHighRes;

        public BakedElementFlags BakeElementsMediumRes {
            get { return _bakeElementsMediumRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsMediumRes;

        public BakedElementFlags BakeElementsLowRes {
            get { return _bakeElementsLowRes; }
        }
        [SerializeField, BitMask(typeof(BakedElementFlags))] private BakedElementFlags _bakeElementsLowRes;


        public Vector2 BakeTextureDimensionsHighRes {
            get { return _bakeTextureDimensionsHighRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsHighRes;

        public Vector2 BakeTextureDimensionsMediumRes {
            get { return _bakeTextureDimensionsMediumRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsMediumRes;

        public Vector2 BakeTextureDimensionsLowRes {
            get { return _bakeTextureDimensionsLowRes; }
        }
        [SerializeField] private Vector2 _bakeTextureDimensionsLowRes;

        #endregion

        #endregion

    }

}
