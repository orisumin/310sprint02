public class MealPlanService
{
    private readonly PlanRepository _planRepository;

    public MealPlanService(string connectionString)
    {
        _planRepository =
            new PlanRepository(connectionString);
    }

    public async Task<int> SavePlanAsync(DatabasePlan plan)
    {
        return await _planRepository.SaveAsync(plan);
    }
}