using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GXSoftwareUK.GxUsingHelper;
using GXSoftwareUK.UsingHelper.Console.Context;

namespace GXSoftwareUK.UsingHelper.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.ReadLine();
        }
    }


    public class ContextHelper_NoTest
    {
        public MyTeam GetTeamsDefaultConnection(string nickName)
        {
            try
            {
                using (var db = new MyDbContext())
                {
                    var model = db.MyTeams.FirstOrDefault(t => t.Nickname == nickName);
                    return model;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }

        public virtual MyDbContext Db { get; set; }

        public MyTeam GetTeamsNewConnection(string nickName, string connectionString)
        {
            try
            {
                Db = new MyDbContext(connectionString);
                using (Db)
                {
                    var model = Db.MyTeams.FirstOrDefault(t => t.Nickname == nickName);
                    return model;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
        }


    }

    public class ContextHelper_Test : AGxUsingHelper
    {
        public MyTeam GetTeamsDefaultConnection(string nickName)
        {
            var db = Using<MyDbContext>();
            try
            {
                using (db)
                {
                    var model = db.MyTeams.FirstOrDefault(t => t.Nickname == nickName);
                    return model;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                Close(ref db);
            }
        }

        public MyTeam GetTeamsNewConnection(string nickName, string connectionString)
        {
            var db = Using<MyDbContext>(new object[] { connectionString });
            try
            {
                using (db)
                {
                    var model = db.MyTeams.FirstOrDefault(t => t.Nickname == nickName);
                    return model;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                Close(ref db);
            }
        }

    }
}
