using MySqlConnector;

public class PlanRepository
{
    private readonly string _connectionString;

    public PlanRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<int> SaveAsync(DatabasePlan plan)
    {
        await using var connection =
            new MySqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            // Insert plan
            const string planSql = """
                INSERT INTO plan (user_id, plan_name)
                VALUES (@userId, @planName);
                """;

            await using var planCommand =
                new MySqlCommand(planSql, connection, transaction);

            planCommand.Parameters.AddWithValue("@userId", plan.UserId);
            planCommand.Parameters.AddWithValue("@planName", plan.PlanName);

            await planCommand.ExecuteNonQueryAsync();

            int planId = await GetLastInsertedIdAsync(
                connection,
                transaction);

            foreach (DatabaseWeek week in plan.Weeks)
            {
                // Insert week
                const string weekSql = """
                    INSERT INTO week_plan
                        (plan_id, total_budget)
                    VALUES
                        (@planId, @totalBudget);
                    """;

                await using var weekCommand =
                    new MySqlCommand(weekSql, connection, transaction);

                weekCommand.Parameters.AddWithValue("@planId", planId);
                weekCommand.Parameters.AddWithValue(
                    "@totalBudget",
                    week.TotalBudget);

                await weekCommand.ExecuteNonQueryAsync();

                int weekId = await GetLastInsertedIdAsync(
                    connection,
                    transaction);

                foreach (DatabaseDay day in week.Days)
                {
                    // Insert day
                    const string daySql = """
                        INSERT INTO day_plan
                            (day_name, week_id, `date`, day_budget)
                        VALUES
                            (@dayName, @weekId, @date, @dayBudget);
                        """;

                    await using var dayCommand =
                        new MySqlCommand(daySql, connection, transaction);

                    dayCommand.Parameters.AddWithValue(
                        "@dayName",
                        day.DayNameId);

                    dayCommand.Parameters.AddWithValue(
                        "@weekId",
                        weekId);

                    dayCommand.Parameters.AddWithValue(
                        "@date",
                        day.Date);

                    dayCommand.Parameters.AddWithValue(
                        "@dayBudget",
                        day.DayBudget);

                    await dayCommand.ExecuteNonQueryAsync();

                    int dayId = await GetLastInsertedIdAsync(
                        connection,
                        transaction);

                    foreach (int recipeId in day.RecipeIds)
                    {
                        const string recipeSql = """
                            INSERT INTO daily_plan_has_recipe
                                (recipe_id, day_id)
                            VALUES
                                (@recipeId, @dayId);
                            """;

                        await using var recipeCommand =
                            new MySqlCommand(
                                recipeSql,
                                connection,
                                transaction);

                        recipeCommand.Parameters.AddWithValue(
                            "@recipeId",
                            recipeId);

                        recipeCommand.Parameters.AddWithValue(
                            "@dayId",
                            dayId);

                        await recipeCommand.ExecuteNonQueryAsync();
                    }
                }
            }

            await transaction.CommitAsync();

            return planId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task<int> GetLastInsertedIdAsync(
        MySqlConnection connection,
        MySqlTransaction transaction)
    {
        const string sql = "SELECT LAST_INSERT_ID();";

        await using var command =
            new MySqlCommand(sql, connection, transaction);

        return Convert.ToInt32(
            await command.ExecuteScalarAsync());
    }
}