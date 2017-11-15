using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Cities {

    public class City : MonoBehaviour, ICity {

        #region instance fields and properties

        #region from ICity

        public int Population {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public ReadOnlyCollection<IBuilding> Buildings {
            get {
                throw new NotImplementedException();
            }
        }

        public ResourceSummary Stockpile {
            get {
                throw new NotImplementedException();
            }

            set {
                throw new NotImplementedException();
            }
        }

        public ResourceSummary Income {
            get {
                throw new NotImplementedException();
            }
        }

        public IProductionProject CurrentProject {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        private IPopulationGrowthLogic growthLogic;

        private IProductionLogic productionLogic;

        private IResourceGenerationLogic resourceGenerationLogic;

        private ITileExpansionLogic expansionLogic;

        private ITilePossessionCanon possessionCanon;

        private IWorkerDistributionLogic distributionLogic;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IPopulationGrowthLogic growthLogic, IProductionLogic productionLogic, 
            IResourceGenerationLogic resourceGenerationLogic, ITileExpansionLogic expansionLogic,
            ITilePossessionCanon possessionCanon, IWorkerDistributionLogic distributionLogic) {

        }

        #region from ICity

        public void AddBuilding(IBuilding building) {
            throw new NotImplementedException();
        }

        public void RemoveBuilding(IBuilding building) {
            throw new NotImplementedException();
        }

        public void SetCurrentProject(IProductionProject project) {
            throw new NotImplementedException();
        }

        public void PerformBeginningOfTurn() {
            throw new NotImplementedException();
        }

        public void PerformEndOfTurn() {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }

}
