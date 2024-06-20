using CourseAppUI.Filters;
using CourseAppUI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace CourseAppUI.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class StudentController : Controller
    {
        private HttpClient _client;
        public StudentController()
        {
            _client = new HttpClient();
        }
        public async Task<IActionResult> Index(int page = 1, int size = 4)
        {
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);
            var queryString = new StringBuilder();
            queryString.Append("?page=").Append(Uri.EscapeDataString(page.ToString()));
            queryString.Append("&size=").Append(Uri.EscapeDataString(size.ToString()));

            // Append query string to base URL
            string requestUrl = "https://localhost:7274/api/students" + queryString;
            using (var response = await _client.GetAsync(requestUrl))
            {
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<PaginatedResponseResource<StudentListItemGetResponse>>(await response.Content.ReadAsStringAsync(), options);
                    if (data.TotalPages < page) return RedirectToAction("index", new { page = data.TotalPages });

                    return View(data);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("login", "account");
                }
                else
                {
                    return RedirectToAction("error", "home");
                }
            }
        }

        public async Task<IActionResult> Create()
        {
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            using (var response = await _client.GetAsync("https://localhost:7274/api/Groups"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var bodyStr = await response.Content.ReadAsStringAsync();
                    List<GroupListItemGetResource> data;

                    try
                    {
                        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                        var groups = JsonSerializer.Deserialize<List<GroupListItemGetResource>>(bodyStr, options);
                        ViewBag.Groups = groups;
                        return View();
                    }
                    catch (JsonException ex)
                    {
                        // JSON dönüşüm hatası
                        Console.WriteLine($"JsonException occurred: {ex.Message}");
                        throw; // Hatanın detaylı incelemesi için genellikle throw kullanılır.
                    }


                    ViewBag.Groups = data;
                    return View();
                }
                else
                {
                    return RedirectToAction("error", "home");
                }
            }
        }



        //[HttpPost]
        //public async Task<IActionResult> Create(StudentCreateResource createRequest)
        //{
        //    _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

        //    if (!ModelState.IsValid)
        //    {
        //        var groupResponse = await _client.GetAsync("https://localhost:7274/api/Groups");
        //        if (groupResponse.IsSuccessStatusCode)
        //        {
        //            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        //            var groups = JsonSerializer.Deserialize<List<Group>>(await groupResponse.Content.ReadAsStringAsync(), options);
        //            ViewBag.Groups = new SelectList(groups, "Id", "Name");
        //        }
        //        return View(createRequest);
        //    }

        //    var content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        //    using (HttpResponseMessage response = await _client.PostAsync("https://localhost:7274/api/Students", content))
        //    {
        //        if (response.IsSuccessStatusCode)
        //        {
        //            return RedirectToAction("Index");
        //        }
        //        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        //        {
        //            return RedirectToAction("Login", "Account");
        //        }
        //        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        //        {
        //            var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        //            ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);

        //            foreach (var item in errorResponse.Errors)
        //                ModelState.AddModelError(item.Key, item.Message);

        //            var groupResponse = await _client.GetAsync("https://localhost:7274/api/Groups");
        //            if (groupResponse.IsSuccessStatusCode)
        //            {
        //                var groups = JsonSerializer.Deserialize<List<Group>>(await groupResponse.Content.ReadAsStringAsync(), options);
        //                ViewBag.Groups = new SelectList(groups, "Id", "Name");
        //            }

        //            return View(createRequest);
        //        }
        //        else
        //        {
        //            TempData["Error"] = "Something went wrong!";
        //        }
        //    }

        //    return View(createRequest);
        //}

        public async Task<IActionResult> Delete(int id)
        {
            _client.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            using (var response = await _client.DeleteAsync("https://localhost:7274/api/Students/" + id))
            {
                if (response.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return Unauthorized();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound();
                }
                else
                {
                    return StatusCode(500);
                }
            }
        }

    }
}
