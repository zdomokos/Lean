/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using System.Threading;
using NUnit.Framework;
using QuantConnect.Lean.Engine.DataFeeds;

namespace QuantConnect.Tests.Engine.DataFeeds
{
    [TestFixture]
    public class FrontierAwareTimeProviderTests
    {
        [Test]
        public void FrontierRemainsConstant()
        {
            var timeProvider = new FrontierAwareTimeProvider(new RealTimeProvider());
            var frontier = timeProvider.FrontierTimeProvider;
            var nowBeforeSleep = frontier.GetUtcNow();
            Thread.Sleep(10);
            var nowAfterSleep = frontier.GetUtcNow();
            Assert.AreEqual(nowBeforeSleep, nowAfterSleep);
        }

        [Test]
        public void FrontierIsUpdatedCorrectly()
        {
            var manualTimeProvider = new ManualTimeProvider();
            manualTimeProvider.SetCurrentTimeUtc(new DateTime(9,9,9));
            var timeProvider = new FrontierAwareTimeProvider(manualTimeProvider);

            var frontier = timeProvider.FrontierTimeProvider;
            var nowBefore = frontier.GetUtcNow();

            manualTimeProvider.SetCurrentTimeUtc(new DateTime(9, 9, 10));
            timeProvider.GetUtcNow();

            var nowAfter = frontier.GetUtcNow();
            Assert.Greater(nowAfter, nowBefore);
            Assert.AreEqual(new DateTime(9, 9, 9), nowBefore);
            Assert.AreEqual(new DateTime(9, 9, 10), nowAfter);
        }

        [Test]
        public void FrontierReturnsCorrectValue()
        {
            var manualTimeProvider = new ManualTimeProvider();
            manualTimeProvider.SetCurrentTimeUtc(new DateTime(9, 9, 9));
            var timeProvider = new FrontierAwareTimeProvider(manualTimeProvider);

            var result = timeProvider.GetUtcNow();
            Assert.AreEqual(new DateTime(9, 9, 9), result);

            manualTimeProvider.SetCurrentTimeUtc(new DateTime(9, 9, 10));
            result = timeProvider.GetUtcNow();
            Assert.AreEqual(new DateTime(9, 9, 10), result);
        }
    }
}
