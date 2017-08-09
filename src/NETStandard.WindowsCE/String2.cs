// Ref: https://github.com/dotnet/coreclr
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

#if NET35_CF
namespace System
#else
namespace Mock.System
#endif
{
    [Flags]
    public enum StringSplitOptions
    {
        None = 0,
        RemoveEmptyEntries = 1,
    }

    public static class String2
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        public static int Compare(string strA, int indexA, string strB, int indexB, int length)
            => string.Compare(strA, indexA, strB, indexB, length);

        public static int Compare(string strA, int indexA, string strB, int indexB, int length, StringComparison comparisonType)
            => string.Compare(strA, indexA, strB, indexB, length, comparisonType);

        public static int Compare(string strA, string strB)
            => string.Compare(strA, strB);

        public static int Compare(string strA, string strB, StringComparison comparisonType)
            => string.Compare(strA, strB, comparisonType);

        public static int CompareOrdinal(string strA, int indexA, string strB, int indexB, int length)
            => string.CompareOrdinal(strA, indexA, strB, indexB, length);

        public static int CompareOrdinal(string strA, string strB)
            => string.CompareOrdinal(strA, strB);

        public static string Concat<T>(IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            using (IEnumerator<T> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                // We called MoveNext once, so this will be the first item
                T currentValue = en.Current;
                string firstString = currentValue?.ToString();

                // If there's only 1 item, simply call ToString on that
                if (!en.MoveNext())
                {
                    // We have to handle the case of either currentValue
                    // or its ToString being null
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new StringBuilder(firstString);
                do
                {
                    currentValue = en.Current;

                    if (currentValue != null)
                        result.Append(currentValue.ToString());

                } while (en.MoveNext());

                return result.ToString();
            }
        }

        public static string Concat(IEnumerable<string> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            string[] castValues = values as string[];
            if (castValues != null)
                return string.Concat(castValues);

            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                // We called MoveNext once, so this will be the first item
                string firstString = en.Current;

                // If there's only 1 item, simply return that
                if (!en.MoveNext())
                {
                    // We have to handle the case of currentValue
                    // being null
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new StringBuilder(firstString);
                do
                {
                    result.Append(en.Current);
                } while (en.MoveNext());

                return result.ToString();
            }
        }

        public static string Concat(object arg0)
            => string.Concat(arg0);

        public static string Concat(object arg0, object arg1)
            => string.Concat(arg0, arg1);

        public static string Concat(object arg0, object arg1, object arg2)
            => string.Concat(arg0, arg1, arg2);

        public static string Concat(params object[] args)
            => string.Concat(args);

        public static string Concat(string str0, string str1)
            => string.Concat(str0, str1);

        public static string Concat(string str0, string str1, string str2)
            => string.Concat(str0, str1, str2);

        public static string Concat(string str0, string str1, string str2, string str3)
            => string.Concat(str0, str1, str2, str3);

        public static string Concat(params string[] values)
            => string.Concat(values);

        public static bool Equals(string a, string b)
            => string.Equals(a, b);

        public static bool Equals(string a, string b, StringComparison comparisonType)
            => string.Equals(a, b, comparisonType);

        public static string Format(IFormatProvider provider, string format, params object[] args)
            => string.Format(provider, format, args);

        public static string Format(string format, params object[] args)
            => string.Format(format, args);

        public static bool IsNullOrEmpty(string value)
            => string.IsNullOrEmpty(value);

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
                return true;

            int len = value.Length;
            if (len == 0)
                return true;

            for (int i = 0; i < len; i++)
            {
                if (value[i] != ' ')
                    return false;
            }

            return true;
        }

        public static string Join<T>(string separator, IEnumerable<T> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            using (IEnumerator<T> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                // We called MoveNext once, so this will be the first item
                T currentValue = en.Current;
                string firstString = currentValue?.ToString();

                // If there's only 1 item, simply call ToString on that
                if (!en.MoveNext())
                {
                    // We have to handle the case of either currentValue
                    // or its ToString being null
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new StringBuilder(firstString);
                do
                {
                    currentValue = en.Current;
                    result.Append(separator);
                    if (currentValue != null)
                        result.Append(currentValue.ToString());

                } while (en.MoveNext());

                return result.ToString();
            }
        }

        public static string Join(string separator, IEnumerable<string> values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            string[] castValues = values as string[];
            if (castValues != null)
                return string.Join(separator, castValues);

            using (IEnumerator<string> en = values.GetEnumerator())
            {
                if (!en.MoveNext())
                    return string.Empty;

                // We called MoveNext once, so this will be the first item
                string firstString = en.Current;

                // If there's only 1 item, simply return that
                if (!en.MoveNext())
                {
                    // We have to handle the case of currentValue
                    // being null
                    return firstString ?? string.Empty;
                }

                StringBuilder result = new StringBuilder(firstString);
                do
                {
                    result.Append(separator);
                    result.Append(en.Current);

                } while (en.MoveNext());

                return result.ToString();
            }
        }

        public static string Join(string separator, params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length == 0)
                return string.Empty;

            string firstString = values[0]?.ToString();

            if (values.Length == 1)
                return firstString ?? string.Empty;

            StringBuilder result = new StringBuilder(firstString);
            for (int i = 1; i < values.Length; i++)
            {
                result.Append(separator);
                object value = values[i];

                if (values != null)
                    result.Append(value.ToString());
            };

            return result.ToString();
        }

        public static string Join(string separator, params string[] value)
            => string.Join(separator, value);

        public static string Join(string separator, string[] value, int startIndex, int count)
            => string.Join(separator, value, startIndex, count);

        public static string Remove(this string s, int startIndex)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    "The start index cannot be less than zero");
            }

            if (startIndex >= s.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex),
                    "The start index must be less than string length");
            }

            return s.Substring(0, startIndex);
        }

        public static string[] Split(this string s, char[] separator, int count)
            => Split(s, separator, count, StringSplitOptions.None);

        public static string[] Split(this string s, char[] separator, int count, StringSplitOptions options)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "The counting cannot be negative");
            }

            if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
                throw new ArgumentException("Illegal value for string split options");

            bool omitEmptyEntries = (options == StringSplitOptions.RemoveEmptyEntries);
            if (count == 0 || (omitEmptyEntries && s.Length == 0))
                return new string[0];

            if (count == 1)
                return new string[] { s };

            int[] sepList = new int[s.Length];
            int numReplaces = MakeSeparatorList(s, separator, sepList);

            // Handle the special case of no replaces.
            if (numReplaces == 0)
                return new string[] { s };

            if (omitEmptyEntries)
                return SplitOmitEmptyEntries(s, sepList, null, 1, numReplaces, count);
            else
                return SplitKeepEmptyEntries(s, sepList, null, 1, numReplaces, count);
        }

        public static string[] Split(this string s, char[] separator, StringSplitOptions options)
            => Split(s, separator, int.MaxValue, options);

        public static string[] Split(this string s, string[] separator, int count, StringSplitOptions options)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count),
                    "The counting cannot be negative");
            }

            if (options < StringSplitOptions.None || options > StringSplitOptions.RemoveEmptyEntries)
                throw new ArgumentException("Illegal value for string split options");

            bool omitEmptyEntries = (options == StringSplitOptions.RemoveEmptyEntries);
            bool singleSeparator = (separator != null) ? separator.Length == 1 : false;

            if (!singleSeparator && (separator == null || separator.Length == 0))
                return Split(s, (char[])null, count, options);

            if (count == 0 || (omitEmptyEntries && s.Length == 0))
                return new string[0];

            if (count == 1 || (singleSeparator && separator[0].Length == 0))
                return new string[] { s };

            int[] sepList = new int[s.Length];
            int[] lengthList;
            int defaultLength;
            int numReplaces;

            if (singleSeparator)
            {
                lengthList = null;
                defaultLength = separator[0].Length;
                numReplaces = MakeSeparatorList(s, separator[0], sepList);
            }
            else
            {
                lengthList = new int[s.Length];
                defaultLength = 0;
                numReplaces = MakeSeparatorList(s, separator, sepList, lengthList);
            }

            // Handle the special case of no replaces.
            if (numReplaces == 0)
                return new string[] { s };

            if (omitEmptyEntries)
                return SplitOmitEmptyEntries(s, sepList, lengthList, defaultLength, numReplaces, count);
            else
                return SplitKeepEmptyEntries(s, sepList, lengthList, defaultLength, numReplaces, count);
        }

        public static string[] Split(this string s, string[] separator, StringSplitOptions options)
            => Split(s, separator, int.MaxValue, options);

        public static char[] ToCharArray(this string s, int startIndex, int length)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            // Range check everything.
            if (startIndex < 0 || startIndex > s.Length || startIndex > s.Length - length)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Index out of allowable range");
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "The number of elements cannot be less than zero");

            if (length > 0)
            {
                char[] chars = new char[length];
                for (int i = 0; i < length; i++)
                    chars[i] = s[i + startIndex];

                return chars;
            }

            return new char[0];
        }

        public static string ToLowerInvariant(this string s)
            => s.ToLower(InvariantCulture);

        public static string ToUpperInvariant(this string s)
            => s.ToUpper(InvariantCulture);


        //--------------------------------------------------------------------    
        // This function returns the number of the places within this instance where 
        // characters in Separator occur.
        // Args: separator  -- A string containing all of the split characters.
        //       sepList    -- an array of ints for split char indices.
        //-------------------------------------------------------------------- 
        private static int MakeSeparatorList(string s, char[] separators, int[] sepList)
        {
            int foundCount = 0;

            if (separators == null || separators.Length == 0)
            {
                for (int i = 0; i < s.Length && foundCount < sepList.Length; i++)
                {
                    if (char.IsWhiteSpace(s[i]))
                        sepList[foundCount++] = i;
                }
            }
            else
            {
                int sepListCount = sepList.Length;
                for (int i = 0; i < s.Length && foundCount < sepListCount; i++)
                {
                    char current = s[i];
                    for (int j = 0; j < separators.Length; j++)
                    {
                        if (current == separators[j])
                        {
                            sepList[foundCount++] = i;
                            break;
                        }
                    }
                }
            }

            return foundCount;
        }

        //--------------------------------------------------------------------
        // This function returns number of the places within baseString where
        // instances of the separator string occurs.
        // Args: separator  -- the separator
        //       sepList    -- an array of ints for split string indices.
        //--------------------------------------------------------------------
        private static int MakeSeparatorList(string s, string separator, int[] sepList)
        {
            int foundCount = 0;
            int sepListCount = sepList.Length;
            int currentSepLength = separator.Length;

            for (int i = 0; i < s.Length && foundCount < sepListCount; i++)
            {
                if (s[i] == separator[0] && currentSepLength <= s.Length - i)
                {
                    if (currentSepLength == 1 ||
                        string.CompareOrdinal(s, i, separator, 0, currentSepLength) == 0
                        )
                    {
                        sepList[foundCount++] = i;
                        i += currentSepLength - 1;
                    }
                }
            }

            return foundCount;
        }

        //--------------------------------------------------------------------    
        // This function returns the number of the places within this instance where 
        // instances of separator strings occur.
        // Args: separators -- An array containing all of the split strings.
        //       sepList    -- an array of ints for split string indices.
        //       lengthList -- an array of ints for split string lengths.
        //--------------------------------------------------------------------    
        private static int MakeSeparatorList(string s, string[] separators, int[] sepList, int[] lengthList)
        {
            int foundCount = 0;
            int sepListCount = sepList.Length;
            int sepCount = separators.Length;

            for (int i = 0; i < s.Length && foundCount < sepListCount; i++)
            {
                for (int j = 0; j < separators.Length; j++)
                {
                    string separator = separators[j];
                    if (string.IsNullOrEmpty(separator))
                        continue;

                    int currentSepLength = separator.Length;
                    if (s[i] == separator[0] && currentSepLength <= s.Length - i)
                    {
                        if (currentSepLength == 1 ||
                            string.CompareOrdinal(s, i, separator, 0, currentSepLength) == 0
                            )
                        {
                            sepList[foundCount] = i;
                            lengthList[foundCount] = currentSepLength;
                            foundCount++;
                            i += currentSepLength - 1;
                            break;
                        }
                    }
                }
            }

            return foundCount;
        }

        private static string[] SplitKeepEmptyEntries(string s, int[] sepList, int[] lengthList, int defaultLength, int numReplaces, int count)
        {
            // Note a special case in this function:
            //     If there is no separator in the string, a string array which only contains 
            //     the original string will be returned regardless of the count. 
            //

            int currIndex = 0;
            int arrIndex = 0;

            count--;
            int numActualReplaces = (numReplaces < count) ? numReplaces : count;

            //Allocate space for the new array.
            //+1 for the string from the end of the last replace to the end of the String.
            string[] splitStrings = new string[numActualReplaces + 1];

            for (int i = 0; i < numActualReplaces && currIndex < s.Length; i++)
            {
                splitStrings[arrIndex++] = s.Substring(currIndex, sepList[i] - currIndex);
                currIndex = sepList[i] + ((lengthList == null) ? defaultLength : lengthList[i]);
            }

            //Handle the last string at the end of the array if there is one.
            if (currIndex < s.Length && numActualReplaces >= 0)
                splitStrings[arrIndex] = s.Substring(currIndex);
            else if (arrIndex == numActualReplaces)
                //We had a separator character at the end of a string.  Rather than just allowing
                //a null character, we'll replace the last element in the array with an empty string.
                splitStrings[arrIndex] = string.Empty;

            return splitStrings;
        }

        // This function will not keep the Empty String 
        private static string[] SplitOmitEmptyEntries(string s, int[] sepList, int[] lengthList, int defaultLength, int numReplaces, int count)
        {
            // Allocate array to hold items. This array may not be 
            // filled completely in this function, we will create a 
            // new array and copy string references to that new array.

            int maxItems = (numReplaces < count) ? (numReplaces + 1) : count;
            string[] splitStrings = new string[maxItems];

            int currIndex = 0;
            int arrIndex = 0;

            for (int i = 0; i < numReplaces && currIndex < s.Length; i++)
            {
                if (sepList[i] - currIndex > 0)
                    splitStrings[arrIndex++] = s.Substring(currIndex, sepList[i] - currIndex);

                currIndex = sepList[i] + ((lengthList == null) ? defaultLength : lengthList[i]);
                if (arrIndex == count - 1)
                {
                    // If all the remaining entries at the end are empty, skip them
                    while (i < numReplaces - 1 && currIndex == sepList[++i])
                    {
                        currIndex += ((lengthList == null) ? defaultLength : lengthList[i]);
                    }
                    break;
                }
            }

            // we must have at least one slot left to fill in the last string.
            Debug.Assert(arrIndex < maxItems);

            //Handle the last string at the end of the array if there is one.
            if (currIndex < s.Length)
                splitStrings[arrIndex++] = s.Substring(currIndex);

            string[] stringArray = splitStrings;
            if (arrIndex != maxItems)
            {
                stringArray = new string[arrIndex];
                for (int j = 0; j < arrIndex; j++)
                    stringArray[j] = splitStrings[j];
            }

            return stringArray;
        }
    }
}
