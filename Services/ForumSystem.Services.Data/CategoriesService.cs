﻿namespace ForumSystem.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ForumSystem.Data.Common.Repositories;
    using ForumSystem.Data.Models;
    using ForumSystem.Services.Mapping;
    using ForumSystem.Web.ViewModels.Administration.Categories;
    using Microsoft.EntityFrameworkCore;

    public class CategoriesService : ICategoriesService
    {
        private readonly IDeletableEntityRepository<Category> categories;

        public CategoriesService(IDeletableEntityRepository<Category> categories)
        {
            this.categories = categories;
        }

        public async Task AddAsync(CategoryInputModel input)
        {
            var category = new Category
            {
                Name = input.Name,
                Description = input.Description,
                Image = input.Image,
            };

            await this.categories.AddAsync(category);
            await this.categories.SaveChangesAsync();
        }

        public async Task EditAsync(int id, CategoryInputModel input)
        {
            var categoryToEdit = this.categories
                .All()
                .FirstOrDefault(x => x.Id == id);

            if (categoryToEdit == null)
            {
                throw new ArgumentNullException(nameof(categoryToEdit));
            }

            categoryToEdit.Name = input.Name;
            categoryToEdit.Description = input.Description;
            categoryToEdit.Image = input.Image;

            this.categories.Update(categoryToEdit);
            await this.categories.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var categoryToDelete = this.categories
                .All()
                .FirstOrDefault(x => x.Id == id);

            if (categoryToDelete == null)
            {
                throw new ArgumentNullException(nameof(categoryToDelete));
            }

            this.categories.Delete(categoryToDelete);
            await this.categories.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>()
        {
            var categories = await this.categories.All()
                .To<T>()
                .ToListAsync();

            return categories;
        }

        public async Task<T> GetByIdAsync<T>(int id)
            => await this.categories
                .All()
                .Where(x => x.Id == id)
                .To<T>()
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<T>> GetMostPostsCategories<T>(int take = 5)
            => await this.categories
                .All()
                .OrderByDescending(x => x.Posts.Count())
                .Take(take)
                .To<T>()
                .ToListAsync();
    }
}
