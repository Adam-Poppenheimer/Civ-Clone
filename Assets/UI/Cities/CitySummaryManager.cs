﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CitySummaryManager {

        #region instance fields and properties

        private List<CitySummaryDisplay> InstantiatedSummaries = new List<CitySummaryDisplay>();



        private CitySummaryDisplay SummaryPrefab;

        private ICityFactory CityFactory;

        private DiContainer Container;

        private ICityUIConfig CityUIConfig;

        private GameCamera GameCamera;

        private RectTransform CitySummaryContainer;

        #endregion

        #region constructors

        [Inject]
        public CitySummaryManager(
            CitySummaryDisplay summaryPrefab, ICityFactory cityFactory,
            DiContainer container, ICityUIConfig cityUIConfig, GameCamera gameCamera,
            [Inject(Id = "City Summary Container")] RectTransform citySummaryContainer,
            CitySignals citySignals
        ){
            SummaryPrefab        = summaryPrefab;
            CityFactory          = cityFactory;
            Container            = container;
            CityUIConfig         = cityUIConfig;
            GameCamera           = gameCamera;
            CitySummaryContainer = citySummaryContainer;

            citySignals.CityBeingDestroyedSignal.Subscribe(OnCityBeingDestroyed);
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

        public void RepositionSummaries() {
            foreach(var summary in InstantiatedSummaries) {
                var cityInScreen = Camera.main.WorldToScreenPoint(summary.ObjectToDisplay.transform.position);

                summary.RectTransform.position = cityInScreen +
                    new Vector3(0f, CityUIConfig.SummaryVerticalOffsetBase)
                    * (GameCamera.Zoom / (1 + CityUIConfig.SummaryZoomGapReductionStrength));
            }
        }

        public void ClearSummaries() {
            foreach(var summary in new List<CitySummaryDisplay>(InstantiatedSummaries)) {
                GameObject.Destroy(summary.gameObject);
            }

            InstantiatedSummaries.Clear();
        }

        private void OnCityBeingDestroyed(ICity city) {
            var summaryDisplayingCity = InstantiatedSummaries.Where(summary => summary.ObjectToDisplay == city).FirstOrDefault();
            if(summaryDisplayingCity != null) {
                GameObject.Destroy(summaryDisplayingCity.gameObject);
                InstantiatedSummaries.Remove(summaryDisplayingCity);
            }            
        }

        #endregion

    }

}
