using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FB.CacheLib.Tests
{
    public class CacheValidatorTests
    {

        [Fact]
        public void When_StringItemAdded_TryGetSucceeds()
        {
            string valueToTest = "one";
            FBGenericCache<int, string> stringCache = new FBGenericCache<int, string>(2, 2);
            bool result = stringCache.AddOrUpdate(1, valueToTest);

            string cacheValue = null;
            stringCache.TryGet(1, out cacheValue);

            Assert.True(cacheValue == valueToTest);
        }


        [Fact]
        public void When_StringCacheCreation_Succeed_Then_CheckCacheSize()
        {
            FBGenericCache<int, string> stringCache = new FBGenericCache<int, string>(2, 2);
            bool result = stringCache.AddOrUpdate(1, "one");
            result = result && stringCache.AddOrUpdate(2, "two");

            Assert.True(stringCache.CacheCount == 2);
        }

        [Fact]
        public void When_Capacity_Full_Then_EvictCacheToAdd()
        {
            FBGenericCache<int, string> stringCache = new FBGenericCache<int, string>(2, 2);
            bool result = stringCache.AddOrUpdate(1, "one");
            result = result && stringCache.AddOrUpdate(2, "two");

            Assert.True(result);
        }


        [Fact]
        public void When_CacheCountExceeded_UpdateItem_And_VerifyValueIntact()
        {
            int valueToTest = 3;
            FBGenericCache<int, int> intCache = new FBGenericCache<int, int>(2, 2);

            intCache.AddOrUpdate(100, 1);
            intCache.AddOrUpdate(101, 2);
            intCache.AddOrUpdate(101, valueToTest);
            int cacheValue;
            intCache.TryGet(101, out cacheValue);

            Assert.True(cacheValue == valueToTest);
        }


        [Fact]
        public void When_CacheCountExceeded_RemoveLeastRecentlyUsedItem_And_GetNotified()
        {
            int valueToTest = 1;

            FBGenericCache<int, int> intCache = new FBGenericCache<int, int>(2, 2);
            intCache.CacheEvictedEvent += (s, e) =>
            {
                Assert.True(e.ToString() == "100");
            };

            intCache.AddOrUpdate(100, valueToTest);
            intCache.AddOrUpdate(101, 2);
            intCache.AddOrUpdate(102, 3);

            int cacheValue;
            intCache.TryGet(100, out cacheValue);

            Assert.True(cacheValue != valueToTest && cacheValue == 0);

            Assert.True(intCache.CacheCount == 2);
        }


        [Fact]
        public void When_AccessCache_InParalell_EnsureConsistency()
        {
            FBGenericCache<int, int> intCache = new FBGenericCache<int, int>(2, 2);

            var parallelRequests = new int[10];

            Parallel.ForEach(parallelRequests, item =>
            {
                int valueToTest;
                intCache.AddOrUpdate(100, 1);
                intCache.AddOrUpdate(101, 2);
                intCache.AddOrUpdate(101, 3);
                intCache.TryGet(101, out valueToTest);
                Assert.True(valueToTest == 3);
            });
        }
    }

}
