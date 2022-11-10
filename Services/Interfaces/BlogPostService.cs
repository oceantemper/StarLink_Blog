﻿using Microsoft.EntityFrameworkCore;
using StarLink_Blog.Data;
using StarLink_Blog.Extensions;
using StarLink_Blog.Models;
using System.Text.RegularExpressions;

namespace StarLink_Blog.Services.Interfaces
{
    public class BlogPostService : IBlogPostService
    {

        private readonly ApplicationDbContext _context;


        public BlogPostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTagsToBlogPostAsync(IEnumerable<int> tagIds, int blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts.FindAsync(blogPostId);

                foreach(int tagId in tagIds)
                {
                    Tag? tag = await _context.Tags.FindAsync(tagId);

                    if (blogPost != null && tag != null)
                    {
                        blogPost.Tags.Add(tag);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BlogPost>> GetAllBlogPostsAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                         .Include(b => b.Comments)
                                                         .Include(b => b.Category)
                                                         .Include(b => b.Tags)
                                                         .OrderByDescending(b => b.CreatedDate)
                                                         .ToListAsync();

                return blogPosts;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<Tag>> GetBlogPostTags(int blogPostId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                return await _context.Categories.Include(c =>c.BlogPost).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BlogPost>> GetPopularBlogPostAsync()
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                         .Include(b => b.Comments)
                                                             .ThenInclude(c => c.Author)
                                                         .Include(b => b.Category)
                                                         .Include(b => b.Tags)
                                                         .ToListAsync();

                return blogPosts.OrderByDescending(b => b.Comments.Count).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BlogPost>> GetRecentBlogPostAsync(int count)
        {
            try
            {
                List<BlogPost> blogPosts = await _context.BlogPosts
                                                       .Where(b=>b.IsDeleted == false && b.IsPublished == true)
                                                       .Include(b => b.Comments)
                                                           .ThenInclude(c => c.Author)
                                                       .Include(b => b.Category)
                                                       .Include(b => b.Tags)
                                                       .ToListAsync();


                return blogPosts.OrderByDescending(b => b.CreatedDate).Take(count).ToList();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Tag>> GetTagsAsync()
        {
            try
            {
                List<Tag> Tags = await _context.Tags.Include(t => t.BlogPosts).ToListAsync();
                return Tags;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveAllBlogPostTagsAsync(int blogPostId)
        {
            try
            {
                BlogPost? blogPost = await _context.BlogPosts.Include(b => b.Tags).FirstOrDefaultAsync(b =>b.Id == blogPostId);


                blogPost!.Tags.Clear();
                _context.Update(blogPost);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> ValidateSlugAsync(string title, int blogPostId)
        {
            try
            {
                string newSlug = title.Slugify();

                if(blogPostId == 0)
                {
                    return !(await _context.BlogPosts.AnyAsync(b=>b.Slug == newSlug));
                }
                else
                {
                    BlogPost? blogPost = await _context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == blogPostId);

                    string? oldSlug = blogPost?.Slug;

                    //checks to see if the tittle/newSlug is the same as the old tittle
                    if(!string.Equals(newSlug,oldSlug))
                    {
                        //now check to see if the title/newslug alrady exists in the database
                        return !(await _context.BlogPosts.AnyAsync(b => b.Id != blogPostId && b.Slug == newSlug));
                    }
                }

                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}