﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Venjix.Controllers
{
    public class TelegramController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}