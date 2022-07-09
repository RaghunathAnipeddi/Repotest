using Microsoft.Dexterity;
using Microsoft.Dexterity.Applications;
using System.Data.SqlClient;

namespace DataAccess
{
    /// <summary>
    /// Class to set the GP Database Connection.
    /// </summary>
    public class GPC
    {
        private System.Data.SqlClient.SqlConnection cn = new SqlConnection();
        private Microsoft.Dexterity.GPConnection gPConnObj;
        private bool success = false;
        
        /// <summary>
        /// This class creates and sql connection instance as that of GP and retrieves the values
        /// </summary>
        public GPC(string user, string pwd)
        {
            // Call Startup
            GPConnection.Startup();

            // Create the connection object
            gPConnObj = new GPConnection();

            // Initialize
            // DO NOT use the sample registration key -- use your own key in
            // any applications you ship.
            gPConnObj.Init(Chempoint.GP.SO.Properties.Resources.GP_LICENCE_COMPANY, Chempoint.GP.SO.Properties.Resources.GP_LICENCE_KEY);

            // Make the connection
            cn.ConnectionString = Chempoint.GP.SO.Properties.Resources.GP_LICENCE_DBNAME_STR + Dynamics.Globals.IntercompanyId.Value;
            gPConnObj.Connect(cn,
                Dynamics.Globals.SqlDataSourceName.Value,
                user,
                pwd);

            if ((gPConnObj.ReturnCode & (int)GPConnection.ReturnCodeFlags.SuccessfulLogin) ==
                (int)GPConnection.ReturnCodeFlags.SuccessfulLogin)
            {
                success = true;
            }
        }

        /// <summary>
        /// Constructor with 3 Parameters and this will provide the user to include the DB Name Explicitly
        /// </summary>
        public GPC(string user, string pwd, string dbname)
        {
            // Call Startup
            GPConnection.Startup();

            // Create the connection object
            gPConnObj = new GPConnection();

            // Initialize
            // DO NOT use the sample registration key -- use your own key in
            // any applications you ship.
            gPConnObj.Init("Fabricam", "<i52C5VV289487ALZY|11^276'275q712=85k~");

            // Make the connection
            cn.ConnectionString = "DATABASE=" + dbname;
            gPConnObj.Connect(cn,
                Dynamics.Globals.SqlDataSourceName.Value,
                user,
                pwd);

            if ((gPConnObj.ReturnCode & (int)GPConnection.ReturnCodeFlags.SuccessfulLogin) ==
                (int)GPConnection.ReturnCodeFlags.SuccessfulLogin)
            {
                success = true;
            }
        }

        /// <summary>
        /// This class creates and sql connection instance as that of GP and retrieves the values
        /// </summary>
        public GPC()
        {
            // Call Startup
            GPConnection.Startup();

            // Create the connection object
            gPConnObj = new GPConnection();

            // Initialize
            // DO NOT use the sample registration key -- use your own key in
            // any applications you ship.
            gPConnObj.Init("Fabricam", "<i52C5VV289487ALZY|11^276'275q712=85k~");

            // Make the connection
            cn.ConnectionString = "DATABASE=" + Microsoft.Dexterity.Applications.Dynamics.Globals.IntercompanyId.Value;
            gPConnObj.Connect(cn,
                Microsoft.Dexterity.Applications.Dynamics.Globals.SqlDataSourceName.Value,
                Microsoft.Dexterity.Applications.Dynamics.Globals.UserId.Value,
                Microsoft.Dexterity.Applications.Dynamics.Globals.SqlPassword.Value);

            if ((gPConnObj.ReturnCode & (int)GPConnection.ReturnCodeFlags.SuccessfulLogin) ==
                (int)GPConnection.ReturnCodeFlags.SuccessfulLogin)
            {
                success = true;
            }
        }

        ~GPC()
        {
            cn = null;
            // Dispose of the connection object
            gPConnObj = null;

            // Call Shutdown
            GPConnection.Shutdown();
        }

        public SqlConnection Connection
        {
            get
            {
                return cn;
            }
        }

        public bool LoginSuccess
        {
            get
            {
                return success;
            }
        }
    }
}