using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units {

    public class GreatPersonFactory : IGreatPersonFactory {

        #region instance fields and properties

        private ICapitalCityCanon                        CapitalCityCanon;
        private IPossessionRelationship<IHexCell, ICity> CityLocationCanon;
        private IUnitFactory                             UnitFactory;
        private IUnitConfig                              UnitConfig;
        private IHexGrid                                 Grid;
        private CivilizationSignals                      CivSignals;

        #endregion

        #region constructors

        [Inject]
        public GreatPersonFactory(
            ICapitalCityCanon capitalCityCanon, IPossessionRelationship<IHexCell, ICity> cityLocationCanon,
            IUnitFactory unitFactory, IUnitConfig unitConfig, IHexGrid grid, CivilizationSignals civSignals
        ) {
            CapitalCityCanon  = capitalCityCanon;
            CityLocationCanon = cityLocationCanon;
            UnitFactory       = unitFactory;
            UnitConfig        = unitConfig;
            Grid              = grid;
            CivSignals        = civSignals;
        }

        #endregion

        #region instance methods

        #region from IGreatPersonFactory

        public IUnit BuildGreatPerson(GreatPersonType type, ICivilization owner) {
            var civCapital      = CapitalCityCanon .GetCapitalOfCiv(owner);
            var capitalLocation = CityLocationCanon.GetOwnerOfPossession(civCapital);

            var personTemplate = UnitConfig.GetTemplateForGreatPersonType(type);

            var location = GetValidNearbyCell(capitalLocation, personTemplate, owner);

            var newPerson = UnitFactory.BuildUnit(location, personTemplate, owner);

            CivSignals.GreatPersonBornSignal.OnNext(new GreatPersonBirthData() {
                Owner = owner, GreatPerson = newPerson, Type = type
            });
            
            return newPerson;
        }

        #endregion

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
