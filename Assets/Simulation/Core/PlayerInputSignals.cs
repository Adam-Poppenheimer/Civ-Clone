using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.Simulation.Core {

    public class EndTurnRequestedSignal : Signal<EndTurnRequestedSignal> { }

}
