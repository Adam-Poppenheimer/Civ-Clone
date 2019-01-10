﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.Barbarians {

    public class EncampmentLocationCanon : PossessionRelationship<IHexCell, IEncampment>, IEncampmentLocationCanon {

        #region instance fields and properties

        private IImprovementLocationCanon ImprovementLocationCanon;

        #endregion

        #region constructors

        [Inject]
        public EncampmentLocationCanon(IImprovementLocationCanon improvementLocationCanon) {
            ImprovementLocationCanon = improvementLocationCanon;
        }

        #endregion

        #region instance methods

        #region from PossessionRelationship<IHexCell, IEncampment>

        protected override bool IsPossessionValid(IEncampment encampment, IHexCell cell) {
            return (cell == null) || CanCellAcceptAnEncampment(cell);
        }

        protected override void DoOnPossessionEstablished(IEncampment possession, IHexCell newOwner) {
            newOwner.RefreshSelfOnly();
        }

        protected override void DoOnPossessionBroken(IEncampment possession, IHexCell oldOwner) {
            oldOwner.RefreshSelfOnly();
        }

        #endregion

        #region from IEncampmentLocationCanon

        public bool CanCellAcceptAnEncampment(IHexCell cell) {
            return !cell.Terrain.IsWater()
                && cell.Shape != CellShape.Mountains
                && !GetPossessionsOfOwner(cell).Any()
                && !ImprovementLocationCanon.GetPossessionsOfOwner(cell).Any();
        }

        #endregion

        #endregion

    }

}
