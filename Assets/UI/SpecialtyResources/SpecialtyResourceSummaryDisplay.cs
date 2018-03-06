using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.UI.SpecialtyResources {

    public class SpecialtyResourceSummaryDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform ResourceSummaryPrefab;
        [SerializeField] private RectTransform ResourceSummaryContainer;

        private List<RectTransform> InstantiatedSummaries = new List<RectTransform>();



        private IGameCore GameCore;

        private IResourceAssignmentCanon AssignmentCanon;

        private ISpecialtyResourcePossessionLogic ResourcePossessionCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IGameCore gameCore, IResourceAssignmentCanon assignmentCanon,
            ISpecialtyResourcePossessionLogic resourcePossessionCanon
        ){
            GameCore                = gameCore;
            AssignmentCanon         = assignmentCanon;
            ResourcePossessionCanon = resourcePossessionCanon;
        }

        #region Unity messages

        private void OnEnable() {
            foreach(KeyValuePair<ISpecialtyResourceDefinition, int> resourcePair in
                ResourcePossessionCanon.GetFullResourceSummaryForCiv(GameCore.ActiveCivilization)
            ) {
                BuildSummary(resourcePair.Key, resourcePair.Value);
            }
        }

        private void OnDisable() {
            for(int i = InstantiatedSummaries.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedSummaries[i].gameObject);
            }

            InstantiatedSummaries.Clear();
        }

        #endregion

        private void BuildSummary(ISpecialtyResourceDefinition resource, int totalCopies) {
            int freeCopies = AssignmentCanon.GetFreeCopiesOfResourceForCiv(resource, GameCore.ActiveCivilization);

            var newSummaryPrefab = Instantiate(ResourceSummaryPrefab);

            newSummaryPrefab.gameObject.SetActive(true);
            newSummaryPrefab.transform.SetParent(ResourceSummaryContainer, false);
            InstantiatedSummaries.Add(newSummaryPrefab);

            var newSummary = newSummaryPrefab.GetComponent<SpecialtyResourceDefinitionSummary>();

            newSummary.Initialize(resource, freeCopies, totalCopies);
        }

        #endregion

    }

}
