using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.UI {

    public static class StringFormats {

        #region static fields and properties

        public static readonly string StockpileAndIncomeDisplayFormat = "<sprite index={0}> {1} ({2:+0;-#})";

        public static readonly string IncomeDisplayFormat = "<sprite index={0}> {1:+0;-#}";

        public static readonly string ResourceNodeSummary_Strategic = "<sprite name=\"Resource Placeholder Icon\"> {0} ({2})";

        public static readonly string ResourceNodeSummary_NonStrategic = "<sprite name=\"Resource Placeholder Icon\"> {0}";

        #endregion

    }

}
