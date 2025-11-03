using Microsoft.EntityFrameworkCore;
using GeoStud.Api.Models;

namespace GeoStud.Api.Data;

public static class RostovSeedData
{
    public static async Task SeedRostovLocationsAsync(GeoStudDbContext context)
    {
        // All location seed data has been removed
        await Task.CompletedTask;
    }
}
