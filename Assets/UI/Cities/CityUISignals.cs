using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using UniRx;

using Assets.Simulation;

namespace Assets.UI.Cities {
    
    public class SlotDisplayClickedSignal : Signal<SlotDisplayClickedSignal, IWorkerSlotDisplay> { }

}
