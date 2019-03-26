using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    [Serializable]
    public class HexMeshData {
        
        public bool UseCollider {
            get { return _useCollider; }
        }
        [SerializeField] private bool _useCollider;

        public bool UseUVCoordinates {
            get { return _useUVCoordinates; }
        }
        [SerializeField] private bool _useUVCoordinates;

        public bool UseUV2Coordinates {
            get { return _useUV2Coordinates; }
        }
        [SerializeField] private bool _useUV2Coordinates;

        public bool UseUV3Coordinates {
            get { return _useUV3Coordinates; }
        }
        [SerializeField] private bool _useUV3Coordinates;

        public bool UseCellData {
            get { return _useCellData; }
        }
        [SerializeField] private bool _useCellData;

        public bool UseColors {
            get { return _useColors; }
        }
        [SerializeField] private bool _useColors;

        public bool WeldMesh {
            get { return _weldMesh; }
        }
        [SerializeField] private bool _weldMesh;

        public bool ConvertVerticesToWorld {
            get { return _convertVerticesToWorld; }
        }
        [SerializeField] private bool _convertVerticesToWorld;


        public HexRenderingData RenderingData {
            get { return _renderingData; }
        }
        [SerializeField] private HexRenderingData _renderingData;

    }

}
