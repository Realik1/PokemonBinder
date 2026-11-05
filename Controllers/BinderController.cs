using Microsoft.AspNetCore.Mvc;
using PokemonBinder.Models;
using PokemonBinder.Services;
using System;
using System.IO;
using System.Threading.Tasks;


namespace PokemonBinder.Controllers
{
    public class BinderController : Controller
    {
        private readonly BinderService _binderService;
        private readonly CardService _cardService;

        public BinderController(BinderService binderService, CardService cardService)
        {
            _binderService = binderService;
            _cardService = cardService;
        }

        public async Task<IActionResult> Index()
        {
            var binders = await _binderService.GetAllBindersAsync();
            return View(binders);
        }

        public async Task<IActionResult> View(int id)
        {
            var binder = await _binderService.GetBinderByIdAsync(id);
            if (binder == null)
                return NotFound();

            // Serialize to JSON to see what's being sent
            var json = System.Text.Json.JsonSerializer.Serialize(binder.Pages);
            System.Diagnostics.Debug.WriteLine("Pages JSON: " + json);


            return View(binder);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(string name, string description, IFormFile frontCover, string coverColor)
        {
            try
            {
                byte[] frontCoverData = null;                

                if (frontCover != null && frontCover.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await frontCover.CopyToAsync(ms);
                        frontCoverData = ms.ToArray();
                    }
                }

                var binderId = await _binderService.CreateBinderAsync(name, description,
                    frontCoverData, coverColor);

                return RedirectToAction(nameof(View), new { id = binderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }
        
        public async Task<IActionResult> ViewPage(int pageId)
        {
            var page = await _binderService.GetPageAsync(pageId);
            if (page == null)
                return NotFound();

            return View(page);
        }

        [HttpPost]
        public async Task<IActionResult> AddPage(int binderId, int gridSize)
        {
            try
            {
                var gridSizeEnum = (GridSize)gridSize;
                var pageId = await _binderService.AddPageToBinderAsync(binderId, gridSizeEnum);
                return RedirectToAction(nameof(View), new { id = binderId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCardToSlot(int slotId, int cardId)
        {
            var result = await _binderService.AddCardToSlotAsync(slotId, cardId);
            return Json(new { success = result });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCardFromSlot(int slotId)
        {
            var result = await _binderService.RemoveCardFromSlotAsync(slotId);
            return Json(new { success = result });
        }
    }
}