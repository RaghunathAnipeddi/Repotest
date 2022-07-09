using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gives collection of each enum entry of the given enum
        /// </summary>
        /// <typeparam name="T">type of enum</typeparam>
        /// <returns>List of given enum</returns>
        public static List<T> ToList<T>() where T : struct
        {
            return (Enum.GetValues(typeof(T)).Cast<T>()).ToList();
        }

        /// <summary>
        /// Gives the text representation of this enum
        /// </summary>
        /// <typeparam name="T">type of enum</typeparam>
        /// <param name="thisEnum">enum instance</param>
        /// <returns>valid string equivalent of enum if defined, else null</returns>
        public static string GetStringValue<T>(this T thisEnum) where T : struct
        {
            if (Enum.IsDefined(typeof(T), thisEnum))
            {
                return ((T)thisEnum).ToString();
            }
            else
                return null;
        }

        /// <summary>
        /// Gives the integer(value) representation of this enum
        /// </summary>
        /// <typeparam name="T">type of enum</typeparam>
        /// <param name="thisEnum">enum instance</param>
        /// <returns>valid Int32 of enum if defined, else default of Int32</returns>
        public static Int32 GetIntValue<T>(this T thisEnum) where T : struct
        {
            if (Enum.IsDefined(typeof(T), thisEnum))
            {
                return Convert.ToInt32(((T)thisEnum));
            }
            else
                return default(Int32);
        }

        /// <summary>
        /// Gets the look up with each enum entry and its related description mentioned in its description.
        /// </summary>
        /// <typeparam name="TEnum">type of the enum</typeparam>
        /// <typeparam name="TOutputKey">type of the key in the output lookup. int or string preferred</typeparam>
        /// <returns>lookup of enum value and description</returns>
        public static Dictionary<object, string> GetDescriptionLookup<TEnum, TOutputKey>() where TEnum : struct
        {
            Dictionary<object, string> lookup = new Dictionary<object, string>();
            var enumLst = ToList<TEnum>();
            Type outputKeyType = typeof(TOutputKey);

            if (enumLst.IsValid())
            {
                foreach (var enumValue in enumLst)
                {
                    var description = enumValue.GetEnumDescription<TEnum>();

                    //if enums with invalid descriptions or no description, it can be ignored here.
                    if (outputKeyType == typeof(Int16) || outputKeyType == typeof(Int32) || outputKeyType == typeof(Int64))
                        lookup.Add(enumValue.GetIntValue(), description);
                    else
                        lookup.Add(enumValue.GetStringValue(), description);
                }
            }

            return lookup;
        }

        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Gets related description of the given enum mentioned in its description.
        /// </summary>
        /// <typeparam name="TEnum">type of the enum</typeparam>
        /// <param name="thisEnum">the enum</param>
        /// <returns>description of the given enum</returns>
        public static string GetEnumDescription<TEnum>(this TEnum thisEnum) where TEnum : struct
        {
            string description = "";
            FieldInfo fi = typeof(TEnum).GetField(thisEnum.ToString());

            if (fi.IsValid())
            {
                var attr = fi.GetCustomAttribute(typeof(DescriptionAttribute), true);

                if (attr.IsValid())
                {
                    return ((DescriptionAttribute)attr).Description;
                }
            }

            return description;
        }
    }
}
