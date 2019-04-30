using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class FullMapRefresher : IFullMapRefresher {

        #region instance fields and properties

        #region from IFullMapRefresher

        public bool IsRefreshingRivers {
            get { return RefreshRiversCoroutine != null; }
        }

        public bool IsRefreshingFarmland {
            get { return RefreshFarmlandCoroutine != null; }
        }

        #endregion

        private Coroutine RefreshRiversCoroutine;
        private Coroutine RefreshFarmlandCoroutine;




        private IFarmTriangulator  FarmTriangulator;
        private IRiverTriangulator RiverTriangulator;
        private IHexGrid           Grid;
        private ITerrainBaker      TerrainBaker;
        private MonoBehaviour      CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public FullMapRefresher(
            IFarmTriangulator farmTriangulator, IRiverTriangulator riverTriangulator,
            IHexGrid grid, ITerrainBaker terrainBaker, [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker
        ) {
            FarmTriangulator  = farmTriangulator;
            RiverTriangulator = riverTriangulator;
            Grid              = grid;
            TerrainBaker      = terrainBaker;
            CoroutineInvoker  = coroutineInvoker;
        }

        #endregion

        #region instance methods 

        #region from IFullMapRefresher

        public void RefreshFarmland() {
            if(RefreshFarmlandCoroutine == null) {
                RefreshFarmlandCoroutine = CoroutineInvoker.StartCoroutine(RefreshFarmland_Perform());
            }
        }

        public void RefreshRivers() {
            if(RefreshRiversCoroutine == null) {
                RefreshRiversCoroutine = CoroutineInvoker.StartCoroutine(RefreshRivers_Perform());
            }
        }

        #endregion

        private IEnumerator RefreshFarmland_Perform() {
            yield return new WaitForEndOfFrame();

            while(IsRefreshingRivers) {
                yield return new WaitForEndOfFrame();
            }

            FarmTriangulator.TriangulateFarmland();

            foreach(var chunk in Grid.Chunks) {
                TerrainBaker.BakeIntoTextures(chunk.LandBakeTexture, chunk.WaterBakeTexture, chunk);
            }

            RefreshFarmlandCoroutine = null;
        }

        private IEnumerator RefreshRivers_Perform() {
            yield return new WaitForEndOfFrame();

            RiverTriangulator.TriangulateRivers();

            foreach(var chunk in Grid.Chunks) {
                TerrainBaker.BakeIntoTextures(chunk.LandBakeTexture, chunk.WaterBakeTexture, chunk);
            }

            RefreshRiversCoroutine = null;
        }

        #endregion

    }

}
