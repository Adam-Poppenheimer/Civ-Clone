using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Visibility;
using Assets.Simulation.Civilizations;

namespace Assets.UI.Cities {

    public class CitySummaryManager {

        #region instance fields and properties

        private List<CitySummaryDisplay> InstantiatedSummaries = new List<CitySummaryDisplay>();



        private CitySummaryDisplay                       SummaryPrefab;
        private ICityFactory                             CityFactory;
        private DiContainer                              Container;
        private ICityUIConfig                            CityUIConfig;
        private IGameCamera                              GameCamera;
        private RectTransform                            CitySummaryContainer;
        private IExplorationCanon                        ExplorationCanon;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public CitySummaryManager(
            CitySummaryDisplay summaryPrefab, ICityFactory cityFactory,
            DiContainer container, ICityUIConfig cityUIConfig, IGameCamera gameCamera,
            [Inject(Id = "City Summary Container")] RectTransform citySummaryContainer,
            CitySignals citySignals, IExplorationCanon explorationCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            VisibilitySignals visibilitySignals
        ){
            SummaryPrefab        = summaryPrefab;
            CityFactory          = cityFactory;
            Container            = container;
            CityUIConfig         = cityUIConfig;
            GameCamera           = gameCamera;
            CitySummaryContainer = citySummaryContainer;
            ExplorationCanon     = explorationCanon;
            CityLocationCanon    = cityLocationCanon;

            citySignals.CityBeingDestroyedSignal           .Subscribe(OnCityBeingDestroyed);
            visibilitySignals.CellBecameExploredByCivSignal.Subscribe(OnCellBecameExploredByCiv);
        }

        #endregion

        #region instance methods

        public void BuildSummaries() {
            foreach(var city in CityFactory.AllCities) {
                var cityLocation = CityLocationCanon.GetOwnerOfPossession(city);

                if(!ExplorationCanon.IsCellExplored(cityLocation)) {
                    continue;
                }

                BuildSummaryForCity(city);
            }
        }

        public void RepositionSummaries() {
            foreach(var summary in InstantiatedSummaries) {
                var cityInScreen = Camera.main.WorldToScreenPoint(summary.ObjectToDisplay.Position);

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

        private void BuildSummaryForCity(ICity city) {
            var newSummary = Container.InstantiatePrefabForComponent<CitySummaryDisplay>(SummaryPrefab);

            newSummary.transform.SetParent(CitySummaryContainer, false);
            newSummary.gameObject.SetActive(true);

            newSummary.ObjectToDisplay = city;
            newSummary.Refresh();

            InstantiatedSummaries.Add(newSummary);
        }

        private void OnCityBeingDestroyed(ICity city) {
            var summaryDisplayingCity = InstantiatedSummaries.Where(summary => summary.ObjectToDisplay == city).FirstOrDefault();
            if(summaryDisplayingCity != null) {
                GameObject.Destroy(summaryDisplayingCity.gameObject);
                InstantiatedSummaries.Remove(summaryDisplayingCity);
            }            
        }

        private void OnCellBecameExploredByCiv(Tuple<IHexCell, ICivilization> data) {
            var cityAtCell = CityLocationCanon.GetPossessionsOfOwner(data.Item1).FirstOrDefault();

            if(cityAtCell != null && ExplorationCanon.IsCellExplored(data.Item1)) {
                BuildSummaryForCity(cityAtCell);
            }
        }

        #endregion

    }

}
