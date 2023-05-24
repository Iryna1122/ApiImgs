using ApiImgs.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

[ApiController]
[Route("api/images")]
public class ImagesController : ControllerBase
{
    private static readonly object lockObject = new object();

    private readonly AppDbContext context;

    public ImagesController(AppDbContext db)
    {
        context = db;
    }

    [HttpPost("upload-by-url")]
    public IActionResult UploadByUrl([FromBody] ImageUrlRequest request)
    {
        lock (lockObject)
        {
            // Перевірка розміру файлу
            if (request.Url.Length > 5 * 1024 * 1024)
            {
                return BadRequest("Розмір файлу перевищує максимально допустимий розмір (5 МБ).");
            }

            // Завантаження зображення за URL
            byte[] imageBytes;
            try
            {
                using (var webClient = new WebClient())
                {
                    imageBytes = webClient.DownloadData(request.Url);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Не вдалося завантажити зображення з вказаного URL.");
            }

            // Генерація унікального імені файлу
            string fileName = Guid.NewGuid().ToString("N") + ".jpg";
            string filePath = Path.Combine("PATH_TO_IMAGE_FOLDER", fileName);

            // Збереження зображення на сервері
            try
            {
                System.IO.File.WriteAllBytes(filePath, imageBytes);
            }
            catch (Exception ex)
            {
                return BadRequest("Не вдалося зберегти зображення на сервері.");
            }

            // Створення URL для зображення
            string imageUrl = $"http://localhost/{fileName}";

            return Ok(new { Url = imageUrl });
        }
    }

    [HttpGet("get-url/{id}")]
    public IActionResult GetImageUrl(int id)
    {
        lock (lockObject)
        {
            var image = context.Images.FirstOrDefault(i => i.Id == id);
            if (image == null)
            {
                return NotFound("Зображення не знайдено.");
            }

            return Ok(new { Url = image.Path });
        }
    }

    [HttpGet("get-url/{id}/size/{size}")]
    public IActionResult GetImageThumbnailUrl(int id, int size)
    {
        lock (lockObject)
        {
            var image = context.Images.FirstOrDefault(i => i.Id == id);
            if (image == null)
            {
                return NotFound("Зображення не знайдено.");
            }

            string thumbnailUrl = GetThumbnailUrl(image.Path, size);
            return Ok(new { Url = thumbnailUrl });
        }
    }

    [HttpPost("remove/{id}")]
    public IActionResult RemoveImage(int id)
    {
        lock (lockObject)
        {
            var image = context.Images.FirstOrDefault(i => i.Id == id);
            if (image == null)
            {
                return NotFound("Зображення не знайдено.");
            }

            context.Images.Remove(image);
            context.SaveChanges();

            return Ok();
        }
    }

    private string GetThumbnailUrl(string imageUrl, int size)
    {
        // Логіка створення мініатюри
        // Реалізуйте обрізку зображень розмірами 100x100 та 300x300
        // і поверніть URL відповідної мініатюри
        // ...

        return ""; // Повернути URL мініатюри
    }
}

public class ImageUrlRequest
{
    public string Url { get; set; }
}

//public class AppDbContext
//{
//    // Код вашого контексту бази даних
//    // ...
//}
