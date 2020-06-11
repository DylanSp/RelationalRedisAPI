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
    public class PowerSqlAdapterTest
    {
        private const string ConnectionString = "Data Source=localhost;Initial Catalog=Superheroes;Persist Security Info=True;User ID=sa;Password=adminpass";
        private PowerSqlAdapter Adapter;

        [TestInitialize]
        public void SetUp()
        {
            Adapter = new PowerSqlAdapter(ConnectionString);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ReadAll_WithPowersInserted_ReturnsAllPowers()
        {
            // Arrange
            var savedPowers = new List<Power>()
            {
                new Power(Guid.NewGuid(), "Flight"),
                new Power(Guid.NewGuid(), "Super Strength")
            };
            foreach (var power in savedPowers)
            {
                Adapter.Save(power);
            }

            // Act
            var allPowers = Adapter.ReadAll();

            // Assert
            foreach (var power in savedPowers)
            {
                Assert.IsTrue(allPowers.Contains(power));
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingPower_ReturnsPower()
        {
            // Arrange
            var savedPower = new Power(Guid.NewGuid(), "Magic");
            Adapter.Save(savedPower);

            // Act
            var returnedPower = Adapter.Read(savedPower.Id);

            // Assert
            Assert.IsTrue(returnedPower.HasValue);
            Assert.AreEqual(savedPower, returnedPower.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_NonexistentPower_ReturnsNull()
        {
            // Arrange - not needed

            // Act
            var result = Adapter.Read(Guid.NewGuid());

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_NewPower_InsertsOneRecord()
        {
            // Arrange
            var numPreExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);
            var newPower = new Power(Guid.NewGuid(), "Super Archery");

            // Act
            Adapter.Save(newPower);

            // Assert
            var numPostExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);
            Assert.AreEqual(numPreExistingPowers + 1, numPostExistingPowers);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingPower_PreservesNumberOfRecords()
        {
            // Arrange

            // make sure at least one power exists, so we have something to update
            var oldPower = new Power(Guid.NewGuid(), "Flight");
            Adapter.Save(oldPower);
            var numPreExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);

            // Act
            var newPower = new Power(oldPower.Id, "SUUUUPER FLIGHT");
            Adapter.Save(newPower);

            // Assert
            var numPostExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);
            Assert.AreEqual(numPreExistingPowers, numPostExistingPowers);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingPower_UpdatesPowerDetails()
        {
            // Arrange

            // make sure at least one power exists, so we have something to update
            var oldPower = new Power(Guid.NewGuid(), "Spy Skills");
            Adapter.Save(oldPower);

            // Act
            var newPower = new Power(oldPower.Id, "SUUUUPER SPY SKILLS");
            Adapter.Save(newPower);

            // Assert
            var readPower = Adapter.Read(newPower.Id);
            Assert.IsTrue(readPower.HasValue);
            Assert.AreEqual(newPower, readPower.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_ExistingPower_RemovesOneRecord()
        {
            // Arrange

            // make sure at least one power exists, so we can delete it
            var power = new Power(Guid.NewGuid(), "Some ridiculous Silver Age stuff");
            Adapter.Save(power);
            var numPreExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);

            // Act
            Adapter.Delete(power.Id);

            // Assert
            var numPostExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);
            Assert.AreEqual(numPreExistingPowers - 1, numPostExistingPowers);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_NonexistentPower_PreservesNumberOfRecords()
        {
            // Arrange
            var numPreExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);

            // Act
            Adapter.Delete(Guid.NewGuid());

            // Assert
            var numPostExistingPowers = AdapterTestHelpers.CountSqlPowers(ConnectionString);
            Assert.AreEqual(numPreExistingPowers, numPostExistingPowers);
        }
    }
}
