using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonUtilityService.Utilities
{
    public class RedisDatabaseService : IDatabaseService
    {
        public required ConnectionMultiplexer _redisConnection {
            private get;
            init
            {
                if(value == null)
                {
                    throw new ArgumentNullException("Redis connection cannot be null.");
                }
                field = value;
                this.Db = value.GetDatabase();
            }
        }
        public required string _SecureKeyId {
            private get;
            init
            {
                if(string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("SecureKeyId cannot be null or empty.");
                }
                field = value;
            }     
        }
        public IDatabase? Db { get; set; }

        public async Task<T> GetOrSetCache<T>(string key , Func<Task<T>> dbFallback)
        {
            var cached = await this.Db.StringGetAsync(key);
            if(!cached.IsNullOrEmpty)
            {
                // 明確指定使用 string overload
                return JsonSerializer.Deserialize<T>(cached.ToString());
            }
            T data = await dbFallback(); // 執行傳進來的資料庫查詢邏輯
            await Db.StringSetAsync(key , JsonSerializer.Serialize(data) , TimeSpan.FromHours(1));
            return data;
        }
    }
}
