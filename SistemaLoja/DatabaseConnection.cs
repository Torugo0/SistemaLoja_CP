using Microsoft.Data.SqlClient;

namespace SistemaLoja.Lab12_ConexaoSQLServer;

public class DatabaseConnection
{
    private static readonly string connectionString =
        "Server=localhost,1433;" +
        "Database=SistemaLoja;" +
        "User Id=sa;" +
        "Password=YourStrong!Passw0rd;" +
        "TrustServerCertificate=True;";

    public static SqlConnection GetConnection()
    {
        return new SqlConnection(connectionString);
    }
}