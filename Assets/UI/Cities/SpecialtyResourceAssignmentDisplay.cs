using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class SpecialtyResourceAssignmentDisplay : CityDisplayBase {

        #region instance fields and properties

        [SerializeField] private Text ResourceNamePrefab;
        [SerializeField] private RectTransform ResourceNameContainer;

        private List<RectTransform> InstantiatedPrefabs = new List<RectTransform>();



        private IResourceAssignmentCanon ResourceAssignmentCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IResourceAssignmentCanon resourceAssignmentCanon) {
            ResourceAssignmentCanon = resourceAssignmentCanon;
        }

        #region from CityDisplayBase

        public override void Refresh() {
            if(ObjectToDisplay == null) {
                return;
            }

            for(int i = InstantiatedPrefabs.Count - 1; i >= 0; i--) {
                Destroy(InstantiatedPrefabs[i].gameObject);
            }
            InstantiatedPrefabs.Clear();

            foreach(var resource in ResourceAssignmentCanon.GetAllResourcesAssignedToCity(ObjectToDisplay)) {
                var newPrefab = Instantiate(ResourceNamePrefab);
                
                newPrefab.gameObject.SetActive(true);
                newPrefab.transform.SetParent(ResourceNameContainer);

                InstantiatedPrefabs.Add(newPrefab.rectTransform);

                newPrefab.text = resource.name;
            }
        }

        #endregion

        #endregion

    }

}
