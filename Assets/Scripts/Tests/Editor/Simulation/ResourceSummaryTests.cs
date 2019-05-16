using System;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using UnityCustomUtilities.Extensions;

using Assets.Simulation;

namespace Assets.Tests.Simulation {

    [TestFixture]
    public class YieldSummaryTests : ZenjectUnitTestFixture{

        [Test(Description = "ResourceSummary addition should sum the corresponding components together. " + 
            "It should not modify its operands")]
        public void AdditionOperator_AddsComponents() {
            var summaryOne   = new YieldSummary(food: 3, production: 2, gold: 1);
            var summaryTwo   = new YieldSummary(production: 5, gold: 1, culture: 6);
            var summaryThree = new YieldSummary(food: 9, gold: 17, culture: 12);

            var copyOfOne   = new YieldSummary(summaryOne);
            var copyOfTwo   = new YieldSummary(summaryTwo);
            var copyOfThree = new YieldSummary(summaryThree);

            var onePlusTwo   = summaryOne + summaryTwo;
            var onePlusThree = summaryOne + summaryThree;
            var twoPlusThree = summaryTwo + summaryThree;

            foreach(var resourceType in EnumUtil.GetValues<YieldType>()) {
                Assert.AreEqual(summaryOne[resourceType] + summaryTwo[resourceType], onePlusTwo[resourceType],
                    "OnePlusTwo has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(summaryOne[resourceType] + summaryThree[resourceType], onePlusThree[resourceType],
                    "OnePlusThree has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(summaryTwo[resourceType] + summaryThree[resourceType], twoPlusThree[resourceType],
                    "TwoPlusThree has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(copyOfOne[resourceType], summaryOne[resourceType], 
                    "SummaryOne's value was changed by the addition operations");

                Assert.AreEqual(copyOfTwo[resourceType], summaryTwo[resourceType], 
                    "SummaryOne's value was changed by the addition operations");

                Assert.AreEqual(copyOfThree[resourceType], summaryThree[resourceType], 
                    "SummaryOne's value was changed by the addition operations");
            }
        }

        [Test(Description = "ResourceSummary subtraction should subtract the second summary's components from the " +
            "corresponding components of the second summary. This operation should not modify its operands")]
        public void SubtractionOperator_SubtractsComponents() {
            var summaryOne   = new YieldSummary(food: 3, production: 2, gold: 1);
            var summaryTwo   = new YieldSummary(production: 5, gold: 1, culture: 6);
            var summaryThree = new YieldSummary(food: 9, gold: 17, culture: 12);

            var copyOfOne   = new YieldSummary(summaryOne);
            var copyOfTwo   = new YieldSummary(summaryTwo);
            var copyOfThree = new YieldSummary(summaryThree);

            var oneMinusTwo   = summaryOne - summaryTwo;
            var oneMinusThree = summaryOne - summaryThree;
            var twoMinusThree = summaryTwo - summaryThree;

            foreach(var resourceType in EnumUtil.GetValues<YieldType>()) {
                Assert.AreEqual(summaryOne[resourceType] - summaryTwo[resourceType], oneMinusTwo[resourceType],
                    "OneMinusTwo has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(summaryOne[resourceType] - summaryThree[resourceType], oneMinusThree[resourceType],
                    "OneMinusThree has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(summaryTwo[resourceType] - summaryThree[resourceType], twoMinusThree[resourceType],
                    "TwoMinusThree has an incorrect value for resource type " + resourceType);

                Assert.AreEqual(copyOfOne[resourceType], summaryOne[resourceType], 
                    "SummaryOne's value was changed by the addition operations");

                Assert.AreEqual(copyOfTwo[resourceType], summaryTwo[resourceType], 
                    "SummaryTwo's value was changed by the addition operations");

                Assert.AreEqual(copyOfThree[resourceType], summaryThree[resourceType], 
                    "SummaryThree's value was changed by the addition operations");
            }
        }

