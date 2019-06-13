using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.UI.Common {

    public class LoadingScreen : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private Text ProgressField;

        private HashSet<IMapChunk> RefreshedChunks = new HashSet<IMapChunk>();

        private IDisposable ChunkFinishedRefreshingSubscription;




        private IHexGrid            Grid;
        private MapRenderingSignals MapRenderingSignals;
        private Animator            UIAnimator;

        #endregion

        #region instance methods

        [Inject]
        private void InjectDependencies(
            IHexGrid grid, MapRenderingSignals mapRenderingSignals,
            [Inject(Id = "UI Animator")] Animator uiAnimator
        ) {
            Grid                = grid;
            MapRenderingSignals = mapRenderingSignals;
            UIAnimator          = uiAnimator;
        }

        #region Unity messages

        private void OnEnable() {
            ChunkFinishedRefreshingSubscription = MapRenderingSignals.ChunkFinishedRefreshing.Subscribe(
                chunk => RefreshedChunks.Add(chunk)
            );
        }

        private void OnDisable() {
            RefreshedChunks.Clear();

            ChunkFinishedRefreshingSubscription.Dispose();
        }

        private void LateUpdate() {
            if(Grid.Chunks != null) {
                int chunkCount = Grid.Chunks.Count();

                if(RefreshedChunks.Count == chunkCount) {
                    UIAnimator.SetTrigger("Return Requested");
                }else {
                    ProgressField.text = string.Format("Chunks loaded: {0}/{1}", RefreshedChunks.Count, Grid.Chunks.Count());
                }
            }else {
                ProgressField.text = "Setting up grid";
            }
        }

        #endregion

        #endregion

    }

}
