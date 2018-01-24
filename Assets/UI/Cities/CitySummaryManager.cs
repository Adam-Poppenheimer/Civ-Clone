﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CitySummaryManager {

        #region instance fields and properties

        private List<CitySummaryDisplay> InstantiatedSummaries = new List<CitySummaryDisplay>();



        private CitySummaryDisplay SummaryPrefab;

        private ICityFactory CityFactory;

        private DiContainer Container;

        private RectTransform CitySummaryContainer; 

        #endregion

        #region constructors

        [Inject]
        public CitySummaryManager(
            CitySummaryDisplay summaryPrefab, ICityFactory cityFactory,
            DiContainer container,
            [Inject(Id = "City Summary Container")] RectTransform citySummaryContainer
        ){
            SummaryPrefab        = summaryPrefab;
            CityFactory          = cityFactory;
            Container            = container;
            CitySummaryContainer = citySummaryContainer;
        }

        #endregion

        #region instance methods

        public void BuildSummaries() {
            foreach(var city in CityFactory.AllCities) {
                var newSummary = Container.InstantiatePrefabForComponent<CitySummaryDisplay>(SummaryPrefab);

                newSummary.transform.SetParent(CitySummaryContainer, false);
                newSummary.gameObject.SetActive(true);

                newSummary.ObjectToDisplay = city;
                newSummary.Refresh();

                InstantiatedSummaries.Add(newSummary);
            }
        }

        public void RefreshSummaries() {
            foreach(var summary in InstantiatedSummaries) {
                summary.Refresh();
            }
        }

        public void ClearSummaries() {
            foreach(var summary in new List<CitySummaryDisplay>(InstantiatedSummaries)) {
                GameObject.Destroy(summary.gameObject);
            }

            InstantiatedSummaries.Clear();
        }

        #endregion

    }

}
