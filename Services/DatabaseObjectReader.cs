using KofCWSC.DBObjectAnalyzer.Models;
using Microsoft.Data.SqlClient;

namespace KofCWSC.DBObjectAnalyzer.Services;

public class DatabaseObjectReader
{
    private readonly string _connectionString;

    public DatabaseObjectReader(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        _connectionString = connectionString;
    }

    public async Task<List<DatabaseObject>> ReadObjectsAsync(
        CancellationToken cancellationToken = default)
    {
        var objects = new List<DatabaseObject>();

        const string sql = """
            SELECT
                o.object_id,
                s.name AS SchemaName,
                o.name,
                o.type,
                OBJECT_DEFINITION(o.object_id) AS Definition
            FROM sys.objects o
            INNER JOIN sys.schemas s
                ON o.schema_id = s.schema_id
            WHERE o.type IN ('P','FN','IF','TF','V','TR')
            ORDER BY
                s.name,
                o.name;
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand(sql, connection);

            command.CommandTimeout = 30;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var sqlType = reader.GetString(3);

                var databaseObject = new DatabaseObject
                {
                    ObjectId = reader.GetInt32(0),

                    Schema = reader.GetString(1),

                    Name = reader.GetString(2),

                    SqlType = sqlType,

                    ObjectType = sqlType.ToDatabaseObjectType(),

                    Definition = reader.IsDBNull(4)
                        ? string.Empty
                        : reader.GetString(4)
                };

                objects.Add(databaseObject);
            }
        }
        catch (SqlException ex)
        {
            throw new InvalidOperationException(
                "Unable to read database objects from SQL Server.",
                ex);
        }

        return objects;
    }
}