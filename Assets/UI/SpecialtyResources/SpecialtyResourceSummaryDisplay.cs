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

using UnityCustomUtilities.Extensions;

namespace Assets.UI.SpecialtyResources {

    public class SpecialtyResourceSummaryDisplay : MonoBehaviour {

        #region instance fields and properties

        [SerializeField] private RectTransform ResourceSummaryPrefab;
        [SerializeField] private RectTransform ResourceSummaryContainer;

        private List<RectTransform> InstantiatedSummaries = new List<RectTransform>();



        private IGameCore                         GameCore;
        private IFreeResourcesLogic               FreeResourcesLogic;
        private IResourceExtractionLogic          ExtractionLogic;
        private IResourceTransferCanon            ResourceTransferCanon;
        IEnumerable<ISpecialtyResourceDefinition> AvailableResources;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            IGameCore gameCore, IFreeResourcesLogic freeResourcesLogic,
            IResourceExtractionLogic extractionLogic, IResourceTransferCanon resourceTransferCanon,
            [Inject(Id = "Available Specialty Resources")] IEnumerable<ISpecialtyResourceDefinition> availableResources
        ){
            GameCore              = gameCore;
            FreeResourcesLogic    = freeResourcesLogic;
            ExtractionLogic       = extractionLogic;
            ResourceTransferCanon = resourceTransferCanon;
            AvailableResources    = availableResources;
        }

        #region Unity messages

        private void OnEnable() {
            var activeCiv = GameCore.ActiveCivilization;

            foreach(var resource in AvailableResources) {
                var extractedCopies = ExtractionLogic      .GetExtractedCopiesOfResourceForCiv(resource, activeCiv);
                var importedCopies  = ResourceTransferCanon.GetImportedCopiesOfResourceForCiv (resource, activeCiv);
                var exportedCopies  = ResourceTransferCanon.GetExportedCopiesOfResourceForCiv (resource, activeCiv);

                BuildSummary(resource, extractedCopies + importedCopies - exportedCopies);
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
            int freeCopies = FreeResourcesLogic.GetFreeCopiesOfResourceForCiv(resource, GameCore.ActiveCivilization);

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
