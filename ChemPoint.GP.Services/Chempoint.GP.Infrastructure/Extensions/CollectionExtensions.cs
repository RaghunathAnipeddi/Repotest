using System;
using System.Collections.Generic;
using System.Linq;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class CollectionExtensions
    {
        private const string NewLineConstant = "\n";

        /// <summary>
        /// True, if the collection has entries
        /// </summary>
        /// <typeparam name="T">any collection type</typeparam>
        /// <param name="collection">collection</param>
        /// <returns>True</returns>
        public static bool IsValid<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Count() > 0;
        }

        /// <summary>
        /// False, if the collection is empty or undefined
        /// </summary>
        /// <typeparam name="T">any collection type</typeparam>
        /// <param name="collection">Collection</param>
        /// <returns>False</returns>
        public static bool IsInValid<T>(this IEnumerable<T> collection)
        {
            return !IsValid(collection);
        }

        /// <summary>
        /// Removes the given elements which is already exists in collection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="rangeToRemove"></param>
        public static void RemoveRange<T>(this List<T> collection, IEnumerable<T> rangeToRemove)
        {
            if (rangeToRemove.IsValid())
            {
                collection.RemoveAll(rc => rangeToRemove.Contains(rc));
            }
        }

        /// <summary>
        /// Gives N items from the collection from the position of given item (inclusive of given item).
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="collection">collection to act upon</param>
        /// <param name="item">start item</param>
        /// <param name="nItems">number of items to take from collection</param>
        /// <returns>collection of items requested to take.</returns>
        public static List<T> TakeNItemsFromItem<T>(this List<T> collection, T item, int nItems)
        {
            int givenItemIndex = collection.IndexOf(item);

            if (givenItemIndex >= 0)
            {
                if (givenItemIndex != 0)
                {
                    int skipPosition = givenItemIndex - 1;

                    //NItems from position of given item (including given item).
                    if (nItems + skipPosition > collection.Count)
                    {
                        nItems = collection.Count - givenItemIndex;
                    }
                }

                return collection.Skip(givenItemIndex).Take(nItems).ToList();
            }
            else
            {
                return new List<T>();
            }
        }

        /// <summary>
        /// Gives out the distinct collection based on input keyProperty
        /// </summary>
        /// <typeparam name="T">base type of collection</typeparam>
        /// <typeparam name="Tkey">type of key for finding distincts</typeparam>
        /// <param name="collection">the collection</param>
        /// <param name="predicate">lambda that determines the distinct ones.</param>
        /// <returns>collection of T</returns>
        public static List<T> MakeDistinctOf<T, Tkey>(this ICollection<T> collection, Func<T, Tkey> predicate)
        {
            return collection.GroupBy(predicate).Select(x => x.First()).ToList();
        }

        /// <summary>
        /// Allows to iterate with index of the collection starts with 1
        /// </summary>
        /// <typeparam name="T">base type of collection</typeparam>
        /// <param name="collection">iterating collection</param>
        /// <param name="action">action to happen</param>
        public static void ForeachNonZeroIndexed<T>(this List<T> collection, Action<T, int> action)
        {
            int idx = 0;
            foreach (T item in collection)
                action(item, ++idx);
        }

        /// <summary>
        /// Allows to iterate with index of the collection starts with 0
        /// </summary>
        /// <typeparam name="T">base type of collection</typeparam>
        /// <param name="collection">iterating collection</param>
        /// <param name="action">action to happen</param>
        public static void ForeachZeroIndexed<T>(this List<T> collection, Action<T, int> action)
        {
            int idx = 0;
            foreach (T item in collection)
                action(item, idx++);
        }

        /// <summary>
        /// True, if the given key is not present in the dictionary
        /// </summary>
        /// <typeparam name="TKey">key</typeparam>
        /// <typeparam name="TValue">value</typeparam>
        /// <param name="indexedcollection">the dictionary</param>
        /// <param name="key">key value</param>
        /// <returns>Boolean</returns>
        public static bool ContainsNoKey<TKey, TValue>(this Dictionary<TKey, TValue> indexedcollection, TKey key)
        {
            return !indexedcollection.ContainsKey(key);
        }

        /// <summary>
        /// Merges the two sequences and form a Key-value pair using .NET native Zip method.
        /// </summary>
        /// <typeparam name="TKey">type of first</typeparam>
        /// <typeparam name="TValue">type of second</typeparam>
        /// <param name="firstCollection">first collection</param>
        /// <param name="secondCollection">second collection</param>
        /// <returns>Dictionary Of TKey and TValue</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <Note>Applicable only for Guid and IConvertible types</Note>
        public static Dictionary<TKey, TValue> ZipAndFormKeyValuePair<TKey, TValue>(this IEnumerable<TKey> firstCollection, IEnumerable<TValue> secondCollection)
        {
            if (
                (typeof(TKey) == typeof(Guid) || typeof(TKey) == typeof(IConvertible)) &&
                (typeof(TValue) == typeof(Guid) || typeof(TValue) == typeof(IConvertible))
            )
            {
                return firstCollection.Zip(secondCollection, (source, target) => new { Key = source, Value = target }).ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                throw new InvalidOperationException("CollectionExtensions.ZipAndFormKeyValuePair(): This method received unsupported types.");
            }
        }

        /// <summary>
        /// Gives the previous element for the currently matched element based on filter predicate.
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="coll">the collection</param>
        /// <param name="filterPredicate">Filter lambda</param>
        /// <returns>Previous element if exists, Null otherwise</returns>
        public static T GetPrevious<T>(this List<T> coll, Predicate<T> filterPredicate) where T : class
        {
            int collLength = coll.Count,
            matchIndex = coll.FindIndex(filterPredicate);
            if (matchIndex < 0 || (matchIndex - 1) < 0)
                return default(T);
            else if (matchIndex < collLength)
            {
                return coll[matchIndex - 1];
            }

            return default(T);
        }

        /// <summary>
        /// Gives the next element for the currently matched element based on filter predicate.
        /// </summary>
        /// <typeparam name="T">type of collection</typeparam>
        /// <param name="coll">the collection</param>
        /// <param name="filterPredicate">Filter lambda</param>
        /// <returns>next element if exists, Null otherwise</returns>
        public static T GetNext<T>(this List<T> coll, Predicate<T> filterPredicate) where T : class
        {
            int collLength = coll.Count,
            matchIndex = coll.FindIndex(filterPredicate);
            if (matchIndex > collLength || (matchIndex + 1) > collLength)
                return default(T);
            else if (matchIndex < collLength - 1)
            {
                return coll[matchIndex + 1];
            }

            return default(T);
        }

        /// <summary>
        /// Forms list of list of integers that are continuous (in sequence).
        /// Each entry of outer list gives a sequence
        /// </summary>
        /// <param name="coll">Collection of integers which is non-sequential</param>
        /// <returns>Collection of Collection of sequence of integers</returns>
        public static List<List<int>> FormPatchSequence(this List<int> coll)
        {
            coll.Sort();

            int minNumber = coll.Min(), maxNumber = coll.Max();

            List<List<int>> sequencedPatchList = new List<List<int>>();

            List<int> currentPatch = new List<int>();
            int expectedNextNumber = 0;
            int idx = 0;
            foreach (var item in coll)
            {
                if (idx == 0)
                    currentPatch.Add(item);
                else
                {
                    if (expectedNextNumber == item)
                    {
                        currentPatch.Add(item);
                    }
                    else
                    {
                        sequencedPatchList.Add(currentPatch);

                        currentPatch = new List<int>();
                        currentPatch.Add(item);
                    }
                }

                expectedNextNumber = (item + 1);
                ++idx;
            }
            sequencedPatchList.Add(currentPatch);

            return sequencedPatchList;
        }

        public static List<TValue> GetValuesFor<TKey, TValue>(this Dictionary<TKey, TValue> theLookup, List<TKey> keys)
        {
            List<TValue> valueLst = new List<TValue>();
            foreach (var theKey in keys)
            {
                if (theLookup.ContainsKey(theKey))
                {
                    valueLst.Add(theLookup[theKey]);
                }
            }

            return valueLst;
        }

        /// <summary>
        /// Returns the default value of the type of the given object.
        /// </summary>
        /// <typeparam name="T">type of the input object</typeparam>
        /// <param name="obj">input object</param>
        /// <returns>default value</returns>
        public static T GetDefault<T>(this object obj) where T : class
        {
            return default(T);
        }

        /// <summary>
        /// Returns the string collection entries as s string separated into lines.
        /// </summary>
        /// <param name="stringLst">string collection</param>
        /// <returns>points string</returns>
        public static string ToPoints(this List<string> stringLst)
        {
            string points = string.Empty;

            if (stringLst.IsValid())
            {
                if (stringLst.Count == 1)
                {
                    points = stringLst[0];
                }
                else
                {
                    points = string.Join(NewLineConstant, stringLst);
                }
            }

            return points;
        }
    }
}
