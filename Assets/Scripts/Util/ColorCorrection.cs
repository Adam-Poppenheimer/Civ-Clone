using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace Assets.Util {

    public static class ColorCorrection {

        #region static methods

        public static Color32 ARGB_To_RGBA(Color32 falseARGB) {
            return new Color32(falseARGB.g, falseARGB.b, falseARGB.a, falseARGB.r);
        }

        public static Color ARGB_To_RGBA(Color falseARGB) {
            return new Color(falseARGB.g, falseARGB.b, falseARGB.a, falseARGB.r);
        }

        #endregion

    }

}
