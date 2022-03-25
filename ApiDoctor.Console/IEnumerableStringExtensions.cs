/*
 * API Doctor
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the ""Software""), to deal in
 * the Software without restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the
 * Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

namespace ApiDoctor.ConsoleApp
{
    using System.Collections.Generic;

    public static class IEnumerableStringExtensions
    {
        public static IEnumerable<string> RemoveMultipleBlankLines(this IEnumerable<string> lines)
        {
            var newLines = new List<string>(lines);

            var blankIndex = newLines.FindIndex(0, s => string.IsNullOrWhiteSpace(s));

            while (blankIndex >= 0)
            {
                var nextLine = blankIndex + 1;
                while (nextLine < newLines.Count && string.IsNullOrWhiteSpace(newLines[nextLine]))
                {
                    newLines.RemoveAt(nextLine);
                }

                blankIndex = nextLine < newLines.Count ? newLines.FindIndex(nextLine, s => string.IsNullOrWhiteSpace(s)) : -1;
            }

            // Because File.WriteAllLines adds a new line at the end, remove any trailing
            // blank line to avoid having two blank lines at the end
            if (string.IsNullOrWhiteSpace(newLines[newLines.Count - 1]))
            {
                newLines.RemoveAt(newLines.Count-1);
            }

            return newLines;
        }
    }
}
