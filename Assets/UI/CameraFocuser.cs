using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Core;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.UI {

    public class CameraFocuser : ICameraFocuser {

        #region instance fields and properties

        private IDisposable TurnBeganSubscription;



        
        private CoreSignals                                   CoreSignals;
        private ICapitalCityCanon                             CapitalCityCanon;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private IGameCamera                                   GameCamera;
        private ICanBuildCityLogic                            CanBuildCityLogic;
        private IGameCore                                     GameCore;

        #endregion

        #region constructors

        [Inject]
        public CameraFocuser(
            CoreSignals coreSignals, ICapitalCityCanon capitalCityCanon,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, IGameCamera gameCamera,
            ICanBuildCityLogic canBuildCityLogic, IGameCore gameCore
        ) {
            CoreSignals         = coreSignals;
            CapitalCityCanon    = capitalCityCanon;
            CityLocationCanon   = cityLocationCanon;
            UnitPossessionCanon = unitPossessionCanon;
            UnitPositionCanon   = unitPositionCanon;
            GameCamera          = gameCamera;
            CanBuildCityLogic   = canBuildCityLogic;
            GameCore            = gameCore;
        }

        #endregion

        #region instance methods

        #region from ICameraFocuser

        public void ActivateBeginTurnFocusing() {
            TurnBeganSubscription = CoreSignals.TurnBeganSignal.Subscribe(OnTurnBegan);

            OnTurnBegan(GameCore.ActiveCivilization);
        }

        public void DeactivateBeginTurnFocusing() {
            TurnBeganSubscription.Dispose();
        }

        #endregion

        private void OnTurnBegan(ICivilization activeCiv) {
            var capital = CapitalCityCanon.GetCapitalOfCiv(activeCiv);

            if(capital != null) {
                var capitalLocation = CityLocationCanon.GetOwnerOfPossession(capital);

                GameCamera.SnapToCell(capitalLocation);

            }else {
                var unitsOf = UnitPossessionCanon.GetPossessionsOfOwner(activeCiv);

                var cityBuilder = unitsOf.Where(unit => CanBuildCityLogic.CanUnitBuildCity(unit)).FirstOrDefault();

                if(cityBuilder != null) {
                    GameCamera.SnapToCell(UnitPositionCanon.GetOwnerOfPossession(cityBuilder));
                }else if(unitsOf.Any()) {
                    GameCamera.SnapToCell(UnitPositionCanon.GetOwnerOfPossession(unitsOf.FirstOrDefault()));
                }
            }
        }

        #endregion

    }

}
