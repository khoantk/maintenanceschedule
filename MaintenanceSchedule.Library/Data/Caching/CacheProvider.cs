using System;
using System.Web;
using System.Web.Caching;
using System.Collections.Generic;

namespace MaintenanceSchedule.Library.Data.Caching
{
    public interface ICacheProvider<T>
    {
        T Fetch(string key, Func<T> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry);                
        T Fetch(string key, T data, DateTime? absoluteExpiry, TimeSpan? relativeExpiry);
        IEnumerable<T> Fetch(string key, Func<IEnumerable<T>> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry);
    }

    public interface ICacheProvider<U, T>
    {
        T Fetch(string key, U data, Func<U, T> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry);
    }
    
    public class CacheProvider<U, T> : ICacheProvider<U, T>
    {
        public T Fetch(string key, U data, Func<U, T> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            return FetchAndCache<T>(key, data, retrieveData, absoluteExpiry, relativeExpiry);
        }

        private T FetchAndCache<T>(string key, U data, Func<U, T> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            T value;
            if (!TryGetValue<T>(key, out value))
            {
                value = retrieveData(data);
                if (!absoluteExpiry.HasValue)
                    absoluteExpiry = Cache.NoAbsoluteExpiration;

                if (!relativeExpiry.HasValue)
                    relativeExpiry = Cache.NoSlidingExpiration;

                HttpContext.Current.Cache.Insert(key, value, null, absoluteExpiry.Value, relativeExpiry.Value);
            }
            return value;
        }

        private bool TryGetValue<T>(string key, out T value)
        {
            object cachedValue = HttpContext.Current.Cache.Get(key);
            if (cachedValue == null)
            {
                value = default(T);
                return false;
            }
            else
            {
                try
                {
                    value = (T)cachedValue;
                    return true;
                }
                catch
                {
                    value = default(T);
                    return false;
                }
            }
        }
    }
        
    public class CacheProvider<T> : ICacheProvider<T>
    {
        #region ICacheProvider<T> Members

        public T Fetch(string key, Func<T> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            return FetchAndCache<T>(key, retrieveData, absoluteExpiry, relativeExpiry);
        }
                
        public T Fetch(string key, T data, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            return FetchAndCache<T>(key, data, absoluteExpiry, relativeExpiry);
        }

        public IEnumerable<T> Fetch(string key, Func<IEnumerable<T>> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            return FetchAndCache<IEnumerable<T>>(key, retrieveData, absoluteExpiry, relativeExpiry);
        }

        #endregion

        #region Helper Methods

        private U FetchAndCache<U>(string key, Func<U> retrieveData, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {
            U value;
            if (!TryGetValue<U>(key, out value))
            {
                value = retrieveData();
                if (!absoluteExpiry.HasValue)
                    absoluteExpiry = Cache.NoAbsoluteExpiration;

                if (!relativeExpiry.HasValue)
                    relativeExpiry = Cache.NoSlidingExpiration;

                HttpContext.Current.Cache.Insert(key, value, null, absoluteExpiry.Value, relativeExpiry.Value);
            }
            return value;
        }
                
        private U FetchAndCache<U>(string key, U data, DateTime? absoluteExpiry, TimeSpan? relativeExpiry)
        {            
            if (!TryGetValue<U>(key, out data))
            {                
                if (!absoluteExpiry.HasValue)
                    absoluteExpiry = Cache.NoAbsoluteExpiration;

                if (!relativeExpiry.HasValue)
                    relativeExpiry = Cache.NoSlidingExpiration;

                HttpContext.Current.Cache.Insert(key, data, null, absoluteExpiry.Value, relativeExpiry.Value);
            }
            return data;
        }

        private bool TryGetValue<U>(string key, out U value)
        {
            object cachedValue = HttpContext.Current.Cache.Get(key);
            if (cachedValue == null)
            {
                value = default(U);
                return false;
            }
            else
            {
                try
                {
                    value = (U)cachedValue;
                    return true;
                }
                catch
                {
                    value = default(U);
                    return false;
                }
            }
        }

        #endregion

    }

}
