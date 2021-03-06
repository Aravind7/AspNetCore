﻿using EmployeeWebApi.Models;
using EmployeeWebApi.Repositories.Interface;
using System;

namespace EmployeeWebApi.Repositories.Repository
{
    public class UnitOfWorks : IUnitOfWorks
    {
        /// <summary>
        /// The DbContext
        /// </summary>
        private EmployeeDbContext _dbContext;

        /// <summary>
        /// Initializes a new instance of the UnitOfWork class.
        /// </summary>
        /// <param name="context">The object context</param>
        public UnitOfWorks(EmployeeDbContext context)
        {
            _dbContext = context;
        }



        /// <summary>
        /// Saves all pending changes
        /// </summary>
        /// <returns>The number of objects in an Added, Modified, or Deleted state</returns>
        public void Commit()
        {
            // Save changes with the default options
            _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Disposes the current object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all external resources.
        /// </summary>
        /// <param name="disposing">The dispose indicator.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dbContext != null)
                {
                    _dbContext.Dispose();
                    _dbContext = null;
                }
            }
        }
    }
}
