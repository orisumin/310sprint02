using System.Globalization;
using MySqlConnector;

public class Options
{
    private const string ConnectionString =
        "Server=localhost;" +
        "Database=meal_budget;" +
        "User ID=root;" +
        "Password=itm222SQLpw@;";

    public async Task HandleOptionsAsync()
    {
        while (true)
        {
            Console.WriteLine("""
                
                MEAL BUDGET MENU
                1. Account menu
                2. Recipe menu
                3. Plan menu
                4. Exit
                """);

            string choice = ReadInput();

            try
            {
                switch (choice)
                {
                    case "1":
                        await AccountMenuAsync();
                        break;

                    case "2":
                        await RecipeMenuAsync();
                        break;

                    case "3":
                        await PlanMenuAsync();
                        break;

                    case "4":
                        return;

                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error: {exception.Message}");
            }
        }
    }

    private async Task AccountMenuAsync()
    {
        Console.WriteLine("""
            
            ACCOUNT MENU
            1. Save account
            2. Edit account
            3. Display accounts
            4. Delete account
            """);

        switch (ReadInput())
        {
            case "1":
                await SaveUserAsync();
                break;

            case "2":
                await EditUserAsync();
                break;

            case "3":
                await DisplayUsersAsync();
                break;

            case "4":
                await DeleteUserAsync();
                break;
        }
    }

    private async Task SaveUserAsync()
    {
        int id = ReadInteger("User ID: ");
        string name = ReadText("Username: ");
        string email = ReadText("Email: ");

        const string sql = """
            INSERT INTO `user` (user_id, user_name, email)
            VALUES (@id, @name, @email);
            """;

        await ExecuteAsync(
            sql,
            new MySqlParameter("@id", id),
            new MySqlParameter("@name", name),
            new MySqlParameter("@email", email));

        Console.WriteLine("Account saved.");
    }

    private async Task EditUserAsync()
    {
        int id = ReadInteger("User ID to edit: ");
        string name = ReadText("New username: ");
        string email = ReadText("New email: ");

        const string sql = """
            UPDATE `user`
            SET user_name = @name,
                email = @email
            WHERE user_id = @id;
            """;

        int rows = await ExecuteAsync(
            sql,
            new MySqlParameter("@id", id),
            new MySqlParameter("@name", name),
            new MySqlParameter("@email", email));

        Console.WriteLine(
            rows == 1
                ? "Account updated."
                : "Account not found.");
    }

    private async Task DisplayUsersAsync()
    {
        const string sql = """
            SELECT user_id, user_name, email
            FROM `user`
            ORDER BY user_id;
            """;

        await using var connection =
            new MySqlConnection(ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"{reader.GetInt32("user_id")}: " +
                $"{reader.GetString("user_name")} - " +
                $"{reader.GetString("email")}");
        }
    }

    private async Task DeleteUserAsync()
    {
        int userId = ReadInteger("User ID to delete: ");

        await ExecuteAsync(
            """
            DELETE FROM daily_plan_has_recipe
            WHERE day_id IN
            (
                SELECT d.day_plan_id
                FROM day_plan d
                INNER JOIN week_plan w
                    ON d.week_id = w.week_plan_id
                INNER JOIN plan p
                    ON w.plan_id = p.plan_id
                WHERE p.user_id = @userId
            );
            """,
            new MySqlParameter("@userId", userId));

        await ExecuteAsync(
            """
            DELETE FROM day_plan
            WHERE week_id IN
            (
                SELECT w.week_plan_id
                FROM week_plan w
                INNER JOIN plan p
                    ON w.plan_id = p.plan_id
                WHERE p.user_id = @userId
            );
            """,
            new MySqlParameter("@userId", userId));

        await ExecuteAsync(
            """
            DELETE FROM week_plan
            WHERE plan_id IN
            (
                SELECT plan_id
                FROM plan
                WHERE user_id = @userId
            );
            """,
            new MySqlParameter("@userId", userId));

        await ExecuteAsync(
            """
            DELETE FROM plan
            WHERE user_id = @userId;
            """,
            new MySqlParameter("@userId", userId));

        int rows = await ExecuteAsync(
            """
            DELETE FROM `user`
            WHERE user_id = @userId;
            """,
            new MySqlParameter("@userId", userId));

        Console.WriteLine(
            rows == 1
                ? "Account deleted."
                : "Account not found.");
    }

