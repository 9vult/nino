﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Localizer
{
    internal static partial class StringParser
    {
        [GeneratedRegex(@"\{\{([^\|]+)\|([0-9]+)\}\}")]
        private static partial Regex Interpolation();

        /// <summary>
        /// Parse a string to get the interpolatable parts
        /// </summary>
        /// <param name="input">Input string</param>
        /// <returns>List of parts</returns>
        /// <exception cref="LocalizationException">An error occured</exception>
        public static List<Part> Parse(string input)
        {
            List<Part> parts = [];
            try
            {
                var matches = Interpolation().Matches(input);
                foreach (Match match in matches.Cast<Match>())
                {
                    parts.Add(new Part
                    {
                        // match.Groups[0] == match.Value
                        Name = match.Groups[1].Value,
                        Index = int.Parse(match.Groups[2].Value),
                        Match = match.Value
                    });
                }
            }
            catch (Exception e)
            {
                throw new LocalizationException("An error occured during processing:", e);
            }
            return parts;
        }

        /// <summary>
        /// Interpolate into the parent string
        /// </summary>
        /// <param name="input">The parent string</param>
        /// <param name="part">Part to replace</param>
        /// <param name="value">Value to replace with</param>
        /// <returns>The updated string</returns>
        public static string Interpolate(string input, Part part, object value)
        {
            return input.Replace(part.Match, value.ToString());
        }
    }

    /// <summary>
    /// Represents the part of a string that's a interpolation target
    /// </summary>
    internal class Part
    {
        /// <summary>
        /// Name of the value
        /// </summary>
        public required string Name { get; set; }
        /// <summary>
        /// Index of the value
        /// </summary>
        public required int Index { get; set; }
        /// <summary>
        /// Full match
        /// </summary>
        public required string Match { get; set; }
    }
}