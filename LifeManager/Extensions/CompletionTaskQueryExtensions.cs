using Microsoft.EntityFrameworkCore;
using LifeManager.Data;

namespace LifeManager.Extensions;

public static class CompletionTaskQueryExtensions
{
    public static IQueryable<TaskCompletion>? GetCompletedTaskByUser(this IQueryable<TaskCompletion> query, int userId)
    {
        return query.Where(completion  => completion.CompletedBy.Id == userId);
    }
}