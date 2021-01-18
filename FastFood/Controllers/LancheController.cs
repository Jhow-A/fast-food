﻿using FastFood.Data.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FastFood.Controllers
{
    public class LancheController : Controller
    {
        private readonly ILancheRepository _lancheRepository;
        private readonly ICategoriaRepository _categoriaRepository;

        public LancheController(ILancheRepository lancheRepository, ICategoriaRepository categoriaRepository)
        {
            _lancheRepository = lancheRepository;
            _categoriaRepository = categoriaRepository;
        }

        public IActionResult List()
        {
            ViewBag.Lanche = "Lanches";
            ViewData["Lanche"] = "Lanches";

            var lanches = _lancheRepository.Lanches;
            return View(lanches);
        }
    }
}