        [Test(Description = "ResourceSummary multiplication should return a ResourceSummary whose values " +
            "are the product of the operands' corresponding fields (food * food, production * production, etc)")]
        public void MultiplicationOperatorSummaries_MultipliesComponents() {
            var firstSummary = new YieldSummary(food: 1, production: 2, gold: -1, culture: 0);
            var secondSummary = new YieldSummary(food: 3, production: 4, gold: 2, culture: 10);

            Assert.AreEqual(
                new YieldSummary(food: 3, production: 8, gold: -2, culture: 0),
                firstSummary * secondSummary,
                "Operator *(ResourceSummary, ResourceSummary) did not return a pairwise multiplication of its operands"
            );
        }

        [Test(Description = "Integer multiplication should multiply all components in the summary by the given coefficient. " + 
            "It should not modify its operands")]
        public void MultiplicationOperatorInt_MultipliesComponentsByCoefficient() {
            var summary = new YieldSummary(food: 3, production: 2, gold: 0);
            var copyOfSummary = new YieldSummary(summary);

            var multOne = summary * 6;
            var multTwo = summary * 19;
            var multThree = summary * -7;

            foreach(var resourceType in EnumUtil.GetValues<YieldType>()) {
                Assert.AreEqual(summary[resourceType] * 6,  multOne[resourceType],   "MultOne has an incorrect value");
                Assert.AreEqual(summary[resourceType] * 19, multTwo[resourceType],   "MultTwo has an incorrect value");
                Assert.AreEqual(summary[resourceType] * -7, multThree[resourceType], "MultThree has an incorrect value");

                Assert.AreEqual(copyOfSummary[resourceType], summary[resourceType], 
                    "Summary's value was changed by the addition operations");
            }
        }

        [Test(Description = "Float multiplication should multiply all components in the summary by the given coefficient, " +
            "rounding the resulting values. It should not modify its operands")]
        public void MultiplicationOperatorFloat_MultipliesComponentsByCoefficientAndRounds() {
            var summary = new YieldSummary(food: 1, production: 1, gold: 1);
            var copyOfSummary = new YieldSummary(summary);

            var multOne = summary * 1f;
            var multTwo = summary * 1.4f;
            var multThree = summary * -1.7f;

            foreach(var resourceType in EnumUtil.GetValues<YieldType>()) {
                Assert.AreEqual(summary[resourceType] * 1f,    multOne[resourceType],   "MultOne has an incorrect value");
                Assert.AreEqual(summary[resourceType] * 1.4f,  multTwo[resourceType],   "MultTwo has an incorrect value");
                Assert.AreEqual(summary[resourceType] * -1.7f, multThree[resourceType], "MultThree has an incorrect value");

                Assert.AreEqual(copyOfSummary[resourceType], summary[resourceType], 
                    "Summary's value was changed by the addition operations");
            }
        }

        [Test(Description = "Float division should divide all components in the summary by the given coefficient. " +
            "It should not modify its operands")]
        public void DivisionOperatorFloat_DividesComponentsByCoefficientAndRounds() {
            var summary = new YieldSummary(food: 2, production: 2, gold: 2);
            var copyOfSummary = new YieldSummary(summary);

            var divOne = summary / 2f;
            var divTwo = summary / 3.5f;
            var divThree = summary / -4.2f;

            foreach(var resourceType in EnumUtil.GetValues<YieldType>()) {
                Assert.AreEqual(summary[resourceType] / 2f,    divOne[resourceType],   "DivOne has an incorrect value");
                Assert.AreEqual(summary[resourceType] / 3.5f,  divTwo[resourceType],   "DivTwo has an incorrect value");
                Assert.AreEqual(summary[resourceType] / -4.2f, divThree[resourceType], "DivThree has an incorrect value");

                Assert.AreEqual(copyOfSummary[resourceType], summary[resourceType], 
                    "Summary's value was changed by the addition operations");
            }
        }

        [Test(Description = "The Total property should return the sum of all resources in the YieldSummary")]
        public void Total_ReturnsSumOfAllYields() {
            var summary = new YieldSummary(food: 4, production: 9, gold: -3, culture: 0);

            Assert.AreEqual(10, summary.Total, "Summary has an incorrect Total value");
        }
        
    }

}