    private async Task RecipeMenuAsync()
    {
        Console.WriteLine("""
            
            RECIPE MENU
            1. Save recipe
            2. Edit recipe
            3. Display recipes
            4. Delete recipe
            """);

        switch (ReadInput())
        {
            case "1":
                await SaveRecipeAsync();
                break;

            case "2":
                await EditRecipeAsync();
                break;

            case "3":
                await DisplayRecipesAsync();
                break;

            case "4":
                await DeleteRecipeAsync();
                break;
        }
    }

    private async Task SaveRecipeAsync()
    {
        int id = ReadInteger("Recipe ID: ");
        string name = ReadText("Recipe name: ");
        int quantity = ReadInteger("Quantity: ");
        int calories = ReadInteger("Calories: ");
        decimal price = ReadDecimal("Price: ");

        const string sql = """
            INSERT INTO recipe
                (recipe_id, name, quantity, calories, price)
            VALUES
                (@id, @name, @quantity, @calories, @price);
            """;

        await ExecuteAsync(
            sql,
            new MySqlParameter("@id", id),
            new MySqlParameter("@name", name),
            new MySqlParameter("@quantity", quantity),
            new MySqlParameter("@calories", calories),
            new MySqlParameter("@price", price));

        Console.WriteLine("Recipe saved.");
    }

    private async Task EditRecipeAsync()
    {
        int id = ReadInteger("Recipe ID to edit: ");
        string name = ReadText("New recipe name: ");
        int quantity = ReadInteger("New quantity: ");
        int calories = ReadInteger("New calories: ");
        decimal price = ReadDecimal("New price: ");

        const string sql = """
            UPDATE recipe
            SET name = @name,
                quantity = @quantity,
                calories = @calories,
                price = @price
            WHERE recipe_id = @id;
            """;

        int rows = await ExecuteAsync(
            sql,
            new MySqlParameter("@id", id),
            new MySqlParameter("@name", name),
            new MySqlParameter("@quantity", quantity),
            new MySqlParameter("@calories", calories),
            new MySqlParameter("@price", price));

        Console.WriteLine(
            rows == 1
                ? "Recipe updated."
                : "Recipe not found.");
    }

