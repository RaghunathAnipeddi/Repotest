using System;

namespace Chempoint.GP.Infrastructure.Extensions
{
    public static class GuidExtensions
    {
        #region Non-Nullable
        
        /// <summary>
        /// True, when given Guid equals Guid.Empty
        /// </summary>
        /// <param name="guidValue">Guid</param>
        /// <returns>Boolean</returns>
        public static bool IsEmpty(this Guid guidValue)
        {
            return guidValue == Guid.Empty;
        }
        
        /// <summary>
        /// True, when guid doesn't equal Guid.Empty.
        /// Converse of IsEmpty().
        /// </summary>
        /// <param name="guidValue">Guid</param>
        /// <returns>Boolean</returns>
        public static bool IsNotEmpty(this Guid guidValue)
        {
            return !guidValue.IsEmpty();
        }
        
        /// <summary>
        /// True, when given Guid is valid.
        /// </summary>
        /// <param name="guidValue">Guid</param>
        /// <returns>Boolean</returns>
        public static bool IsValid(this Guid guidValue)
        {
            return guidValue.IsNotEmpty();
        }
        
        /// <summary>
        /// True, when given Guid is not valid.
        /// Converse of IsValid().
        /// </summary>
        /// <param name="guidValue">Guid</param>
        /// <returns>Boolean</returns>
        public static bool IsNotValid(this Guid guidValue)
        {
            return !guidValue.IsValid();
        }
        
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   A Guid extension method that query if 'guidValue' is equal to. </summary>
        /// <param name="guidValue">            Guid. </param>
        /// <param name="comparedToGuidValue">  The compared to unique identifier value. </param>
        ///
        /// <returns>   true if equal to, false if not. </returns>
        ///-------------------------------------------------------------------------------------------------
        
        public static bool IsEqualTo(this Guid guidValue, Guid comparedToGuidValue)
        {
            return guidValue.Equals(comparedToGuidValue);
        }
        
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   A Guid extension method that query if 'guidValue' is not equal to. </summary>
        ///<param name="guidValue">            Guid. </param>
        /// <param name="comparedToGuidValue">  The compared to unique identifier value. </param>
        ///
        /// <returns>   true if not equal to, false if not. </returns>
        ///-------------------------------------------------------------------------------------------------
        
        public static bool IsNotEqualTo(this Guid guidValue, Guid comparedToGuidValue)
        {
            return !guidValue.IsEqualTo(comparedToGuidValue);
        }
        
        #endregion Non-Nullable
        
        #region Nullable
        
        /// <summary>
        /// True, when given Guid equals Guid.Empty
        /// </summary>
        /// <param name="guidValue">Guid?</param>
        /// <returns>Boolean</returns>
        public static bool IsEmpty(this Guid? guidValue)
        {
            return !guidValue.HasValue || guidValue.Value.IsEmpty();
        }
        
        /// <summary>
        /// True, when guid doesn't equal Guid.Empty.
        /// Converse of IsEmpty().
        /// </summary>
        /// <param name="guidValue">Guid?</param>
        /// <returns>Boolean</returns>
        public static bool IsNotEmpty(this Guid? guidValue)
        {
            return !guidValue.IsEmpty();
        }
        
        /// <summary>
        /// True, when given Guid is valid.
        /// </summary>
        /// <param name="guidValue">Guid?</param>
        /// <returns>Boolean</returns>
        public static bool IsValid(this Guid? guidValue)
        {
            return guidValue.IsNotEmpty();
        }
        
        /// <summary>
        /// True, when given Guid is not valid.
        /// Converse of IsValid().
        /// </summary>
        /// <param name="guidValue">Guid?</param>
        /// <returns>Boolean</returns>
        public static bool IsNotValid(this Guid? guidValue)
        {
            return !guidValue.IsValid();
        }
        
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   A Guid? extension method that query if 'guidValue' is equal to. </summary>
        /// <param name="guidValue">            Guid? </param>
        /// <param name="comparedToGuidValue">  The compared to unique identifier value. </param>
        ///
        /// <returns>   true if equal to, false if not. </returns>
        ///-------------------------------------------------------------------------------------------------
        
        public static bool IsEqualTo(this Guid? guidValue, Guid? comparedToGuidValue)
        {
            return guidValue.Equals(comparedToGuidValue);
        }
        
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   A Guid? extension method that query if 'guidValue' is not equal to. </summary>
        /// <param name="guidValue">            Guid? </param>
        /// <param name="comparedToGuidValue">  The compared to unique identifier value. </param>
        ///
        /// <returns>   true if not equal to, false if not. </returns>
        ///-------------------------------------------------------------------------------------------------
        
        public static bool IsNotEqualTo(this Guid? guidValue, Guid? comparedToGuidValue)
        {
            return !guidValue.IsEqualTo(comparedToGuidValue);
        }
    
        #endregion Nullable
    }
}
