﻿using Adapters.Interfaces;
using Data;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Adapters
{
    public class PowerRedisAdapter : IPowerAdapter
    {
        private readonly IDatabase Db;

        public PowerRedisAdapter(string connectionString)
        {
            var redis = ConnectionMultiplexer.Connect(connectionString);
            Db = redis.GetDatabase();
        }

        // takes n + 1 queries - is there a way to do it in less?
        public IEnumerable<Power> ReadAll()
        {
            var powers = new List<Power>();

            var powerIds = Db.SetMembers("allpowers");
            foreach(var stringId in powerIds)
            {
                var guidId = Guid.Parse(stringId);
                var power = Read(guidId);
                if (power.HasValue) // should always fire, assuming integrity of allpowers set is maintained
                {
                    powers.Add(power.Value);
                }
            }

            return powers;
        }

        public IEnumerable<Power> SearchPowers(string name)
        {
            throw new NotImplementedException();
        }

        public Power? Read(Guid id)
        {
            var isPresent = Db.KeyExists($"power:{id.ToString()}");
            if (!isPresent)
            {
                return null;
            }

            var name = Db.HashGet($"power:{id.ToString()}", "name");

            return new Power(id, name);
        }

        public void Save(Power newPowerData)
        {
            var powerIdKey = newPowerData.Id.ToString();

            var isPresent = Db.KeyExists($"power:{powerIdKey}");
            if (!isPresent)     // if inserting, update "allpowers" set as well 
            {
                Db.SetAdd("allpowers", powerIdKey);
            }

            Db.HashSet($"power:{powerIdKey}", new HashEntry[] { new HashEntry("name", newPowerData.Name) });
        }

        public void Delete(Guid id)
        {
            Db.KeyDelete($"power:{id.ToString()}");
            Db.SetRemove("allpowers", id.ToString());
        }
    }
}
