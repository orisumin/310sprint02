using MySqlConnector;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(User user)
    {
        const string sql = """
            INSERT INTO `user`
                (user_id, user_name, email)
            VALUES
                (@id, @name, @email);
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", user.UserId);
        command.Parameters.AddWithValue("@name", user.UserName);
        command.Parameters.AddWithValue("@email", user.Email);

        await command.ExecuteNonQueryAsync();
    }

    public async Task EditAsync(User user)
    {
        const string sql = """
            UPDATE `user`
            SET user_name = @name,
                email = @email
            WHERE user_id = @id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", user.UserId);
        command.Parameters.AddWithValue("@name", user.UserName);
        command.Parameters.AddWithValue("@email", user.Email);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int userId)
    {
        const string sql = """
            DELETE FROM `user`
            WHERE user_id = @id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", userId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<User>> GetAllAsync()
    {
        const string sql = """
            SELECT user_id, user_name, email
            FROM `user`
            ORDER BY user_id;
            """;

        var users = new List<User>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                UserId = reader.GetInt32("user_id"),
                UserName = reader.GetString("user_name"),
                Email = reader.GetString("email")
            });
        }

        return users;
    }
}