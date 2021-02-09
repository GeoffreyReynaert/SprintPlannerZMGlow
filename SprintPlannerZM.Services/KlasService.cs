﻿using SprintPlannerZM.Model;
using SprintPlannerZM.Repository;
using SprintPlannerZM.Services.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace SprintPlannerZM.Services
{
    public class KlasService: IKlasService
    {

        private readonly TihfDbContext _database;

        public KlasService(TihfDbContext database)
        {
            _database = database;
        }
        public Klas Get(int id)
        {
            return _database.Klas.SingleOrDefault(l => l.klasID == id);
        }

        public IList<Klas> Find()
        {
            return _database.Klas.ToList();
        }

        public Klas Create(Klas klas)
        {
            _database.Klas.Add(klas);
            _database.SaveChanges();
            return klas;
        }

        public Klas Update(int id, Klas klas)
        {
            {
                var dbKlas = Get(id);
                if (dbKlas == null)
                {
                    return klas;
                }
                _database.Klas.Update(dbKlas);
                _database.SaveChanges();
                return klas;
            }
        }

        public bool Delete(int id)
        {
            {
                var dbKlas = Get(id);
                if (dbKlas == null)
                {
                    return false;
                }
                _database.Klas.Remove(dbKlas);
                _database.SaveChanges();
                return true;
            }
        }
    }
}
