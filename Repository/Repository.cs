using FourtitudeIntegrated.DbContexts;
using Microsoft.EntityFrameworkCore;
using Prema.PriceHarborScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarborScraper.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private PriceHarborContext priceHarborContext;
        private readonly DbSet<T> _dbSet;

        public Repository(PriceHarborContext priceHarborContext)
        {
            this.priceHarborContext = priceHarborContext;
            _dbSet = priceHarborContext.Set<T>();
        }

        public T GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public IEnumerable<T> GetAll()
        {
            return _dbSet.ToList();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void AddList(List<T> entities)
        {
            _dbSet.AddRange(entities);
            priceHarborContext.SaveChanges();
        }

        public void Update(T entity)
        {
            priceHarborContext.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }


        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    priceHarborContext.Dispose();
                }
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
