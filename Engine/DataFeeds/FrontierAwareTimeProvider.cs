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

namespace QuantConnect.Lean.Engine.DataFeeds
{
    /// <summary>
    /// This time provider will expose a frontier time provider and will update it when queried, using provided time provider
    /// </summary>
    public class FrontierAwareTimeProvider : ITimeProvider
    {
        private readonly ManualTimeProvider _manualTimeProvider;
        private readonly ITimeProvider _timeProvider;

        /// <summary>
        /// The frontier time provider
        /// </summary>
        public ITimeProvider FrontierTimeProvider => _manualTimeProvider;

        /// <summary>
        /// Creates a new instance of the FrontierAwareTimeProvider
        /// </summary>
        public FrontierAwareTimeProvider(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _manualTimeProvider = new ManualTimeProvider(timeProvider.GetUtcNow());
        }

        /// <summary>
        /// Updates and gets the current frontier time
        /// </summary>
        public DateTime GetUtcNow()
        {
            var now = _timeProvider.GetUtcNow();
            _manualTimeProvider.SetCurrentTimeUtc(now);
            return now;
        }
    }
}
