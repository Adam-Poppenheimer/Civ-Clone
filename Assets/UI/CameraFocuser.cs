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
using Assets.Simulation.Players;

namespace Assets.UI {

    public class CameraFocuser : ICameraFocuser {

        #region instance fields and properties

        private IDisposable TurnBeganSubscription;



        
        
        private ICapitalCityCanon                             CapitalCityCanon;
        private IGameCamera                                   GameCamera;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, IUnit> UnitPossessionCanon;
        private IUnitPositionCanon                            UnitPositionCanon;
        private ICanBuildCityLogic                            CanBuildCityLogic;

        #endregion

        #region constructors

        [Inject]
        public CameraFocuser(
            ICapitalCityCanon capitalCityCanon, IGameCamera gameCamera,
            IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, IUnit> unitPossessionCanon,
            IUnitPositionCanon unitPositionCanon, ICanBuildCityLogic canBuildCityLogic
        ) {
            CapitalCityCanon    = capitalCityCanon;
            GameCamera          = gameCamera;
            CityLocationCanon   = cityLocationCanon;
            UnitPossessionCanon = unitPossessionCanon;
            UnitPositionCanon   = unitPositionCanon;
            CanBuildCityLogic   = canBuildCityLogic;
        }

        #endregion

        #region instance methods

        #region from ICameraFocuser

        public void ReturnFocusToPlayer(IPlayer player) {
            var civOfPlayer = player.ControlledCiv;

            var capital = CapitalCityCanon.GetCapitalOfCiv(civOfPlayer);

            if(capital != null) {
                var capitalLocation = CityLocationCanon.GetOwnerOfPossession(capital);

                GameCamera.SnapToCell(capitalLocation);

            }else {
                var unitsOf = UnitPossessionCanon.GetPossessionsOfOwner(civOfPlayer);

                var cityBuilder = unitsOf.Where(unit => CanBuildCityLogic.CanUnitBuildCity(unit)).FirstOrDefault();

                if(cityBuilder != null) {
                    GameCamera.SnapToCell(UnitPositionCanon.GetOwnerOfPossession(cityBuilder));
                }else if(unitsOf.Any()) {
                    GameCamera.SnapToCell(UnitPositionCanon.GetOwnerOfPossession(unitsOf.FirstOrDefault()));
                }
            }
        }

        #endregion

        #endregion

    }

}
