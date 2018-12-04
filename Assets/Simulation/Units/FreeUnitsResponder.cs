using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.HexMap;
using Assets.Simulation.Core;

namespace Assets.Simulation.Units {

    public class FreeUnitsResponder : IPlayModeSensitiveElement {

        #region instance fields and properties

        #region from IFreeUnitsResponder

        public bool IsActive {
            get { return _isActive; }
            set {
                if(_isActive != value) {
                    _isActive = value;

                    if(_isActive) {
                        SignalSubscriptions.Add(CivSignals.CivUnlockedPolicy    .Subscribe(OnCivUnlockedPolicy));
                        SignalSubscriptions.Add(CivSignals.CivUnlockedPolicyTree.Subscribe(OnCivUnlockedPolicyTree));
                        SignalSubscriptions.Add(CivSignals.CivFinishedPolicyTree.Subscribe(OnCivFinishedPolicyTree));

                        SignalSubscriptions.Add(CitySignals.CityGainedBuildingSignal.Subscribe(OnCityGainedBuilding));

                    }else {
                        SignalSubscriptions.ForEach(subscription => subscription.Dispose());

                        SignalSubscriptions.Clear();
                    }
                }
            }
        }
        private bool _isActive;

        #endregion

        private List<IDisposable> SignalSubscriptions = new List<IDisposable>();




        private CivilizationSignals                           CivSignals;
        private CitySignals                                   CitySignals;
        private IUnitFactory                                  UnitFactory;
        private IHexGrid                                      Grid;
        private IPossessionRelationship<IHexCell, ICity>      CityLocationCanon;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private ICapitalCityCanon                             CapitalCityCanon;

        #endregion

        #region constructors

        [Inject]
        public FreeUnitsResponder(
            CivilizationSignals civSignals, CitySignals citySignals, IUnitFactory unitFactory,
            IHexGrid grid, IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            ICapitalCityCanon capitalCityCanon
        ) {
            CivSignals          = civSignals;
            CitySignals         = citySignals;
            UnitFactory         = unitFactory;
            Grid                = grid;
            CityLocationCanon   = cityLocationCanon;
            CityPossessionCanon = cityPossessionCanon;
            CapitalCityCanon    = capitalCityCanon;
        }

        #endregion

        #region instance methods

        private void OnCivUnlockedPolicy(Tuple<ICivilization, ISocialPolicyDefinition> data) {
            var civ    = data.Item1;
            var policy = data.Item2;

            BuildUnitsFromBonuses(civ, policy.Bonuses);
        }

        private void OnCivUnlockedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            BuildUnitsFromBonuses(civ, tree.UnlockingBonuses);
        }

        private void OnCivFinishedPolicyTree(Tuple<ICivilization, IPolicyTreeDefinition> data) {
            var civ  = data.Item1;
            var tree = data.Item2;

            BuildUnitsFromBonuses(civ, tree.CompletionBonuses);
        }

        private void OnCityGainedBuilding(Tuple<ICity, IBuilding> data) {
            var city     = data.Item1;
            var building = data.Item2;

            var cityLocation = CityLocationCanon  .GetOwnerOfPossession(city);
            var cityOwner    = CityPossessionCanon.GetOwnerOfPossession(city);

            foreach(var templateToBuild in building.Template.FreeUnits) {
                var validCell = GetValidNearbyCell(cityLocation, templateToBuild, cityOwner);

                UnitFactory.BuildUnit(validCell, templateToBuild, cityOwner);
            }
        }




        private void BuildUnitsFromBonuses(ICivilization civ, ISocialPolicyBonusesData bonuses) {
            var civCapital = CapitalCityCanon.GetCapitalOfCiv(civ);

            var capitalLocation = CityLocationCanon.GetOwnerOfPossession(civCapital);

            foreach(var templateToBuild in bonuses.FreeUnits) {
                var validCell = GetValidNearbyCell(capitalLocation, templateToBuild, civ);

                UnitFactory.BuildUnit(validCell, templateToBuild, civ);
            }
        }

        private IHexCell GetValidNearbyCell(IHexCell centerCell, IUnitTemplate template, ICivilization owner) {
            for(int i = 0; i < 10; i++) {
                foreach(var nearbyCell in Grid.GetCellsInRing(centerCell, i)) {
                    if(UnitFactory.CanBuildUnit(nearbyCell, template, owner)) {
                        return nearbyCell;
                    }
                }
            }

            throw new InvalidOperationException("There is no cell within 10 cells of the argued location that can support this person");
        }

        #endregion

    }

}
