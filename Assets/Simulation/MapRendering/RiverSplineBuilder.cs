using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEditor;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class RiverSplineBuilder : IRiverSplineBuilder {

        #region instance fields and properties

        #region from IRiverSplineBuilder

        public ReadOnlyCollection<RiverSpline> LastBuiltRiverSplines {
            get { return RiverSplines.AsReadOnly(); }
        }
        private List<RiverSpline> RiverSplines = new List<RiverSpline>();

        #endregion




        private IMapRenderConfig    RenderConfig;
        private IRiverAssemblyCanon RiverAssemblyCanon;

        #endregion

        #region constructors

        [Inject]
        public RiverSplineBuilder(
            IMapRenderConfig renderConfig, IRiverAssemblyCanon riverAssemblyCanon
        ) {
            RenderConfig       = renderConfig;
            RiverAssemblyCanon = riverAssemblyCanon;
        }

        #endregion

        #region instance methods

        #region from IRiverSplineBuilder

        public void RefreshRiverSplines() {
            RiverSplines.Clear();
             
            AssembleRiverSplines();
        }

        #endregion

        private void AssembleRiverSplines() {
            RiverAssemblyCanon.RefreshRivers();

            foreach(var river in RiverAssemblyCanon.Rivers) {
                RiverSection section = river[0];

                var centerSpline = new BezierSpline(section.FlowFromOne == RiverFlow.Clockwise ? section.Start : section.End);

                Vector3 centerToV1Direction, centerToV4Direction;

                Vector3 v1Tangent, v4Tangent;

                Vector3 controlOne, controlTwo;

                Vector3 v1, v4;

                bool isV1Internal, isV4Internal;

                bool isControlOneNegative, isControlTwoNegative;

                for(int i = 0; i < river.Count; i++) {
                    section = river[i];
                    
                    if(section.FlowFromOne == RiverFlow.Clockwise) {
                        v1 = section.Start;
                        v4 = section.End;

                        isV1Internal = section.PreviousOnInternalCurve;
                        isV4Internal = section.NextOnInternalCurve;

                    }else {
                        v1 = section.End;
                        v4 = section.Start;

                        isV1Internal = section.NextOnInternalCurve;
                        isV4Internal = section.PreviousOnInternalCurve;
                    }

                    if(isV1Internal) {
                        centerToV1Direction = (v1 - section.AdjacentCellOne.AbsolutePosition).normalized;
                    }else {
                        centerToV1Direction = (v1 - section.AdjacentCellTwo.AbsolutePosition).normalized;
                    }

                    if(isV4Internal) {
                        centerToV4Direction = (v4 - section.AdjacentCellOne.AbsolutePosition).normalized;
                    }else {
                        centerToV4Direction = (v4 - section.AdjacentCellTwo.AbsolutePosition).normalized;
                    }

                    isControlOneNegative = (section.FlowFromOne == RiverFlow.Clockwise        &&  isV1Internal) ||
                                           (section.FlowFromOne == RiverFlow.Counterclockwise && !isV1Internal);

                    isControlTwoNegative = (section.FlowFromOne == RiverFlow.Clockwise        && !isV4Internal) ||
                                           (section.FlowFromOne == RiverFlow.Counterclockwise &&  isV4Internal);

                    v1Tangent = new Vector3(-centerToV1Direction.z, centerToV1Direction.y, centerToV1Direction.x);
                    v4Tangent = new Vector3(-centerToV4Direction.z, centerToV4Direction.y, centerToV4Direction.x);

                    controlOne = v1 + v1Tangent * RenderConfig.RiverCurveStrength * (isControlOneNegative ? -1f : 1f);
                    controlTwo = v4 + v4Tangent * RenderConfig.RiverCurveStrength * (isControlTwoNegative ? -1f : 1f);

                    centerSpline.AddCubicCurve(controlOne, controlTwo, v4);
                }

                var newRiverSpline = new RiverSpline() {
                    CenterSpline = centerSpline
                };

                RiverSplines.Add(newRiverSpline);
            }
        }

        #endregion
        
    }

}
