using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using UnityCustomUtilities.Extensions;

using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;

namespace Assets.Simulation.Cities.Production {

    public class CityProductionResolver : ICityProductionResolver {

        #region internal types

        private class ProductionRequest {

            public IProductionProject Project;
            public ICity              RequestingCity;

        }

        #endregion

        #region instance fields and properties

        private DictionaryOfLists<IBuildingTemplate, ProductionRequest> ProductionRequestsByTemplate = 
            new DictionaryOfLists<IBuildingTemplate, ProductionRequest>();




        private CitySignals                                   CitySignals;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICityFactory                                  CityFactory;
        private IBuildingProductionValidityLogic              BuildingValidityLogic;

        #endregion

        #region constructors

        [Inject]
        public CityProductionResolver(
            CitySignals citySignals, IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICityFactory cityFactory, IBuildingProductionValidityLogic buildingValidityLogic,
            CoreSignals coreSignals
        ) {
            CitySignals           = citySignals;
            CityPossessionCanon   = cityPossessionCanon;
            CityFactory           = cityFactory;
            BuildingValidityLogic = buildingValidityLogic;

            CitySignals.CityGainedBuildingSignal.Subscribe(OnCityGainedBuilding);
            coreSignals.RoundBegan.Subscribe(OnRoundBegan);
        }

        #endregion

        #region instance methods

        #region from ICityProductionResolver

        public void MakeProductionRequest(
            IProductionProject project, ICity requestingCity
        ) {
            if(project.UnitToConstruct != null) {
                project.Execute(requestingCity);

            }else if(project.BuildingToConstruct != null && project.BuildingToConstruct.Type == BuildingType.Normal) {
                project.Execute(requestingCity);

            }else if(project.BuildingToConstruct != null) {
                ProductionRequestsByTemplate.AddElementToList(
                    project.BuildingToConstruct,
                    new ProductionRequest() {
                        Project        = project,
                        RequestingCity = requestingCity,
                    }
                );
            }
        }

        public void ResolveBuildingConstructionRequests() {
            foreach(var wonderTemplate in ProductionRequestsByTemplate.Keys) {
                if(wonderTemplate.Type == BuildingType.NationalWonder) {
                    foreach(var requestsFromCiv in SeparateRequestsByCiv(ProductionRequestsByTemplate[wonderTemplate])) {
                        ResolveRequests(requestsFromCiv);
                    }
                }else if(wonderTemplate.Type == BuildingType.WorldWonder) {
                    ResolveRequests(ProductionRequestsByTemplate[wonderTemplate]);
                }
            }

            ProductionRequestsByTemplate.Clear();
        }

        #endregion

        private void ResolveRequests(IEnumerable<ProductionRequest> requests) {
            ProductionRequest requestToCarryOut = null;

            foreach(var nextRequest in requests) {
                if(requestToCarryOut == null || requestToCarryOut.Project.Progress < nextRequest.Project.Progress) {
                    requestToCarryOut = nextRequest;
                }
            }

            if(requestToCarryOut != null) {
                requestToCarryOut.Project.Execute(requestToCarryOut.RequestingCity);
            }
        }

        private IEnumerable<List<ProductionRequest>> SeparateRequestsByCiv(List<ProductionRequest> requests) {
            var requestsByCiv = new DictionaryOfLists<ICivilization, ProductionRequest>();

            foreach(var request in requests) {
                var cityOwner = CityPossessionCanon.GetOwnerOfPossession(request.RequestingCity);

                requestsByCiv.AddElementToList(cityOwner, request);
            }

            return requestsByCiv.Values;
        }

        private void OnCityGainedBuilding(UniRx.Tuple<ICity, IBuilding> data) {
            foreach(var otherCity in CityFactory.AllCities.Where(city => city != data.Item1)) {
                var buildingInProgress = otherCity.ActiveProject == null ? null : otherCity.ActiveProject.BuildingToConstruct;

                if(buildingInProgress != null && !BuildingValidityLogic.IsTemplateValidForCity(buildingInProgress, otherCity)) {
                    otherCity.ActiveProject = null;
                }
            }
        }

        private void OnRoundBegan(int turn) {
            ResolveBuildingConstructionRequests();
        }

        #endregion

    }

}
