using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Cities.Production;

namespace Assets.Simulation.Cities {

    public class CityClickedSignal : Signal<CityClickedSignal, ICity> { }

    public class CityProjectChangedSignal : Signal<CityProjectChangedSignal, ICity, IProductionProject> { }

}
