using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Cities.UI {

    public interface ICityUIConfig {

        #region properties

        Material OccupiedSlotMaterial { get; }
        Material UnoccupiedSlotMaterial { get; }

        #endregion

    }

}
