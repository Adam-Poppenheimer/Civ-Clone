using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.Simulation.MapRendering {

    public class UnitRealignmentResponder {

        #region static fields and properties

        private static WaitForEndOfFrame SkipFrame = new WaitForEndOfFrame();

        #endregion

        #region instance fields and properties

        private Dictionary<IUnit, Coroutine> RepositioningCoroutineForUnit = 
            new Dictionary<IUnit, Coroutine>();



        private IUnitPositionCanon UnitPositionCanon;
        private MonoBehaviour      CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public UnitRealignmentResponder(
            IUnitPositionCanon unitPositionCanon, [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            MapRenderingSignals mapRenderingSignals
        ) {
            UnitPositionCanon = unitPositionCanon;
            CoroutineInvoker  = coroutineInvoker;

            mapRenderingSignals.ChunkFinishedRefreshing.Subscribe(OnChunkFinishedRefreshing);
        }

        #endregion

        #region instance methods

        private void OnChunkFinishedRefreshing(IMapChunk chunk) {
            foreach(var cell in chunk.CenteredCells) {
                foreach(var unit in UnitPositionCanon.GetPossessionsOfOwner(cell)) {
                    if(!RepositioningCoroutineForUnit.ContainsKey(unit)) {
                        RepositioningCoroutineForUnit[unit] = CoroutineInvoker.StartCoroutine(
                            RefreshAfterChunksDone(unit, cell.OverlappingChunks)
                        );
                    }
                }
            }
        }

        private IEnumerator RefreshAfterChunksDone(IUnit unit, IEnumerable<IMapChunk> chunks) {
            while(chunks.Any(chunk => chunk.IsRefreshing)) {
                yield return SkipFrame;
            }

            yield return SkipFrame;

            unit.RefreshPosition();

            RepositioningCoroutineForUnit.Remove(unit);
        }

        #endregion

    }

}
