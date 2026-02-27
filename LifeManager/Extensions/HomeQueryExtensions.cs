using Microsoft.EntityFrameworkCore;
using LifeManager.Data;
using LifeManager.Model;

namespace LifeManager.Extensions;

public static class HomeQueryExtensions
{
    public static IQueryable<Room> GetRoomsByHome(this IQueryable<Room> query, int homeId)
    {
        return query.Where(room => room.Home!.Id == homeId);
    }
    
    public static IQueryable<HouseTask>? GetTasksByHome(this IQueryable<HouseTask> query, int homeId)
    {
        return query.Where(task => task.Room.Home!.Id == homeId);
    }
}