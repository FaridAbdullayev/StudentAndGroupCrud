﻿using CourseAppUI.Filters;
using CourseAppUI.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CourseAppUI.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class GroupController : Controller
    {

        private readonly HttpClient _httpClient;

        public GroupController()
        {
            _httpClient = new HttpClient();
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var response = await client.GetAsync("https://localhost:7274/api/Groups?page=" + page + "&size=2"))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var bodyStr = await response.Content.ReadAsStringAsync();

                        var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                        PaginatedResponseResource<GroupListItemGetResource> data = JsonSerializer.Deserialize<PaginatedResponseResource<GroupListItemGetResource>>(bodyStr, options);
                        return View(data);
                    }
                    else
                    {
                        return RedirectToAction("error", "home");
                    }
                }
            }
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(GroupCreateResource createRequest)
        {
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            if (!ModelState.IsValid) return View();

            var content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await _httpClient.PostAsync("https://localhost:7274/api/Groups", content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("login", "account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);

                    foreach (var item in errorResponse.Errors)
                        ModelState.AddModelError(item.Key, item.Message);

                    return View();
                }
                else
                {
                    TempData["Error"] = "Something went wrong!";
                }
            }

            return View(createRequest);
        }


        public async Task<IActionResult> Edit(int id)
        {
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            using (var response = await _httpClient.GetAsync("https://localhost:7274/api/Groups/" + id))
            {
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    GroupCreateResource request = JsonSerializer.Deserialize<GroupCreateResource>(await response.Content.ReadAsStringAsync(), options);
                    return View(request);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("login", "account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    TempData["Error"] = "Group not found";
                else
                    TempData["Error"] = "Something went wrong!";
            }
            return RedirectToAction("index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(GroupCreateResource editRequest, int id)
        {
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            if (!ModelState.IsValid) return View();

            var content = new StringContent(JsonSerializer.Serialize(editRequest), Encoding.UTF8, "application/json");
            using (HttpResponseMessage response = await _httpClient.PutAsync("https://localhost:7274/api/Groups/" + id, content))
            {
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("index");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("login", "account");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var options = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                    ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(await response.Content.ReadAsStringAsync(), options);

                    foreach (var item in errorResponse.Errors)
                        ModelState.AddModelError(item.Key, item.Message);

                    return View();
                }
                else
                {
                    TempData["Error"] = "Something went wrong!";
                }
            }

            return View(editRequest);
        }

        public async Task<IActionResult> Delete(int id)
        {
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, Request.Cookies["token"]);

            using (var response = await _httpClient.DeleteAsync("https://localhost:7274/api/Groups/" + id))
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
