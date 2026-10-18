public class DatabasePlan
{
    public int UserId { get; set; }
    public string PlanName { get; set; } = "";
    public List<DatabaseWeek> Weeks { get; set; } = new();
}

public class DatabaseWeek
{
    public decimal TotalBudget { get; set; }
    public List<DatabaseDay> Days { get; set; } = new();
}

public class DatabaseDay
{
    public int DayNameId { get; set; }
    public DateTime Date { get; set; }
    public decimal DayBudget { get; set; }
    public List<int> RecipeIds { get; set; } = new();
}