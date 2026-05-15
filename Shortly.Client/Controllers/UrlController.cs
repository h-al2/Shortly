using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Shortly.Client.Data.ViewModels;
using Shortly.Client.Helpers.Roles;
using Shortly.Data.Models;
using Shortly.Data.Services;
using System.Security.Claims;

namespace Shortly.Client.Controllers
{
    public class UrlController : Controller
    {
        private IUrlsService _urlsService;
        private readonly IMapper _mapper;
        public UrlController(IUrlsService urlsService, IMapper mapper) 
        { 
            _urlsService = urlsService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole(Role.Admin);

            var allUrls = await _urlsService.GetUrlsAsync(loggedInUserId, isAdmin);
            var mappedAllUrls = _mapper.Map<List<Url>, List<GetUrlVM>>(allUrls);

            return View(mappedAllUrls);
        }

        public async Task<IActionResult> Create()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            await _urlsService.DeleteAsync(id);
            return RedirectToAction("Index");
        }

        [Route("/go/{shortCode}")]
        public async Task<IActionResult> RedirectShortUrl(string shortCode)
        {
            var ignore = new[] { "Home", "Url", "Authentication", "favicon.ico", "lib", "css", "js", "img" };
            if (ignore.Contains(shortCode)) return NotFound();

            var urlObj = await _urlsService.GetOriginalUrlAsync(shortCode);
            if (urlObj != null)
            {
                await _urlsService.IncrementNumberOfClicksAsync(urlObj.Id);
                return Redirect(urlObj.OriginalLink);
            }
            return NotFound();
        }
    }
}
