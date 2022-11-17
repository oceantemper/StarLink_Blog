using StarLink_Blog.Models;

namespace StarLink_Blog.Services.Interfaces
{

    public interface IBlogPostService
    {
        public Task AddTagsToBlogPostAsync(IEnumerable<int> tagIds, int blogPostId);
        Task AddTagsToBlogPostAsync(string tagNames, int blogPostId);

        public Task<bool> ValidateSlugAsync(string title, int BlogPostId);


        public Task<List<BlogPost>> GetAllBlogPostsAsync();


        public Task<List<BlogPost>> GetPopularBlogPostAsync();


        public Task<List<BlogPost>> GetRecentBlogPostAsync(int count);


        public Task<List<Category>> GetCategoriesAsync();


        public Task<List<Tag>> GetTagsAsync();


        public Task<List<Tag>> GetBlogPostTags(int blogPostId);

        public Task RemoveAllBlogPostTagsAsync(int blogPostId);

        public  IEnumerable<BlogPost> SearchBlogPosts(string searchString);

       
    }
}
