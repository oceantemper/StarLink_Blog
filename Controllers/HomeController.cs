using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarLink_Blog.Data;
using StarLink_Blog.Models;
using StarLink_Blog.Services.Interfaces;
using System.Diagnostics;
using X.PagedList;

namespace StarLink_Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IBlogPostService _blogPostService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IBlogPostService blogPostService)
        {
            _logger = logger;
            _context = context;
            _blogPostService = blogPostService;

        }

        public async Task<IActionResult> Index(int? pageNum)
        {
            //lists navigation for the pages in the blog 
            int pageSize = 3;
            //if pageNum == null give it 1 
            int page = pageNum ?? 1;

            IPagedList<BlogPost> model = (await _blogPostService.GetAllBlogPostsAsync())
                                                          .Where(b=>b.IsDeleted == false && b.IsPublished == true)
                                                          .ToPagedList(page,pageSize);

            return View(model);
        }




        public async Task<IActionResult> SearchIndex(string searchString, int? pageNum)
        {
            //lists navigation for the pages in the blog 
            int pageSize = 3;
            //if pageNum == null give it 1 
            int page = pageNum ?? 1;

            ViewData["SearchString"] = searchString;

            IPagedList<BlogPost> model = _blogPostService.SearchBlogPosts(searchString).ToPagedList(page, pageSize);

            return View(nameof(Index), model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}