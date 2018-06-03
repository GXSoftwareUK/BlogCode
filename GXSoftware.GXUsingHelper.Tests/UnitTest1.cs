using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using GXSoftwareUK.UsingHelper.Console;
using GXSoftwareUK.UsingHelper.Console.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GXSoftware.GXUsingHelper.Tests
{
    [TestClass]
    public class ContextHelperTests
    {
        #region Inserts
        private const string ConnectionString = "data source=LAPPYLAPTOP\\SQL02;initial catalog=Contacts;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework";
        private const string DropTestSetupRows = "DELETE dbo.MyTeam WHERE Id < 0";
        private const string DropTestSetupProcedure = "IF EXISTS(SELECT 1 FROM sys.procedures WHERE Name = 'TestSetup') DROP PROCEDURE TestSetup";
        private const string CreateTestStoredProc = "CREATE PROCEDURE dbo.TestSetup AS BEGIN " +
                                                    "SET IDENTITY_INSERT Contacts.dbo.MyTeam ON;" +
                                                    "DELETE MyTeam WHERE Id< 0;" +
                                                    "DECLARE @start int = -1, @end int = -1000, @id varchar(6), @lastLogin DATETIME2(7);" +
                                                    "WHILE @start > @end " +
                                                    "BEGIN " +
                                                    "SET @Id = CAST((@start * -1) AS VARCHAR); " +
                                                    "SET @lastLogin = DATEADD(d, @start, GETDATE()); " +
                                                    "INSERT INTO MyTeam(Id, NickName,LastLogin, FirstName, LastName) VALUES(@start,'Tester_' + @Id , @lastLogin ,'Test','User_' + @Id); " +
                                                    "SET @start = @start - 1; " +
                                                    "END; " +
                                                    "SET IDENTITY_INSERT Contacts.dbo.MyTeam ON; " +
                                                    "SELECT Id, NickName, FirstName, LastName, LastLogin FROM MyTeam; " + 
                                                    "END ";

        private const string LoadTeamsSetup = "SELECT Id, NickName, FirstName, LastName, LastLogin FROM MyTeam;";
        #endregion

        public readonly List<MyTeam> _teamsSetup = new List<MyTeam>( );

        public readonly Mock<ContextHelper_NoTest> _mockClassUnderTest_1 = new Mock<ContextHelper_NoTest>() {CallBase = false};
        public readonly Mock<ContextHelper_Test> _mockClassUnderTest_2 = new Mock<ContextHelper_Test>() { CallBase = true };
        public readonly Mock<List<MyTeam>> _mockTeams = new Mock<List<MyTeam>>();

        public readonly Mock<MyTeam> _mockTeam = new Mock<MyTeam>();

        [TestInitialize]
        public void Setup()
        {

            _mockTeam.Setup(t => t.Id).Returns(1);
            _mockTeam.Setup(t => t.Nickname).Returns("mockNickname");
            _mockTeam.Setup(t => t.FirstName).Returns("mockFirstname");
            _mockTeam.Setup(t => t.LastName).Returns("mockLastname");

            _mockTeams.Object.Add(_mockTeam.Object);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var sql = "UPDATE";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandText = DropTestSetupRows;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = DropTestSetupProcedure;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = CreateTestStoredProc;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "TestSetup";
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _teamsSetup.Add(new MyTeam
                            {
                                Id = reader.GetInt32(0),
                                Nickname = reader.GetString(1),
                                FirstName = reader.GetString(2),
                                LastName = reader.GetString(3),
                                LastLogin = reader.GetDateTime(4)
                            });
                        }
                    }

                }
            }
        }

        [TestCleanup]
        public void Teardown()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                var sql = "UPDATE";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    cmd.CommandText = DropTestSetupRows;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = DropTestSetupProcedure;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        [TestMethod]
        public void TestIfSetup()
        {
            //Arrange

            //Act

            //Assert
            Assert.AreEqual(_teamsSetup.Count, 999);
        }

        [TestClass]
        public class WhenNotUsingHelper: ContextHelperTests
        {
            [TestMethod]
            public void TestIfResultFromDbOrMock()
            {
                //arrange
                var expected = "Tester_999"; //As we cannot mock out this connection
                var classUnderTest = new ContextHelper_NoTest();
                //act
                var result = classUnderTest.GetTeamsDefaultConnection(expected);
                //asset
                Assert.IsNotNull(result);
                Assert.AreEqual(expected, result.Nickname);
            }

            [TestMethod]
            public void TestIfResultFromDbOrMockShouldFail()
            {
                //arrange
                var expected = "Tester_999"; //As we cannot mock out this connection
                //act
                var result = _mockClassUnderTest_1.Object.GetTeamsDefaultConnection(expected);
                //asset
                //This will fail as how do you mock out the DbContext when in a using statement.
                //Option 1.  Change teh way you use using statements (to never)  
                //Option 2.  Initialize object outside using statement (see next test
                Assert.IsNull(result);
            }

            [TestMethod]
            public void TestIfResultFromDbOrMockWithConnectionString()
            {
                //arrange
                var expected = "mockNickname"; //As we cannot mock out this connection
                var mockDbContext = new Mock<MyDbContext>(ConnectionString) {CallBase = false};
              //  mockDbContext.Setup(t => t.MyTeams).Returns(_mockTeam.Object);
                _mockClassUnderTest_1.Setup(t => t.Db).Returns(mockDbContext.Object);               
                //act
                var result = _mockClassUnderTest_1.Object.GetTeamsNewConnection(expected, ConnectionString);
                //asset
                Assert.IsNotNull(result);
                Assert.AreEqual(expected, result.Nickname);
                Assert.IsNull(_mockClassUnderTest_1.Object.Db);
            }
        }

        [TestClass]
        public class WhenUsingHelper: ContextHelperTests
        {
            private readonly Mock<MyDbContext> _mockDbContext =  new Mock<MyDbContext>();
            [TestMethod]
            public void TestIfResultFromDbOrMock()
            {
                //arrange
                var expected = "mockNickname";

                DbSet<MyTeam> mockDbSet = GetQueryableMockItems(_mockTeams.Object);

                _mockDbContext.Setup(t => t.MyTeams).Returns(mockDbSet);

                _mockClassUnderTest_2.Setup(t => t.PassedObject).Returns(_mockDbContext.Object);
                _mockClassUnderTest_2.Setup(t => t.Using<Mock<MyDbContext>>(new object[] {})).Returns(_mockDbContext);

                //act
                var result = _mockClassUnderTest_2.Object.GetTeamsDefaultConnection(expected);
                //asset
                Assert.IsNotNull(result);
                Assert.AreEqual(expected, result.Nickname);
            }

            private static DbSet<T> GetQueryableMockItems<T>(List<T> listItems) where T : class
            {
                var queryable = listItems.AsQueryable();

                var mockDbSet = new Mock<DbSet<T>>();
                mockDbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
                mockDbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
                mockDbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
                mockDbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);

                return mockDbSet.Object;
            }
        }

        public class MockDbContext
        {
            public virtual List<MyTeam> Teams { get; set; }
        }

    }



}
