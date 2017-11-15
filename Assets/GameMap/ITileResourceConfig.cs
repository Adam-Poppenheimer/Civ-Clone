﻿using System;

namespace Assets.GameMap {

    public interface ITileResourceConfig {

        #region properties

        ResourceSummary DesertYield { get; }
        ResourceSummary ForestYield { get; }
        ResourceSummary GrasslandsYield { get; }
        ResourceSummary HillsYield { get; }
        ResourceSummary PlainsYield { get; }

        #endregion

    }

}