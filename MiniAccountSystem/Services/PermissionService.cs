using Microsoft.Data.SqlClient;

namespace MiniAccountSystem.Services
{
    public class PermissionService
    {

        private readonly string _connectionString;

        public PermissionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("Connection string is missing!");
        }

        public bool HasAccess(string roleName, string moduleName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM ModuleAccess WHERE RoleName = @Role AND ModuleName = @Module", conn);
                cmd.Parameters.AddWithValue("@Role", roleName);
                cmd.Parameters.AddWithValue("@Module", moduleName);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
    }
}
