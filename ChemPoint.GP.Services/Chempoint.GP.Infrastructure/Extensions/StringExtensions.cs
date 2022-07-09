using System;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// True, if string has value
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>Boolean</returns>
        public static bool IsValid(this string str)
        {
            return !str.IsInValid();
        }

        /// <summary>
        /// True, if string is null or empty
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>Boolean</returns>
        public static bool IsInValid(this string str)
        {
            return (str == null || str.Trim() == string.Empty);
        }

        /// <summary>
        /// True, if the given string is equal to stringToCompare.
        /// Note that this is a case-insensitive comparison.
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="stringToCompare">comparison string</param>
        /// <returns>Boolean</returns>
        public static bool IsEqualTo(this string str, string stringToCompare)
        {
            if (str.IsInValid() || stringToCompare.IsInValid())
            {
                if (str.IsInValid() || stringToCompare.IsInValid())
                    return true;
                else
                    return false;
            }
            else
            {
                return str.Equals(stringToCompare, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// True, if the given string is not equal to stringToCompare.
        /// Note that this is a case-insensitive comparison.
        /// Converse method for IsEqualTo().
        /// </summary>
        /// <param name="str">source string</param>
        /// <param name="stringToCompare">comparison string</param>
        /// <returns>Boolean</returns>
        public static bool IsNotEqualTo(this string str, string stringToCompare)
        {
            return !str.IsEqualTo(stringToCompare);
        }

        /// <summary>
        /// True, if the given string is a Guid.
        /// False, otherwise.
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>Boolean</returns>
        public static bool IsGuid(this string str)
        {
            Guid resolvedGuid = new Guid();
            return Guid.TryParse(str, out resolvedGuid);
        }

        /// <summary>
        /// True, if the given string is not a Guid.
        /// Converse method for IsGuid().
        /// </summary>
        /// <param name="str">a string</param>
        /// <returns>Boolean</returns>
        public static bool IsNotGuid(this string str)
        {
            return !str.IsGuid();
        }

        /// <summary>
        /// Gives the matching enum for the given string provided enum type is mandatory.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="str">The STR.</param>
        /// <returns>TEnum</returns>
        public static TEnum ToEnum<TEnum>(this string str) where TEnum : struct
        {
            TEnum enumValue = default(TEnum);

            Enum.TryParse(str, true, out enumValue);
            return enumValue;
        }

        /// <summary>
        /// Converts value of given string to Int32.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>Int32</returns>
        public static int ToInt32(this string str)
        {
            int intEquiv = default(int);

            Int32.TryParse(str, out intEquiv);
            return intEquiv;
        }

        /// <summary>
        /// Converts value of given string to Int16.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>Int16</returns>
        public static Int16 ToInt16(this string str)
        {
            Int16 intEquiv = default(Int16);

            Int16.TryParse(str, out intEquiv);
            return intEquiv;
        }

        /// <summary>
        /// Converts value of given string to short.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>short</returns>
        public static Int16 ToShort(this string str)
        {
            short shortEquiv = default(short);

            short.TryParse(str, out shortEquiv);
            return shortEquiv;
        }

        /// <summary>
        /// Converts value of given string to Boolean.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>Boolean</returns>
        public static bool ToBool(this string str)
        {
            bool boolEquiv = default(bool);

            Boolean.TryParse(str, out boolEquiv);
            return boolEquiv;
        }

        /// <summary>
        /// Gives alternate value if the given string is not valid,
        /// else, string itself.
        /// </summary>
        /// <param name="str">string</param>
        /// <param name="alternateValue">alternate value to use</param>
        /// <returns>string</returns>
        public static string UseGivenIfNotAvailable(this string str, string alternateValue)
        {
            return str.IsValid() ? str : (alternateValue.IsValid() ? alternateValue : string.Empty);
        }

        /// <summary>
        /// Converts the given string to valid Guid
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>Guid</returns>
        public static Guid ToGuid(this string str)
        {
            if (str.IsGuid())
            {
                return Guid.Parse(str);
            }
            else
            {
                return Guid.Empty;
            }
        }

        /// <summary>
        /// Converts the given string to Nullable Guid.
        /// </summary>
        /// <param name="str">string</param>
        /// <returns>Nullable Guid</returns>
        public static Guid? ToNullableGuid(this string str)
        {
            if (str.IsGuid())
            {
                return Guid.Parse(str);
            }
            else
            {
                return default(Guid?);
            }
        }

        /// <summary>
        /// Method that truncates the string to the given length.
        /// </summary>
        /// <param name="source">
        /// A <see cref="string"/> that holds the source string.
        /// </param>
        /// <param name="length">
        /// A <see cref="int"/> that holds the length to truncate.
        /// </param>
        /// <remarks>
        /// If is given length is shorter than the string length, then the entire
        /// string will be returned.
        /// </remarks>
        /// <returns>
        /// A <see cref="string"/> that holds the truncated string.
        /// </returns>
        ///
        public static string Truncate(this string source, int length)
        {
            if (source.Length > length)
            {
                source = source.Substring(0, length);
            }
            return source;
        }

        /// <summary>
        /// Converts the given string to equivalent pascal case.
        /// </summary>
        /// <param name="source">source string</param>
        /// <returns>string converted to pascal case</returns>
        public static string ToPascalCase(this string source)
        {
            if (source.IsInValid())
                return source;

            if (source.Length == 1)
                return source.ToUpperInvariant();

            string pascalled = source.ToLowerInvariant();
            return char.ToUpperInvariant(pascalled[0]) + pascalled.Substring(1, pascalled.Length - 1);
        }

        /// <summary>
        /// checks if the given search text is contained in string. (Case Insensitive)
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="searchText">search string</param>
        /// <returns>true, if contains; false, otherwise.</returns>
        public static bool ContainsCaseInsensitive(this string source, string searchText)
        {
            return source.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        /// <summary>
        /// Surrounds the given string with the character or string given.
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="surroundChar">string or character used to surround the string with.</param>
        /// <returns>Encapsulated string.</returns>
        public static string SurroundWith(this string source, string surroundChar)
        {
            return string.Concat(surroundChar, source, surroundChar);
        }

        /// <summary>
        ///  Prepends the start char(string) to source and appends the end char(string) to the source.
        /// </summary>
        /// <param name="source">source string</param>
        /// <param name="surroundStartChar">character or string to prepend to source</param>
        /// <param name="surroundEndChar">character or string to append to source</param>
        /// <returns>Encapsulated string.</returns>
        public static string SurroundWith(this string source, string surroundStartChar, string surroundEndChar)
        {
            return string.Concat(surroundStartChar, source, surroundEndChar);
        }

        /// <summary>
        /// Splits the given string for time component.
        /// </summary>
        /// <param name="sourceWithTimeComponentOnly">string with time component</param>
        /// <returns>individual time components</returns>
        public static string[] SplitForTimeComponent(this string sourceWithTimeComponentOnly)
        {
            return sourceWithTimeComponentOnly.Split(':');
        }

        public static bool EqualsTo(this string first, string second)
        {
            if (first == null && second == null)
                return true;
            else if (first == null || second == null)
                return false;

            return first.Trim().Equals(second.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsTo(this object first, object second)
        {
            if (first == null && second == null)
                return true;

            return first.ToString().Trim().Equals(second.ToString().Trim(), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GetValueOrDefault(this string value, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                    return string.Empty;
                return defaultValue;
            }
            return value;
        }

        public static string ToLowerCase(this string value)
        {
            if (value == null)
                return null;
            else
                return value.Trim().ToLower();
        }

        public static string ConvertToString(this object value)
        {
            return value == null ? null : value.ToString().Trim();
        }
    }
}
