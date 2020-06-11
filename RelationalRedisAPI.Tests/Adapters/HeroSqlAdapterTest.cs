using Adapters;
using Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RelationalRedisAPI.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RelationalRedisAPI.Tests.Adapters
{
    [TestClass]
    public class HeroSqlAdapterTest
    {
        private const string ConnectionString = "Data Source=localhost;Initial Catalog=Superheroes;Persist Security Info=True;User ID=sa;Password=adminpass";
        private HeroSqlAdapter Adapter;

        [TestInitialize]
        public void SetUp()
        {
            var powerAdapter = new PowerSqlAdapter(ConnectionString);
            Adapter = new HeroSqlAdapter(ConnectionString, powerAdapter);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ReadAll_WithHeroesInserted_ReturnsAllHeroes()
        {
            // Arrange
            var savedHeroes = new List<Hero>()
            {
                new Hero(Guid.NewGuid(), "Captain America", "New York City", new List<Power>()),
                new Hero(Guid.NewGuid(), "Iron Man", "Los Angeles", new List<Power>())
            };
            foreach (var hero in savedHeroes)
            {
                Adapter.Save(hero);
            }

            // Act
            var allHeroes = Adapter.ReadAll();

            // Assert
            foreach (var hero in savedHeroes)
            {
                Assert.IsTrue(allHeroes.Contains(hero));
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingHero_ReturnsHero()
        {
            // Arrange
            var savedHero = new Hero(Guid.NewGuid(), "Thor", "Asgard", new List<Power>());
            Adapter.Save(savedHero);

            // Act
            var returnedHero = Adapter.Read(savedHero.Id);

            // Assert
            Assert.IsTrue(returnedHero.HasValue);
            Assert.AreEqual(savedHero, returnedHero.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingHeroWithPowers_ReturnsHeroWithPowers()
        {
            // Arrange
            var powers = new List<Power>
            {
                new Power(Guid.NewGuid(), "Flight")
            };
            var savedHero = new Hero(Guid.NewGuid(), "Thor", "Asgard", powers);
            Adapter.Save(savedHero);

            // Act
            var returnedHero = Adapter.Read(savedHero.Id);

            // Assert
            Assert.IsTrue(returnedHero.HasValue);
            Assert.AreEqual(savedHero, returnedHero.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_NonexistentHero_ReturnsNull()
        {
            // Arrange - not needed

            // Act
            var result = Adapter.Read(Guid.NewGuid());

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_NewHero_InsertsOneRecord()
        {
            // Arrange
            var numPreExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);
            var newHero = new Hero(Guid.NewGuid(), "Hawkeye", "???", new List<Power>());

            // Act
            Adapter.Save(newHero);

            // Assert
            var numPostExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);
            Assert.AreEqual(numPreExistingHeroes + 1, numPostExistingHeroes);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingHero_PreservesNumberOfRecords()
        {
            // Arrange

            // make sure at least one hero exists, so we have something to update
            var oldHero = new Hero(Guid.NewGuid(), "Superman", "Metropolis", new List<Power>());
            Adapter.Save(oldHero);
            var numPreExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);

            // Act
            var newHero = new Hero(oldHero.Id, "Superman Red Son", "Moscow", new List<Power>());
            Adapter.Save(newHero);

            // Assert
            var numPostExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);
            Assert.AreEqual(numPreExistingHeroes, numPostExistingHeroes);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingHero_UpdatesHeroDetails()
        {
            // Arrange

            // make sure at least one hero exists, so we have something to update
            var oldHero = new Hero(Guid.NewGuid(), "The Hulk", "Brazil", new List<Power>());
            Adapter.Save(oldHero);

            // Act
            var newHero = new Hero(oldHero.Id, "The Hulk", "The Nine Realms", new List<Power>());
            Adapter.Save(newHero);

            // Assert
            var readHero = Adapter.Read(newHero.Id);
            Assert.IsTrue(readHero.HasValue);
            Assert.AreEqual(newHero, readHero.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingHeroWithNewPowers_UpdatesHeroPowers()
        {
            // Arrange

            // make sure at least one hero exists, so we have something to update
            var oldPowers = new List<Power>
            {
                new Power(Guid.NewGuid(), "Mjolnir")
            };
            var oldHero = new Hero(Guid.NewGuid(), "Thor", "Asgard", oldPowers);
            Adapter.Save(oldHero);

            // Act
            var newPowers = new List<Power>
            {
                new Power(Guid.NewGuid(), "Stormbreaker")
            };
            var newHero = new Hero(oldHero.Id, "Thor", "Asgard", newPowers);
            Adapter.Save(newHero);

            // Assert
            var readHero = Adapter.Read(newHero.Id);
            Assert.IsTrue(readHero.HasValue);
            Assert.AreEqual(newHero, readHero.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_ExistingHero_RemovesOneRecord()
        {
            // Arrange

            // make sure at least one hero exists, so we can delete it
            var hero = new Hero(Guid.NewGuid(), "Batman", "Gotham", new List<Power>());
            Adapter.Save(hero);
            var numPreExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);

            // Act
            Adapter.Delete(hero.Id);

            // Assert
            var numPostExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);
            Assert.AreEqual(numPreExistingHeroes - 1, numPostExistingHeroes);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_NonexistentHero_PreservesNumberOfRecords()
        {
            // Arrange
            var numPreExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);

            // Act
            Adapter.Delete(Guid.NewGuid());

            // Assert
            var numPostExistingHeroes = AdapterTestHelpers.CountSqlHeroes(ConnectionString);
            Assert.AreEqual(numPreExistingHeroes, numPostExistingHeroes);
        }
    }
}
