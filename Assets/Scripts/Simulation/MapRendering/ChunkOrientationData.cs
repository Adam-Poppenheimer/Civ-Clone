using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public class ChunkOrientationData {

        #region properties

        public IMapChunk Chunk { get; private set; }

        public bool IsReady {
            get { return BakerTriplet != null && BakerTriplet.Item1.IsReady && BakerTriplet.Item2.IsReady && BakerTriplet.Item3.IsReady; }
        }

        public Texture2D OrientationTexture { get { return BakerTriplet.Item1.Texture; } }
        public Texture2D WeightsTexture     { get { return BakerTriplet.Item2.Texture; } }
        public Texture2D DuckTexture        { get { return BakerTriplet.Item3.Texture; } }

        public Tuple<OrientationSubBaker, OrientationSubBaker, OrientationSubBaker> BakerTriplet;

        #endregion

        #region constructors

        public ChunkOrientationData(IMapChunk chunk) {
            Chunk = chunk;
        }

        #endregion

    }

}
