﻿namespace ForumSystem.Web.Areas.Administration.Controllers
{
    using System;
    using System.Threading.Tasks;

    using ForumSystem.Services.Data;
    using ForumSystem.Web.ViewModels.Administration.Posts;
    using ForumSystem.Web.ViewModels.PartialViews;
    using ForumSystem.Web.ViewModels.Posts;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    using static ForumSystem.Common.GlobalConstants;

    public class PostsController : AdministrationController
    {
        private const int PostsPerPage = 5;

        private readonly IPostsService postsService;
        private readonly ICategoriesService categoriesService;

        public PostsController(
            IPostsService postsService,
            ICategoriesService categoriesService)
        {
            this.postsService = postsService;
            this.categoriesService = categoriesService;
        }

        [Authorize(Roles = AdministratorRoleName)]
        public async Task<IActionResult> Index(int id)
        {
            var page = Math.Max(1, id);

            var posts = await this.postsService
                .GetAllAsync<PostCrudModel>(PostsPerPage, (page - 1) * PostsPerPage);

            var postCount = this.postsService.GetCount();

            var pagesCount = (int)Math.Ceiling((decimal)postCount / PostsPerPage);

            var viewModel = new PostCrudModelList
            {
                Posts = posts,
                PaginationModel = new PaginationViewModel
                {
                    CurrentPage = page,
                    TotalPages = pagesCount,
                    RouteName = "areaRoute",
                },
            };

            return this.View(viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = this.postsService
                .GetById<PostEditModel>(id);

            if (post == null)
            {
                return this.NotFound();
            }

            var categories = await this.categoriesService
                .GetAllAsync<CategoryDropDownViewModel>();

            post.Categories = categories;


            return this.View(post);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(PostEditModel editModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(editModel.Id);
            }

            try
            {
                await this.postsService
                .EditAsync(
                    editModel.Id,
                    editModel.Title,
                    editModel.Content,
                    editModel.CategoryId);
            }
            catch
            {
                return this.NotFound();
            }

            return this.RedirectToAction("ById", "Posts", new { editModel.Id, area = string.Empty });
        }

        [Authorize]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return this.NotFound();
            }

            var post = this.postsService
                .GetById<PostEditModel>(id.Value);

            if (post == null)
            {
                return this.NotFound();
            }

            return this.View(post);
        }

        [HttpPost]
        [ActionName("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await this.postsService.DeleteAsync(id);
            }
            catch
            {
                return this.NotFound();
            }

            return this.RedirectToAction("All", "Posts", new { area = string.Empty, id = 1 });
        }
    }
}
