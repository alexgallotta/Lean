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
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace QuantConnect.Data.Auxiliary
{
    /// <summary>
    /// Represents a single row in a map_file. This is a csv file ordered as {date, mapped symbol}
    /// </summary>
    public class MapFileRow : IEquatable<MapFileRow>
    {
        /// <summary>
        /// Gets the date associated with this data
        /// </summary>
        public DateTime Date { get; }

        /// <summary>
        /// Gets the mapped symbol
        /// </summary>
        public string MappedSymbol { get; }

        /// <summary>
        /// Gets the mapped symbol
        /// </summary>
        public Exchange PrimaryExchange { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MapFileRow"/> class.
        /// </summary>
        public MapFileRow(DateTime date, string mappedSymbol, string primaryExchange, string market = QuantConnect.Market.USA)
            : this(date, mappedSymbol, primaryExchange.GetPrimaryExchange(SecurityType.Equity, market))
        { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MapFileRow"/> class.
        /// </summary>
        public MapFileRow(DateTime date, string mappedSymbol, Exchange primaryExchange = null)
        {
            Date = date;
            MappedSymbol = mappedSymbol.LazyToUpper();
            PrimaryExchange = primaryExchange ?? Exchange.UNKNOWN;
        }

        /// <summary>
        /// Reads in the map_file for the specified equity symbol
        /// </summary>
        public static IEnumerable<MapFileRow> Read(string file, string market)
        {
            return File.Exists(file)
                ? File.ReadAllLines(file).Where(l => !string.IsNullOrWhiteSpace(l)).Select(s => Parse(s, market))
                : Enumerable.Empty<MapFileRow>();
        }

        /// <summary>
        /// Parses the specified line into a MapFileRow
        /// </summary>
        public static MapFileRow Parse(string line, string market)
        {
            var csv = line.Split(',');
            var primaryExchange = Exchange.UNKNOWN;
            if (csv.Length == 3)
            {
                primaryExchange = csv[2].GetPrimaryExchange(SecurityType.Equity, market);
            }

            return new MapFileRow(DateTime.ParseExact(csv[0], DateFormat.EightCharacter, null), csv[1], primaryExchange);
        }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(MapFileRow other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Date.Equals(other.Date) &&
                   string.Equals(MappedSymbol, other.MappedSymbol) &&
                   string.Equals(PrimaryExchange, other.PrimaryExchange);
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MapFileRow)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Date.GetHashCode() * 397) ^ 
                       (MappedSymbol != null ? MappedSymbol.GetHashCode() : 0) ^
                       (PrimaryExchange.GetHashCode());
            }
        }

        /// <summary>
        /// Determines whether or not the two instances are equal
        /// </summary>
        public static bool operator ==(MapFileRow left, MapFileRow right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether or not the two instances are not equal
        /// </summary>
        public static bool operator !=(MapFileRow left, MapFileRow right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Writes this row to csv format
        /// </summary>
        public string ToCsv()
        {
            var encodedExchange = PrimaryExchange == Exchange.UNKNOWN? string.Empty : $",{PrimaryExchange.Code}";
            return $"{Date.ToStringInvariant(DateFormat.EightCharacter)},{MappedSymbol.ToLowerInvariant()}{encodedExchange}";
        }

        /// <summary>
        /// Convert this row into string form
        /// </summary>
        /// <returns>resulting string</returns>
        public override string ToString()
        {
            var mainExchange = PrimaryExchange == Exchange.UNKNOWN ? string.Empty : $" - {PrimaryExchange}";
            return Date.ToShortDateString() + ": " + MappedSymbol + mainExchange;
        }
    }
}
