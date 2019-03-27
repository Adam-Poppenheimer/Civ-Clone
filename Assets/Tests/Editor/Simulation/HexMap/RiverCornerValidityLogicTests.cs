using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.HexMap {

    public class RiverCornerValidityLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class AreCornerFlowsValidTestData {

            public RiverFlow  CenterRight;
            public RiverFlow? CenterLeft;
            public RiverFlow? LeftRight;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable AreCornerFlowsValidTestCases {
            get {
                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = null, LeftRight = null

                }).SetName("CenterRight Clockwise, CenterLeft null, LeftRight null").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = null, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Clockwise, CenterLeft null, LeftRight Clockwise").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = null, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Clockwise, CenterLeft null, LeftRight Counterclockwise").Returns(false);


                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = null

                }).SetName("CenterRight Clockwise, CenterLeft Clockwise, LeftRight null").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Clockwise, CenterLeft Clockwise, LeftRight Clockwise").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Clockwise, CenterLeft Clockwise, LeftRight Counterclockwise").Returns(true);


                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = null

                }).SetName("CenterRight Clockwise, CenterLeft Counterclockwise, LeftRight null").Returns(false);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Clockwise, CenterLeft Counterclockwise, LeftRight Clockwise").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Clockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Clockwise, CenterLeft Counterclockwise, LeftRight Counterclockwise").Returns(false);



                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = null, LeftRight = null

                }).SetName("CenterRight Counterclockwise, CenterLeft null, LeftRight null").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = null, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft null, LeftRight Clockwise").Returns(false);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = null, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft null, LeftRight Counterclockwise").Returns(true);


                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = null

                }).SetName("CenterRight Counterclockwise, CenterLeft Clockwise, LeftRight null").Returns(false);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft Clockwise, LeftRight Clockwise").Returns(false);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Clockwise, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft Clockwise, LeftRight Counterclockwise").Returns(true);


                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = null

                }).SetName("CenterRight Counterclockwise, CenterLeft Counterclockwise, LeftRight null").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = RiverFlow.Clockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft Counterclockwise, LeftRight Clockwise").Returns(true);

                yield return new TestCaseData(new AreCornerFlowsValidTestData() {
                    CenterRight = RiverFlow.Counterclockwise, CenterLeft = RiverFlow.Counterclockwise, LeftRight = RiverFlow.Counterclockwise

                }).SetName("CenterRight Counterclockwise, CenterLeft Counterclockwise, LeftRight Counterclockwise").Returns(true);
            }
        }

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<RiverCornerValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("AreCornerFlowsValidTestCases")]
        public bool AreCornerFlowsValidTests(AreCornerFlowsValidTestData testData) {
            var validityLogic = Container.Resolve<RiverCornerValidityLogic>();

            return validityLogic.AreCornerFlowsValid(testData.CenterRight, testData.CenterLeft, testData.LeftRight);
        }

        #endregion

        #endregion

    }

}
