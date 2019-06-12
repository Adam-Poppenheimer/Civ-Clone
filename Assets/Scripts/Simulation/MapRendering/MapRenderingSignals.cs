using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingSignals {

        #region instance fields and properties

        public ISubject<Unit>      FarmlandsTriangulated   { get; private set; }

        public ISubject<IMapChunk> ChunkStartingToRefresh  { get; private set; }
        public ISubject<IMapChunk> ChunkFinishedRefreshing { get; private set; }
        public ISubject<IMapChunk> ChunkBeingDestroyed     { get; private set; }

        public ISubject<Unit> MapFinishedLoading { get; private set; }

        #endregion

        #region constructors

        public MapRenderingSignals() {
            FarmlandsTriangulated = new Subject<Unit>();

            ChunkStartingToRefresh  = new Subject<IMapChunk>();
            ChunkFinishedRefreshing = new Subject<IMapChunk>();
            ChunkBeingDestroyed     = new Subject<IMapChunk>();

            MapFinishedLoading = new Subject<Unit>();
        }

        #endregion

    }

}
