﻿using CourseAppUI.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CourseAppUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _client;

        public AccountController()
        {
            _client = new HttpClient();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest, options), Encoding.UTF8, "application/json");

            using (var response = await _client.PostAsync("https://localhost:7274/api/Auth/login", content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(await response.Content.ReadAsStringAsync(), options);
                    Response.Cookies.Append("token", "Bearer " + loginResponse.Token);
                    return RedirectToAction("index", "group");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ModelState.AddModelError("", "Email or Password incorrect!");
                    return View();
                }
                else
                {
                    TempData["Error"] = "Something went wrong";
                }
            }

            return View();
        }
    }

}
