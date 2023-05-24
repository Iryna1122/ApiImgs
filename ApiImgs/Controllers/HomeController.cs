﻿using ApiImgs.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;


namespace ApiImgs.Controllers
{
    //[ApiController]
    //[Route("api/images")]


    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static readonly object lockObject = new object();

        AppDbContext context;

        public List<Models.Image> Images;
        public Models.Image img;

        public HomeController(AppDbContext db)
        {
            this.context = db;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.imgs = context.Images.ToList();

            var images = context.Images.ToList();
            return View(images);
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



        [HttpPost]
        public async Task<ActionResult> AddImg()
        {
            var files = Request.Form.Files;

            for (int i = 0; i < files.Count; i++)
            {
                try
                {
                    var image = new Models.Image();

                    var uploadPath = $@"{Directory.GetCurrentDirectory()}\wwwroot\Photos";

                    Directory.CreateDirectory(uploadPath);

                    image.Path = $@"{uploadPath}\{files[i].FileName}";

                    using (var fs = new FileStream(image.Path, FileMode.Create))
                    {
                        await files[i].CopyToAsync(fs);
                    }

                    image.Path = image.Path.Split("wwwroot")[1];



                    await context.Images.AddAsync(image);
                    await context.SaveChangesAsync();

                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc.Message);
                }
            }

            return Redirect("Index");

        }
        //[HttpPost]
        ////public IActionResult AddImg([FromBody] ImageUrlRequest request)
        //public async Task<ActionResult> AddImg([FromBody] ImageUrlRequest request)
        //{
        //    //lock (lockObject)
        //    //{
        //        // Перевірка розміру файлу
        //        if (request.Url.Length > 5 * 1024 * 1024)
        //        {
        //            return BadRequest("Розмір файлу перевищує максимально допустимий розмір (5 МБ).");
        //        }

        //        try
        //        {
        //            var image = new Models.Image();

        //            var uploadPath = $@"{Directory.GetCurrentDirectory()}\wwwroot\Photos";

        //            Directory.CreateDirectory(uploadPath);

        //            image.Path = $@"{uploadPath}\{Guid.NewGuid().ToString("N")}.jpg";

        //            using (var fs = new FileStream(image.Path, FileMode.Create))
        //            {
        //                // Читання даних з потоку запиту
        //                using (var reader = new StreamReader(Request.Body))
        //                {
        //                    var body = reader.ReadToEnd();

        //                    // Розпізнавання даних JSON з тіла запиту
        //                    var requestData = JsonConvert.DeserializeObject<ImageUrlRequest>(body);
        //                    var imageUrl = requestData.Url;

        //                    // Завантаження зображення по URL
        //                    using (var webClient = new WebClient())
        //                    {
        //                        var imageBytes = webClient.DownloadData(imageUrl);
        //                        fs.Write(imageBytes, 0, imageBytes.Length);
        //                    }
        //                }
        //            }

        //            image.Path = image.Path.Split("wwwroot")[1];

        //            context.Images.AddAsync(image);
        //            context.SaveChangesAsync();

        //            return RedirectToAction("Index");
        //        }
        //        catch (Exception exc)
        //        {
        //            return BadRequest("Не вдалося зберегти зображення на сервері.");
        //        }
        //    //}
        //}



        [HttpGet]
        public async Task<IActionResult> ImageInfo(int id)
        {
            Models.Image img = await context.Images.FindAsync(id);

            if (img != null)
            {
                ViewBag.photosInfo = context.Images.ToList();

                return View(img);
            }
            return View("ImageInfo");
        }


        public static async Task DeleteFolder(string imagePath)
        {
            //var uploadPath = $@"{Directory.GetCurrentDirectory()}\wwwroot\Photos{imagePath}";

            var uploadPath = $@"{Directory.GetCurrentDirectory()}\wwwroot\Photos\{imagePath}";
            if (System.IO.File.Exists(uploadPath))
            {
                System.IO.File.Delete(uploadPath);
            }
        }


        [HttpPost]
        public async Task<ActionResult> ImageDelete(int id)
        {
            Models.Image image = await context.Images.FindAsync(id);
            if (image != null)
            {
                context.Images.Remove(image);
                await HomeController.DeleteFolder(image.Path);
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Index");

        }



        [HttpGet]
        public ActionResult ImageEdit(int id)
        {
            Models.Image image = context.Images.Where(o => o.Id == id).FirstOrDefault();

            return View(image);
        }



        [HttpPost]
        public async Task<IActionResult> ImageEdit2(int id)
        {
            var image = await context.Images.FindAsync(id);
            if (image != null)
            {
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    try
                    {
                        var uploadPath = $@"{Directory.GetCurrentDirectory()}\wwwroot\Photos";

                        Directory.CreateDirectory(uploadPath);

                        var newImagePath = $@"{uploadPath}\{files[0].FileName}";

                        using (var fs = new FileStream(newImagePath, FileMode.Create))
                        {
                            await files[0].CopyToAsync(fs);
                        }

              
                        if (System.IO.File.Exists($@"{Directory.GetCurrentDirectory()}\wwwroot\Photos{image.Path}"))
                        {
                            System.IO.File.Delete($@"{Directory.GetCurrentDirectory()}\wwwroot\Photos{image.Path}");
                        }

                        image.Path = newImagePath.Split("wwwroot")[1];
                        context.Images.Update(image);
                        await context.SaveChangesAsync();
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}