using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Core;

namespace Assets.Simulation.Cities.Buildings {

    public class FreeBuildingsResponder : IPlayModeSensitiveElement {

        #region instance fields and properties

        #region from IFreeBuildingsResponder

        public bool IsActive {
            get { return _isActive; }
            set {
                if(value != _isActive) {
                    _isActive = value;

                    if(_isActive) {
                        CityGainedBuildingSubscription = CitySignals.GainedBuilding.Subscribe(OnCityGainedBuilding);
                    }else {
                        CityGainedBuildingSubscription.Dispose();
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private IDisposable CityGainedBuildingSubscription;




        private CitySignals                      CitySignals;
        private IBuildingFactory                 BuildingFactory;
        private IBuildingProductionValidityLogic BuildingValidityLogic;

        #endregion

        #region constructors

        [Inject]
        public FreeBuildingsResponder(
            CitySignals citySignals, IBuildingFactory buildingFactory,
            IBuildingProductionValidityLogic buildingValidityLogic
        ) {
            CitySignals           = citySignals;
            BuildingFactory       = buildingFactory;
            BuildingValidityLogic = buildingValidityLogic;
        }

        #endregion

        #region instance methods

        private void OnCityGainedBuilding(Tuple<ICity, IBuilding> data) {
            var city             = data.Item1;
            var buildingTemplate = data.Item2.Template;

            foreach(var freeTemplate in buildingTemplate.FreeBuildings) {
                if(BuildingValidityLogic.IsTemplateValidForCity(freeTemplate, city)) {
                    BuildingFactory.BuildBuilding(freeTemplate, city);
                }
            }
        }

        #endregion

    }

}
