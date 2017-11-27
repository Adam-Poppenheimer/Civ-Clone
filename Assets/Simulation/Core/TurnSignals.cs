using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Core {

    public class TurnBeganSignal : Signal<TurnBeganSignal, int> { }
    public class TurnEndedSignal : Signal<TurnEndedSignal, int> { }

}
