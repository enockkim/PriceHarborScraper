﻿using Prema.PriceHarborScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prema.PriceHarborScraper.Repository
{
    public interface IRepository<T> : IDisposable
    {   
        T GetById(int id);
        IEnumerable<T> GetAll();
        void Add(T entity);
        void AddList(List<T> entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
