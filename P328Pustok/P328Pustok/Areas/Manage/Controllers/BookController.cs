using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using P328Pustok.DAL;
using P328Pustok.Helpers;
using P328Pustok.Models;
using P328Pustok.ViewModels;

namespace P328Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class BookController : Controller
    {
        private readonly PustokContext _context;
        private readonly IWebHostEnvironment _env;

        public BookController(PustokContext pustokContext, IWebHostEnvironment env)
        {
            _context = pustokContext;
            _env = env;
        }
        public IActionResult Index(int page=1, string search=null)
        {
            var query = _context.Books
                .Include(x => x.Author).Include(x => x.Genre).Include(x=>x.BookImages.Where(bi=>bi.PosterStatus==true)).AsQueryable();

            if (search != null)
                query = query.Where(x => x.Name.Contains(search));
            
            ViewBag.Search = search;

            return View(PaginatedList<Book>.Create(query,page,3));
        }

        public IActionResult Create()
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();


            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Create(Book book)
        {
            if (!ModelState.IsValid) return View();

            if(!_context.Authors.Any(x=>x.Id == book.AuthorId))
            {
                ModelState.AddModelError("AuthorId", "AuthorIs is not correct");
                return View();
            }

            if (!_context.Genres.Any(x => x.Id == book.GenreId))
            {
                ModelState.AddModelError("GenreId", "GenreId is not correct");
                return View();
            }


            if (book.PosterImage == null)
            {
                ModelState.AddModelError("PosterImage", "posterImage is required");
                return View();
            }
            if (book.HoverPosterImage == null)
            {
                ModelState.AddModelError("HoverPosterImage", "posterImage is required");
                return View();
            }

            BookImage poster = new BookImage
            {
                ImageName = FileManager.Save(_env.WebRootPath, "uploads/books", book.PosterImage),
                PosterStatus = true,
            };
            book.BookImages.Add(poster);

            BookImage hoverPoster = new BookImage
            {
                ImageName = FileManager.Save(_env.WebRootPath, "uploads/books", book.HoverPosterImage),
                PosterStatus = false,
            };
            book.BookImages.Add(hoverPoster);

            foreach (var img in book.Images)
            {
                BookImage bookImage = new BookImage
                {
                    ImageName = FileManager.Save(_env.WebRootPath, "uploads/books", img),
                };
                book.BookImages.Add(bookImage);
            }

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction("index");
        }

        public IActionResult Edit(int id)
        {
            ViewBag.Authors = _context.Authors.ToList();
            ViewBag.Genres = _context.Genres.ToList();

            Book book = _context.Books.FirstOrDefault(x => x.Id == id);
            return View(book);
        }

    }
}