    private async Task DisplayRecipesAsync()
    {
        const string sql = """
            SELECT recipe_id, name, quantity, calories, price
            FROM recipe
            ORDER BY recipe_id;
            """;

        await using var connection =
            new MySqlConnection(ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"{reader.GetInt32("recipe_id")}: " +
                $"{reader.GetString("name")} | " +
                $"{reader.GetInt32("quantity")} servings | " +
                $"{reader.GetInt32("calories")} calories | " +
                $"${reader.GetDecimal("price"):F2}");
        }
    }

    private async Task DeleteRecipeAsync()
    {
        int recipeId = ReadInteger("Recipe ID to delete: ");

        await ExecuteAsync(
            """
            DELETE FROM ingredient_quantity
            WHERE recipe_id = @recipeId;
            """,
            new MySqlParameter("@recipeId", recipeId));

        await ExecuteAsync(
            """
            DELETE FROM daily_plan_has_recipe
            WHERE recipe_id = @recipeId;
            """,
            new MySqlParameter("@recipeId", recipeId));

        int rows = await ExecuteAsync(
            """
            DELETE FROM recipe
            WHERE recipe_id = @recipeId;
            """,
            new MySqlParameter("@recipeId", recipeId));

        Console.WriteLine(
            rows == 1
                ? "Recipe deleted."
                : "Recipe not found.");
    }

    private async Task PlanMenuAsync()
    {
        Console.WriteLine("""
            
            PLAN MENU
            1. Save plan
            2. Save weekly plan
            3. Save daily plan
            4. Display plans
            5. Edit weekly budget
            6. Edit daily budget
            7. Delete daily plan
            8. Delete weekly plan
            9. Delete plan
            """);

        switch (ReadInput())
        {
            case "1":
                await SavePlanAsync();
                break;

            case "2":
                await SaveWeekAsync();
                break;

            case "3":
                await SaveDayAsync();
                break;

            case "4":
                await DisplayPlansAsync();
                break;

            case "5":
                await EditWeekAsync();
                break;

            case "6":
                await EditDayAsync();
                break;

            case "7":
                await DeleteDayAsync();
                break;

            case "8":
                await DeleteWeekAsync();
                break;

            case "9":
                await DeletePlanAsync();
                break;
        }
    }

    private async Task SavePlanAsync()
    {
        int userId = ReadInteger("User ID: ");
        string planName = ReadText("Plan name: ");

        const string sql = """
            INSERT INTO plan (user_id, plan_name)
            VALUES (@userId, @planName);
            """;

        int planId = await InsertAndGetIdAsync(
            sql,
            new MySqlParameter("@userId", userId),
            new MySqlParameter("@planName", planName));

        Console.WriteLine($"Plan saved. New plan ID: {planId}");
    }

    private async Task SaveWeekAsync()
    {
        int planId = ReadInteger("Plan ID: ");
        decimal budget = ReadDecimal("Weekly budget: ");

        const string sql = """
            INSERT INTO week_plan
                (plan_id, total_budget)
            VALUES
                (@planId, @budget);
            """;

        int weekId = await InsertAndGetIdAsync(
            sql,
            new MySqlParameter("@planId", planId),
            new MySqlParameter("@budget", budget));

        Console.WriteLine($"Weekly plan saved. New week ID: {weekId}");
    }

    private async Task SaveDayAsync()
    {
        int dayNameId = ReadInteger("Day name ID, 1-7: ");
        int weekId = ReadInteger("Week plan ID: ");
        DateTime date = ReadDate("Date, YYYY-MM-DD: ");
        decimal budget = ReadDecimal("Daily budget: ");

        const string sql = """
            INSERT INTO day_plan
                (day_name, week_id, `date`, day_budget)
            VALUES
                (@dayName, @weekId, @date, @budget);
            """;

        int dayId = await InsertAndGetIdAsync(
            sql,
            new MySqlParameter("@dayName", dayNameId),
            new MySqlParameter("@weekId", weekId),
            new MySqlParameter("@date", date.Date),
            new MySqlParameter("@budget", budget));

        Console.WriteLine($"Daily plan saved. New day ID: {dayId}");
    }

    private async Task DisplayPlansAsync()
    {
        const string sql = """
            SELECT
                p.plan_id,
                p.plan_name,
                p.user_id,
                w.week_plan_id,
                w.total_budget,
                d.day_plan_id,
                dn.day_name,
                d.date,
                d.day_budget
            FROM plan p
            LEFT JOIN week_plan w
                ON w.plan_id = p.plan_id
            LEFT JOIN day_plan d
                ON d.week_id = w.week_plan_id
            LEFT JOIN day_name dn
                ON dn.day_id = d.day_name
            ORDER BY
                p.plan_id,
                w.week_plan_id,
                d.day_plan_id;
            """;

        await using var connection =
            new MySqlConnection(ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            Console.WriteLine(
                $"Plan ID: {reader["plan_id"]} | " +
                $"Name: {reader["plan_name"]} | " +
                $"User ID: {reader["user_id"]} | " +
                $"Week ID: {reader["week_plan_id"]} | " +
                $"Week budget: {reader["total_budget"]} | " +
                $"Day ID: {reader["day_plan_id"]} | " +
                $"Day: {reader["day_name"]} | " +
                $"Date: {reader["date"]} | " +
                $"Day budget: {reader["day_budget"]}");
        }
    }

    private async Task EditWeekAsync()
    {
        int weekId = ReadInteger("Week plan ID: ");
        decimal budget = ReadDecimal("New weekly budget: ");

        const string sql = """
            UPDATE week_plan
            SET total_budget = @budget
            WHERE week_plan_id = @weekId;
            """;

        int rows = await ExecuteAsync(
            sql,
            new MySqlParameter("@weekId", weekId),
            new MySqlParameter("@budget", budget));

        Console.WriteLine(
            rows == 1
                ? "Week updated."
                : "Week not found.");
    }

    private async Task EditDayAsync()
    {
        int dayId = ReadInteger("Day plan ID: ");
        decimal budget = ReadDecimal("New daily budget: ");
        DateTime date = ReadDate("New date, YYYY-MM-DD: ");

        const string sql = """
            UPDATE day_plan
            SET day_budget = @budget,
                `date` = @date
            WHERE day_plan_id = @dayId;
            """;

        int rows = await ExecuteAsync(
            sql,
            new MySqlParameter("@dayId", dayId),
            new MySqlParameter("@budget", budget),
            new MySqlParameter("@date", date.Date));

        Console.WriteLine(
            rows == 1
                ? "Day updated."
                : "Day not found.");
    }

    private async Task DeleteDayAsync()
    {
        int dayId = ReadInteger("Day plan ID to delete: ");

        await ExecuteAsync(
            """
            DELETE FROM daily_plan_has_recipe
            WHERE day_id = @dayId;
            """,
            new MySqlParameter("@dayId", dayId));

        int rows = await ExecuteAsync(
            """
            DELETE FROM day_plan
            WHERE day_plan_id = @dayId;
            """,
            new MySqlParameter("@dayId", dayId));

        Console.WriteLine(
            rows == 1
                ? "Day deleted."
                : "Day not found.");
    }

    private async Task DeleteWeekAsync()
    {
        int weekId = ReadInteger("Week plan ID to delete: ");

        await ExecuteAsync(
            """
            DELETE FROM daily_plan_has_recipe
            WHERE day_id IN
            (
                SELECT day_plan_id
                FROM day_plan
                WHERE week_id = @weekId
            );
            """,
            new MySqlParameter("@weekId", weekId));

        await ExecuteAsync(
            """
            DELETE FROM day_plan
            WHERE week_id = @weekId;
            """,
            new MySqlParameter("@weekId", weekId));

        int rows = await ExecuteAsync(
            """
            DELETE FROM week_plan
            WHERE week_plan_id = @weekId;
            """,
            new MySqlParameter("@weekId", weekId));

        Console.WriteLine(
            rows == 1
                ? "Week deleted."
                : "Week not found.");
    }

    private async Task DeletePlanAsync()
    {
        int planId = ReadInteger("Plan ID to delete: ");

        await ExecuteAsync(
            """
            DELETE FROM daily_plan_has_recipe
            WHERE day_id IN
            (
                SELECT d.day_plan_id
                FROM day_plan d
                INNER JOIN week_plan w
                    ON d.week_id = w.week_plan_id
                WHERE w.plan_id = @planId
            );
            """,
            new MySqlParameter("@planId", planId));

        await ExecuteAsync(
            """
            DELETE FROM day_plan
            WHERE week_id IN
            (
                SELECT week_plan_id
                FROM week_plan
                WHERE plan_id = @planId
            );
            """,
            new MySqlParameter("@planId", planId));

        await ExecuteAsync(
            """
            DELETE FROM week_plan
            WHERE plan_id = @planId;
            """,
            new MySqlParameter("@planId", planId));

        int rows = await ExecuteAsync(
            """
            DELETE FROM plan
            WHERE plan_id = @planId;
            """,
            new MySqlParameter("@planId", planId));

        Console.WriteLine(
            rows == 1
                ? "Plan deleted."
                : "Plan not found.");
    }

    private static async Task<int> ExecuteAsync(
        string sql,
        params MySqlParameter[] parameters)
    {
        await using var connection =
            new MySqlConnection(ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddRange(parameters);

        return await command.ExecuteNonQueryAsync();
    }

    private static async Task<int> InsertAndGetIdAsync(
        string sql,
        params MySqlParameter[] parameters)
    {
        await using var connection =
            new MySqlConnection(ConnectionString);

        await connection.OpenAsync();

        await using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddRange(parameters);

        await command.ExecuteNonQueryAsync();

        await using var idCommand =
            new MySqlCommand(
                "SELECT LAST_INSERT_ID();",
                connection);

        return Convert.ToInt32(
            await idCommand.ExecuteScalarAsync());
    }

    public static int ReadInteger()
    {
        return ReadInteger("Enter an integer: ");
    }

    public static int ReadInteger(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (int.TryParse(
                Console.ReadLine(),
                out int value))
            {
                return value;
            }

            Console.WriteLine("Please enter a valid integer.");
        }
    }

    private static decimal ReadDecimal(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (decimal.TryParse(
                Console.ReadLine(),
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimal value))
            {
                return value;
            }

            Console.WriteLine("Please enter a valid decimal number.");
        }
    }

    public static string ReadInput()
    {
        return Console.ReadLine() ?? "";
    }

    private static string ReadText(string message)
    {
        while (true)
        {
            Console.Write(message);

            string value = Console.ReadLine() ?? "";

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("Input cannot be empty.");
        }
    }

    private static DateTime ReadDate(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (DateTime.TryParseExact(
                Console.ReadLine(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime date))
            {
                return date;
            }

            Console.WriteLine(
                "Please enter the date using YYYY-MM-DD.");
        }
    }
}