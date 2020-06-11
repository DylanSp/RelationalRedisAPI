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
    public class TeamRedisAdapterTest
    {
        private const string ConnectionString = "localhost";
        private TeamRedisAdapter Adapter;

        [TestInitialize]
        public void SetUp()
        {
            var powerAdapter = new PowerRedisAdapter(ConnectionString);
            var heroAdapter = new HeroRedisAdapter(ConnectionString, powerAdapter);
            Adapter = new TeamRedisAdapter(ConnectionString, heroAdapter);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void ReadAll_WithTeamsInserted_ReturnsAllTeams()
        {
            // Arrange
            var savedTeams = new List<Team>()
            {
                new Team(Guid.NewGuid(), "Avengers", new List<Hero>()),
                new Team(Guid.NewGuid(), "X-Men", new List<Hero>())
            };
            foreach (var team in savedTeams)
            {
                Adapter.Save(team);
            }

            // Act
            var allTeams = Adapter.ReadAll();

            // Assert
            foreach (var team in savedTeams)
            {
                Assert.IsTrue(allTeams.Contains(team));
            }
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingTeam_ReturnsTeam()
        {
            // Arrange
            var savedTeam = new Team(Guid.NewGuid(), "Avengers", new List<Hero>());
            Adapter.Save(savedTeam);

            // Act
            var returnedTeam = Adapter.Read(savedTeam.Id);

            // Assert
            Assert.IsTrue(returnedTeam.HasValue);
            Assert.AreEqual(savedTeam, returnedTeam.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingTeamWithMembers_ReturnsTeamWithMembers()
        {
            // Arrange
            var teamMembers = new List<Hero>
            {
                new Hero(Guid.NewGuid(), "Captain America", "New York City", new List<Power>())
            };
            var savedTeam = new Team(Guid.NewGuid(), "Avengers", teamMembers);
            Adapter.Save(savedTeam);

            // Act
            var returnedTeam = Adapter.Read(savedTeam.Id);

            // Assert
            Assert.IsTrue(returnedTeam.HasValue);
            Assert.AreEqual(savedTeam, returnedTeam.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_ExistingTeamWithMembersWithPowers_ReturnsTeamWithMembersWithPowers()
        {
            // Arrange
            var powers = new List<Power>
            {
                new Power(Guid.NewGuid(), "Magic")
            };
            var teamMembers = new List<Hero>
            {
                new Hero(Guid.NewGuid(), "Dr. Strange", "New York City", powers)
            };
            var savedTeam = new Team(Guid.NewGuid(), "Avengers", teamMembers);
            Adapter.Save(savedTeam);

            // Act
            var returnedTeam = Adapter.Read(savedTeam.Id);

            // Assert
            Assert.IsTrue(returnedTeam.HasValue);
            Assert.AreEqual(savedTeam, returnedTeam.Value);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Read_NonexistentTeam_ReturnsNull()
        {
            // Arrange - not needed

            // Act
            var result = Adapter.Read(Guid.NewGuid());

            // Assert
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_NewTeam_InsertsOneRecord()
        {
            // Arrange
            var numPreExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);
            var newTeam = new Team(Guid.NewGuid(), "Guardians of the Galaxy", new List<Hero>());

            // Act
            Adapter.Save(newTeam);

            // Assert
            var numPostExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);
            Assert.AreEqual(numPreExistingTeams + 1, numPostExistingTeams);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingTeam_PreservesNumberOfRecords()
        {
            // Arrange

            // make sure at least one power exists, so we have something to update
            var oldTeam = new Team(Guid.NewGuid(), "Justice League", new List<Hero>());
            Adapter.Save(oldTeam);
            var numPreExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);

            // Act
            var newTeam = new Team(oldTeam.Id, "SUPER JUSTICE LEAGUE!!!!11", new List<Hero>());
            Adapter.Save(newTeam);

            // Assert
            var numPostExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);
            Assert.AreEqual(numPreExistingTeams, numPostExistingTeams);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Save_ExistingTeamWithNewName_UpdatesTeamName()
        {
            // Arrange

            // make sure at least one power exists, so we have something to update
            var oldTeam = new Team(Guid.NewGuid(), "X-Men", new List<Hero>());
            Adapter.Save(oldTeam);

            // Act
            var newTeam = new Team(oldTeam.Id, "X-Force", new List<Hero>());
            Adapter.Save(newTeam);

            // Assert
            var readTeam = Adapter.Read(newTeam.Id);
            Assert.IsTrue(readTeam.HasValue);
            Assert.AreEqual(newTeam, readTeam.Value);
        }

        // TODO - updating team members

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_ExistingTeam_RemovesOneRecord()
        {
            // Arrange

            // make sure at least one team exists, so we can delete it
            var team = new Team(Guid.NewGuid(), "Bat-Family", new List<Hero>());
            Adapter.Save(team);
            var numPreExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);

            // Act
            Adapter.Delete(team.Id);

            // Assert
            var numPostExistingPowers = AdapterTestHelpers.CountRedisTeams(ConnectionString);
            Assert.AreEqual(numPreExistingTeams - 1, numPostExistingPowers);
        }

        [TestMethod]
        [TestCategory("IntegrationTest")]
        public void Delete_NonexistentTeam_PreservesNumberOfRecords()
        {
            // Arrange
            var numPreExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);

            // Act
            Adapter.Delete(Guid.NewGuid());

            // Assert
            var numPostExistingTeams = AdapterTestHelpers.CountRedisTeams(ConnectionString);
            Assert.AreEqual(numPreExistingTeams, numPostExistingTeams);
        }
    }
}
