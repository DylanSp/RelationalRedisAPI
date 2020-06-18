using Adapters.Interfaces;
using Data;
using Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Adapters
{
    public class HeroRedisAdapter : IHeroAdapter
    {
        private readonly IDatabase Db;
        private readonly IEntityAdapter<Power> PowerAdapter;

        public HeroRedisAdapter(string connectionString, IEntityAdapter<Power> powerAdapter)
        {
            var redis = ConnectionMultiplexer.Connect(connectionString);
            Db = redis.GetDatabase();

            this.PowerAdapter = powerAdapter;
        }

        // n + 1 queries - can we simplify?
        public IEnumerable<Hero> ReadAll()
        {
            var heroes = new List<Hero>();

            var heroIds = Db.SetMembers("allheroes");
            foreach (var heroId in heroIds)
            {
                var hero = Read(Guid.Parse(heroId));
                if (hero.HasValue)  // should always fire, if allheroes set is consistent
                {
                    heroes.Add(hero.Value);
                }
            }

            return heroes;
        }
        
        public IEnumerable<Hero> SearchHeroes(string name, string location)
        {
            throw new NotImplementedException();
        }

        public Hero? Read(Guid id)
        {
            var isPresent = Db.KeyExists($"hero:{id.ToString()}");
            if (!isPresent)
            {
                return null;
            }

            var heroDetails = Db.HashGet($"hero:{id.ToString()}", new RedisValue[] { "name", "location" });
            var name = heroDetails[0];
            var location = heroDetails[1];

            var powers = new List<Power>();

            var powerIds = Db.SetMembers($"heropowers:{id.ToString()}");
            foreach (var powerId in powerIds)
            {
                var power = PowerAdapter.Read(Guid.Parse(powerId));
                if (power.HasValue)     // should always fire, if heropowers integrity is maintained
                {
                    powers.Add(power.Value);
                }
            }

            return new Hero(id, name, location, powers);
        }

        public void Save(Hero newHeroData)
        {
            var heroIdKey = newHeroData.Id.ToString();

            var isPresent = Db.KeyExists($"hero:{heroIdKey}");
            if (!isPresent)     // if inserting, update "allheroes" set as well 
            {
                Db.SetAdd("allheroes", heroIdKey);
            }

            Db.HashSet($"hero:{heroIdKey}", new HashEntry[] { new HashEntry("name", newHeroData.Name) });
            Db.HashSet($"hero:{heroIdKey}", new HashEntry[] { new HashEntry("location", newHeroData.Location) });

            // more elegant way to do this then deleting heropowers and recreating?
            Db.KeyDelete($"heropowers:{heroIdKey}");

            if (newHeroData.Powers.Count() > 0)     // if no powers are present, Redis throws error "wrong number of parameters for SADD"
            {
                Db.SetAdd($"heropowers:{heroIdKey}", newHeroData.Powers.Select(pow => (RedisValue)pow.Id.ToString()).ToArray());
            }

            // n queries to update powers
            foreach (var power in newHeroData.Powers)
            {
                PowerAdapter.Save(power);
            }
        }

        public void Delete(Guid id)
        {
            Db.KeyDelete($"hero:{id.ToString()}");
            Db.KeyDelete($"heropowers:{id.ToString()}");
        }
    }
}
