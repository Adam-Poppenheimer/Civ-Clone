using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

using Assets.UI;

using Assets.Util;

namespace Assets.Simulation.Core {

    public class ChunkCullingBrain : ITickable {

        #region instance fields and properties

        private HashSet<IMapChunk> ChunksRefreshing = new HashSet<IMapChunk>();




        private IGameCamera         GameCamera;
        private IHexGrid            Grid;
        private MapRenderingSignals MapRenderingSignals;

        #endregion

        #region constructors

        public ChunkCullingBrain(
            IGameCamera gameCamera, IHexGrid grid, MapRenderingSignals mapRenderingSignals
        ) {
            GameCamera          = gameCamera;
            Grid                = grid;
            MapRenderingSignals = mapRenderingSignals;

            MapRenderingSignals.ChunkStartingToRefresh    .Subscribe(OnChunkBeganToRefresh);
            MapRenderingSignals.ChunkFinishedRefreshing.Subscribe(chunk => ChunksRefreshing.Remove(chunk));

            MapRenderingSignals.ChunkBeingDestroyed.Subscribe(chunk => ChunksRefreshing.Remove(chunk));
        }

        #endregion

        #region instance methods

        #region from ITickable

        public void Tick() {
            if(ChunksRefreshing.Count == 0 && Grid.Chunks != null) {
                UpdateShownChunks();
            }
        }

        #endregion

        private void UpdateShownChunks() {
            var xzPlane = new Plane(Vector3.up, Vector3.zero);

            Ray bottomLeftRay  = GameCamera.ManagedCamera.ViewportPointToRay(new Vector3(0f, 0f, 0f));
            Ray topLeftRay     = GameCamera.ManagedCamera.ViewportPointToRay(new Vector3(0f, 1f, 0f));
            Ray topRightRay    = GameCamera.ManagedCamera.ViewportPointToRay(new Vector3(1f, 1f, 0f));
            Ray bottomRightRay = GameCamera.ManagedCamera.ViewportPointToRay(new Vector3(1f, 0f, 0f));

            float bottomLeftDistance, topLeftDistance, topRightDistance, bottomRightDistance;

            xzPlane.Raycast(bottomLeftRay,  out bottomLeftDistance);
            xzPlane.Raycast(topLeftRay,     out topLeftDistance);
            xzPlane.Raycast(topRightRay,    out topRightDistance);
            xzPlane.Raycast(bottomRightRay, out bottomRightDistance);

            Vector2 bottomLeftPoint  = bottomLeftRay .GetPoint(bottomLeftDistance ).ToXZ();
            Vector2 topLeftPoint     = topLeftRay    .GetPoint(topLeftDistance    ).ToXZ();
            Vector2 topRightPoint    = topRightRay   .GetPoint(topRightDistance   ).ToXZ();
            Vector2 bottomRightPoint = bottomRightRay.GetPoint(bottomRightDistance).ToXZ();

            foreach(var chunk in Grid.Chunks) {
                chunk.IsRendering = chunk.DoesXZFrustumOverlap(bottomLeftPoint, topLeftPoint, topRightPoint, bottomRightPoint);
            }
        }

        private void OnChunkBeganToRefresh(IMapChunk chunk) {
            if(ChunksRefreshing.Count == 0) {
                foreach(var gridChunk in Grid.Chunks) {
                    gridChunk.IsRendering = true;
                }
            }

            ChunksRefreshing.Add(chunk);
        }

        #endregion

    }

}
