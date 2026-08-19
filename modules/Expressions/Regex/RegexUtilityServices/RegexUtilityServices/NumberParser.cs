using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace RegexUtilityServices
{
    public static class NumberParser
    {

        /// <summary>
        /// check the nth occurrence of number is equal to a specified number or not. 
        /// </summary>
        /// <param name="input">The pattern to be matched</param>
        /// <param name="n">nth occurrence of number</param>
        /// <param name="a">The specified number</param>
        /// <returns>
        /// + returns true iff nth occurrence of number is equal to a specified number
        ///
        /// + returns false, otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// + argument `n` is passed as nonnegative number.
        /// 
        /// + argument `a` is passed as nonnegative number.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// + argument `input` is passed as null or empty or whitespace.
        /// </exception>
        public static bool CheckNthOccurrence(string input, int n, int a)
        {
            // input must be neither null nor empty nor whitespace.
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            // n must be a postive integer.
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);

            // a must be a postive integer.
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(a);

            MatchCollection matches = Regex.Matches(input, @"\d+");

            // 檢查是否有足夠的匹配次數 (注意：n 是從 1 開始算，所以索引要減 1)
            if (matches.Count >= n)
            {
                string foundValueStr = matches[n - 1].Value;
                
                if (int.TryParse(foundValueStr, out int foundValue))
                {
                    return foundValue == a;
                }
            }
            return false;
        }
    }
}