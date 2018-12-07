﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Simulation.Cities.ResourceGeneration {

    public class CityCenterYieldLogic : ICityCenterYieldLogic {

        #region instance fields and properties

        private IIncomeModifierLogic                          IncomeModifierLogic;
        private ICityConfig                                   CityConfig;
        private IPossessionRelationship<ICivilization, ICity> CityPossessionCanon;
        private IUnitGarrisonLogic                            UnitGarrisonLogic;
        private ICityModifiers                                CityModifiers;
        

        #endregion

        #region constructors

        [Inject]
        public CityCenterYieldLogic(
            IIncomeModifierLogic incomeModifierLogic, ICityConfig cityConfig,
            IPossessionRelationship<ICivilization, ICity> cityPossessionCanon,
            IUnitGarrisonLogic unitGarrisonLogic, ICityModifiers cityModifiers
        ) {
            IncomeModifierLogic = incomeModifierLogic;
            CityPossessionCanon = cityPossessionCanon;
            CityConfig          = cityConfig;
            UnitGarrisonLogic   = unitGarrisonLogic;
            CityModifiers       = cityModifiers;
        }

        #endregion

        #region instance methods

        #region from ICityCenterYieldLogic

        public YieldSummary GetYieldOfCityCenter(ICity city) {
            YieldSummary centerYield = CityConfig.CityCenterBaseYield;

            centerYield += new YieldSummary(science: 1) * city.Population;

            centerYield += CityModifiers.BonusYield.GetValueForCity(city);

            if(UnitGarrisonLogic.IsCityGarrisoned(city)) {
                centerYield += CityModifiers.GarrisonedYield.GetValueForCity(city);
            }

            var cityMultipliers = IncomeModifierLogic.GetYieldMultipliersForCity(city);
            var civMultipliers  = IncomeModifierLogic.GetYieldMultipliersForCivilization(CityPossessionCanon.GetOwnerOfPossession(city));

            centerYield *= (YieldSummary.Ones + cityMultipliers + civMultipliers);

            return centerYield;
        }

        #endregion

        #endregion
        
    }

}
