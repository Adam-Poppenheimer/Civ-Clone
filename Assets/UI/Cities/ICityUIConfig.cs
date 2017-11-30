using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.UI.Cities {

    public interface ICityUIConfig {

        #region properties

        Material OccupiedSlotMaterial { get; }
        Material UnoccupiedSlotMaterial { get; }
        Material LockedSlotMaterial { get; }

        #endregion

    }

}
