﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using StarLink_Blog.Data;
using StarLink_Blog.Extensions;
using StarLink_Blog.Models;
using StarLink_Blog.Services;
using StarLink_Blog.Services.Interfaces;
using X.PagedList;

namespace StarLink_Blog.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class BlogPostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IBlogPostService _blogPostService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IEmailSender _emailService;

        public BlogPostsController(ApplicationDbContext context,
                                     IImageService imageService, 
                                     IBlogPostService blogPostService,
                                     UserManager<BlogUser> userManager, IEmailSender emailService)
        {
            _context = context;
            _imageService = imageService;
            _blogPostService = blogPostService;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: BlogPosts
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var blogPosts = (await _blogPostService.GetAllBlogPostsAsync())
                                                          .Where(b => b.IsDeleted == false && b.IsPublished == true);
            return View(blogPosts);
        }


    


        // GET: BlogPosts/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(string? slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                                         .Include(b => b.Category)
                                         .Include(b=>b.Comments)
                                             .ThenInclude(c=>c.Author)
                                         .FirstOrDefaultAsync(m => m.Slug == slug);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // GET: BlogPosts/Create
        
        [Authorize(Roles = "Administrator , Moderator")]
        public async Task<IActionResult> Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name");
            return View(new BlogPost());
        }

        // POST: BlogPosts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator , Moderator")]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,CategoryId,Abstract,IsDeleted,IsPublished,BlogPostImage")] BlogPost blogPost,string stringTags)
        {



            ModelState.Remove("CreatorId");


            if (ModelState.IsValid)
            {
                blogPost.CreatorId = _userManager.GetUserId(User);

                //Get Slug

                if (!await _blogPostService.ValidateSlugAsync(blogPost.Title!,blogPost.Id))
                {
                    ModelState.AddModelError("Title", "A similar Title or Slug has already been used! ");
                    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                    ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name");
                    return View(blogPost);
                }
                blogPost.Slug = blogPost.Title!.Slugify();


                blogPost.CreatedDate = DateTime.UtcNow;


                if(blogPost.BlogPostImage != null)
                {
                    blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.BlogPostImage);
                    blogPost.ImageType = blogPost.BlogPostImage.ContentType;
                }


                _context.Add(blogPost);
                await _context.SaveChangesAsync();

                await _blogPostService.AddTagsToBlogPostAsync(stringTags, blogPost.Id);
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Edit/5
        [Authorize(Roles = "Administrator , Moderator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts.Include(b=>b.Tags).FirstOrDefaultAsync(b=>b.Id == id);

            if (blogPost == null)
            {
                return NotFound();
            }

            IEnumerable<int> blogPostTags = blogPost.Tags.Select(t => t.Id);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            ViewData["BlogPostTags"] = new MultiSelectList(await _blogPostService.GetTagsAsync(), "Id", "Name", blogPostTags);

            return View(blogPost);
        }

        // POST: BlogPosts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator , Moderator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreatorId,CreatedDate,LastUpdated,CategoryId,Slug,Abstract,IsDeleted,IsPublished,ImageData,ImageType, BlogPostImage")] BlogPost blogPost,IEnumerable<int> selectedTags)
        {
            if (id != blogPost.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    blogPost.CreatedDate = DateTime.SpecifyKind(blogPost.CreatedDate, DateTimeKind.Utc);
                    blogPost.LastUpdated = DateTime.UtcNow;


                    if (blogPost.BlogPostImage != null)
                    {
                        blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.BlogPostImage);
                        blogPost.ImageType = blogPost.BlogPostImage.ContentType;
                    }

                    if (!await _blogPostService.ValidateSlugAsync(blogPost.Title!, blogPost.Id))
                    {
                        ModelState.AddModelError("Title", "A similar Title or Slug has already been used! ");
                        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
                        return View(blogPost);
                    }
                    blogPost.Slug = blogPost.Title!.Slugify();



                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();


                    await _blogPostService.RemoveAllBlogPostTagsAsync(blogPost.Id);


                    await _blogPostService.AddTagsToBlogPostAsync(selectedTags, blogPost.Id);


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);
            return View(blogPost);
        }

        // GET: BlogPosts/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlogPosts == null)
            {
                return NotFound();
            }

            var blogPost = await _context.BlogPosts
                .Include(b => b.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        // POST: BlogPosts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlogPosts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.BlogPosts'  is null.");
            }
            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost != null)
            {
                blogPost.IsDeleted = true;
                
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogPostExists(int id)
        {
          return _context.BlogPosts.Any(e => e.Id == id);
        }


        [Authorize]
        public async Task<IActionResult> ContactMe()
        {
            EmailData model = new()
            {
                EmailAddress = User.Identity!.Name,
                Subject = "Contact Me: From My Blog"
            };
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe(EmailData model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _emailService.SendEmailAsync(model.EmailAddress!, model.Subject!, model.Message!);
                    //TODO: ADD some alert 

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "Home");
                    throw;
                }
            }

            //TODO: Add some alert 
            return RedirectToAction("ContactMe", "BlogPosts");
        }


    }
}
