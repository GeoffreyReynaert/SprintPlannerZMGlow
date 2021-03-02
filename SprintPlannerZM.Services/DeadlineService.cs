using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Services.Abstractions;

namespace SprintPlannerZM.Services
{
    public class DeadlineService: IDeadlineService

    {
    private readonly TihfDbContext _database;

    public DeadlineService(TihfDbContext database)
    {
        _database = database;
    }

    public async Task<Deadline> Get(int id)
    {
        return await _database.Deadline.SingleOrDefaultAsync(d => d.deadlineID == id);
    }

    public async Task<IList<Deadline>> Find()
    {
        return await _database.Deadline.ToListAsync();
    }

    public async Task<Deadline> Create(Deadline deadline)
    {
       await _database.Deadline.AddAsync(deadline);
       await _database.SaveChangesAsync();
        return deadline;
    }

    public async Task<Deadline> Update(int id, Deadline deadline)
    {
        {
            var dbDeadline =await Get(id);
            if (dbDeadline == null)
            {
                return deadline;
            }

            _database.Deadline.Update(dbDeadline);
            await _database.SaveChangesAsync();
            return deadline;
        }
    }

    public async Task<bool> Delete(int id)
    {
        {
            var dbDeadline = await Get(id);
            if (dbDeadline == null)
            {
                return false;
            }

            _database.Deadline.Remove(dbDeadline);
            await _database.SaveChangesAsync();
            return true;
        }
    }
    }
}
