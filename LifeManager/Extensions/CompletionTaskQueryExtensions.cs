using Microsoft.EntityFrameworkCore;
using LifeManager.Data;

namespace LifeManager.Extensions;

public static class CompletionTaskQueryExtensions
{
    public static IQueryable<TaskCompletion>? GetCompletedTaskByUser(this IQueryable<TaskCompletion> query, User user)
    {
        return query.Where(completion  => completion.CompletedBy.Id == user.Id);
    }
}