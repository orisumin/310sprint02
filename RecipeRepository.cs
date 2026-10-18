using MySqlConnector;

public class RecipeRepository
{
    private readonly string _connectionString;

    public RecipeRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SaveAsync(Recipe recipe)
    {
        const string sql = """
            INSERT INTO recipe
                (recipe_id, name, quantity, calories, price)
            VALUES
                (@id, @name, @quantity, @calories, @price);
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", recipe.RecipeId);
        command.Parameters.AddWithValue("@name", recipe.Name);
        command.Parameters.AddWithValue("@quantity", recipe.Quantity);
        command.Parameters.AddWithValue("@calories", recipe.Calories);
        command.Parameters.AddWithValue("@price", recipe.Price);

        await command.ExecuteNonQueryAsync();
    }

    public async Task EditAsync(Recipe recipe)
    {
        const string sql = """
            UPDATE recipe
            SET name = @name,
                quantity = @quantity,
                calories = @calories,
                price = @price
            WHERE recipe_id = @id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", recipe.RecipeId);
        command.Parameters.AddWithValue("@name", recipe.Name);
        command.Parameters.AddWithValue("@quantity", recipe.Quantity);
        command.Parameters.AddWithValue("@calories", recipe.Calories);
        command.Parameters.AddWithValue("@price", recipe.Price);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int recipeId)
    {
        const string sql = """
            DELETE FROM recipe
            WHERE recipe_id = @id;
            """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue("@id", recipeId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<Recipe>> GetAllAsync()
    {
        const string sql = """
            SELECT recipe_id, name, quantity, calories, price
            FROM recipe
            ORDER BY recipe_id;
            """;

        var recipes = new List<Recipe>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            recipes.Add(new Recipe
            {
                RecipeId = reader.GetInt32("recipe_id"),
                Name = reader.GetString("name"),
                Quantity = reader.GetInt32("quantity"),
                Calories = reader.GetInt32("calories"),
                Price = reader.GetDecimal("price")
            });
        }

        return recipes;
    }
}