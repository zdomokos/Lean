﻿/*
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
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;
using QuantConnect.Lean.Engine.DataFeeds;
using QuantConnect.Logging;

namespace QuantConnect.Lean.Engine.Setup
{
    /// <summary>
    ///  Base class that provides shared code for
    /// the <see cref="ISetupHandler"/> implementations
    /// </summary>
    public static class BaseSetupHandler
    {
        /// <summary>
        /// Will first check and add all the required conversion rate securities
        /// and later will seed an initial value to them.
        /// </summary>
        /// <param name="algorithm">The algorithm instance</param>
        /// <param name="universeSelection">The universe selection instance</param>
        public static void SetupCurrencyConversions(
            IAlgorithm algorithm,
            UniverseSelection universeSelection)
        {
            // this is needed to have non-zero currency conversion rates during warmup
            // will also set the Cash.ConversionRateSecurity
            universeSelection.EnsureCurrencyDataFeeds(SecurityChanges.None);

            // now set conversion rates
            var cashToUpdate = algorithm.Portfolio.CashBook.Values
                .Where(x => x.ConversionRateSecurity != null && x.ConversionRate == 0)
                .ToList();

            var historyRequestFactory = new HistoryRequestFactory(algorithm);
            var historyRequests = new List<HistoryRequest>();
            foreach (var cash in cashToUpdate)
            {
                // if we already added a history request for this security, skip
                if (historyRequests.Any(x => x.Symbol == cash.ConversionRateSecurity.Symbol))
                {
                    continue;
                }

                var configs = algorithm
                    .SubscriptionManager
                    .SubscriptionDataConfigService
                    .GetSubscriptionDataConfigs(cash.ConversionRateSecurity.Symbol);

                var resolution = configs.GetHighestResolution();
                var startTime = historyRequestFactory.GetStartTimeAlgoTz(
                    cash.ConversionRateSecurity.Symbol,
                    1,
                    resolution,
                    cash.ConversionRateSecurity.Exchange.Hours);
                var endTime = algorithm.Time.RoundDown(resolution.ToTimeSpan());

                // we need to order and select a specific configuration type
                // so the conversion rate is deterministic
                var configToUse = configs.OrderBy(x => x.TickType).First();

                historyRequests.Add(historyRequestFactory.CreateHistoryRequest(
                    configToUse,
                    startTime,
                    endTime,
                    cash.ConversionRateSecurity.Exchange.Hours,
                    resolution));
            }

            var slices = algorithm.HistoryProvider.GetHistory(historyRequests, algorithm.TimeZone);
            slices.PushThrough(data =>
            {
                foreach (var cash in cashToUpdate
                    .Where(x => x.ConversionRateSecurity.Symbol == data.Symbol))
                {
                    cash.Update(data);
                }
            });

            Log.Trace("BaseSetupHandler.SetupCurrencyConversions():" +
                $"{Environment.NewLine}{algorithm.Portfolio.CashBook}");
        }
    }
}
