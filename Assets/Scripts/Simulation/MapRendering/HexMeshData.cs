using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    [Serializable]
    public class HexMeshData {
        
        public bool UseCollider {
            get { return _useCollider; }
        }
        [SerializeField] private bool _useCollider = false;

        public bool UseUVCoordinates {
            get { return _useUVCoordinates; }
        }
        [SerializeField] private bool _useUVCoordinates = false;

        public bool UseUV2Coordinates {
            get { return _useUV2Coordinates; }
        }
        [SerializeField] private bool _useUV2Coordinates = false;

        public bool UseUV3Coordinates {
            get { return _useUV3Coordinates; }
        }
        [SerializeField] private bool _useUV3Coordinates = false;

        public bool UseCellData {
            get { return _useCellData; }
        }
        [SerializeField] private bool _useCellData = false;

        public bool UseColors {
            get { return _useColors; }
        }
        [SerializeField] private bool _useColors = false;

        public bool ConvertVerticesToWorld {
            get { return _convertVerticesToWorld; }
        }
        [SerializeField] private bool _convertVerticesToWorld = false;

        public int Layer {
            get { return _layer; }
        }
        [SerializeField, Layer] private int _layer = 0;


        public HexRenderingData RenderingData {
            get { return _renderingData; }
        }
        [SerializeField] private HexRenderingData _renderingData = null;

    }

}
