using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.Core {

    /// <summary>
    /// A signal that should fire whenever a new turn is begun.
    /// </summary>
    public class TurnBeganSignal : Signal<TurnBeganSignal, int> { }

    /// <summary>
    /// A signal that should fire whenever the old turn has ended.
    /// </summary>
    public class TurnEndedSignal : Signal<TurnEndedSignal, int> { }

}
