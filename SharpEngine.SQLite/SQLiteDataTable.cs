using System.Data.SQLite;
using SharpEngine.Core.Data.DataTable;

namespace SharpEngine.SQLite;

/// <summary>
/// Sqlite Data Table
/// </summary>
public class SQLiteDataTable<T> : IDataTable<T>
    where T : class, new()
{
    private List<T> Objects { get; }

    private string DbFile { get; set; }
    private string Version { get; set; }

    /// <summary>
    /// Create Data Table from SQLite
    /// </summary>
    /// <param name="dbFile">SQLite File</param>
    /// <param name="version">Version</param>
    /// <exception cref="NotImplementedException">If use not implement type</exception>
    public SQLiteDataTable(string dbFile, string version = "3")
    {
        DbFile = dbFile;
        Version = version;
        Objects = [];

        var connection = new SQLiteConnection(
            $"Data Source={dbFile};Version={version};New=True;Compress=True;"
        );

        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = $"SELECT * FROM {typeof(T).Name};";
        var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var obj = new T();

            var index = 0;
            foreach (var property in typeof(T).GetProperties())
            {
                if (reader.IsDBNull(index))
                    property.SetValue(obj, null);
                else if (property.PropertyType == typeof(string))
                    property.SetValue(obj, reader.GetString(index));
                else if (property.PropertyType == typeof(int))
                    property.SetValue(obj, reader.GetInt32(index));
                else if (property.PropertyType == typeof(bool))
                    property.SetValue(obj, reader.GetBoolean(index));
                else if (property.PropertyType == typeof(float))
                    property.SetValue(obj, reader.GetFloat(index));
                else
                    throw new NotImplementedException(
                        $"Not implemented type : {property.PropertyType.Name}"
                    );
                index++;
            }
            Objects.Add(obj);
        }
        connection.Close();
    }

    /// <inheritdoc />
    public void Add(T obj)
    {
        var connection = new SQLiteConnection(
            $"Data Source={DbFile};Version={Version};New=True;Compress=True;"
        );
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"INSERT INTO {typeof(T).Name} VALUES ({string.Join(", ", typeof(T).GetProperties().Select(x => $"@{x.Name}"))});";
        foreach (var property in typeof(T).GetProperties())
        {
            var value = property.GetValue(obj);
            if (value == null)
                cmd.Parameters.AddWithValue($"@{property.Name}", DBNull.Value);
            else if (property.PropertyType == typeof(string))
                cmd.Parameters.AddWithValue($"@{property.Name}", value.ToString());
            else if (property.PropertyType == typeof(int))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToInt32(value));
            else if (property.PropertyType == typeof(bool))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToBoolean(value));
            else if (property.PropertyType == typeof(float))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToSingle(value));
            else
                throw new NotImplementedException(
                    $"Not implemented type : {property.PropertyType.Name}"
                );
        }
        cmd.ExecuteNonQuery();
        connection.Close();

        Objects.Add(obj);
    }

    /// <inheritdoc />
    public void Remove(T obj)
    {
        var connection = new SQLiteConnection(
            $"Data Source={DbFile};Version={Version};New=True;Compress=True;"
        );
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = $"DELETE FROM {typeof(T).Name} WHERE {string.Join(" AND ", typeof(T).GetProperties().Select(x => $"{x.Name} = @{x.Name}"))};";
        foreach (var property in typeof(T).GetProperties())
        {
            var value = property.GetValue(obj);
            if (value == null)
                cmd.Parameters.AddWithValue($"@{property.Name}", DBNull.Value);
            else if (property.PropertyType == typeof(string))
                cmd.Parameters.AddWithValue($"@{property.Name}", value.ToString());
            else if (property.PropertyType == typeof(int))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToInt32(value));
            else if (property.PropertyType == typeof(bool))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToBoolean(value));
            else if (property.PropertyType == typeof(float))
                cmd.Parameters.AddWithValue($"@{property.Name}", Convert.ToSingle(value));
            else
                throw new NotImplementedException(
                    $"Not implemented type : {property.PropertyType.Name}"
                );
        }
        cmd.ExecuteNonQuery();
        connection.Close();
        Objects.Remove(obj);
    }

    /// <inheritdoc />
    public IEnumerable<T> Get(Func<T, bool> predicate)
    {
        return Objects.Where(predicate);
    }
}
