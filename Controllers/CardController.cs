using Microsoft.AspNetCore.Mvc;
using PokemonBinder.Models;
using PokemonBinder.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PokemonBinder.Controllers
{
    public class CardController : Controller
    {
        private readonly CardService _cardService;

        public CardController(CardService cardService)
        {
            _cardService = cardService;
        }

        public async Task<IActionResult> Index()
        {
            var cards = await _cardService.GetAllCardsAsync();
            return View(cards);
        }

        public async Task<IActionResult> Details(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
                return NotFound();

            return View(card);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Card card, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(card);
            }

            try
            {
                int cardId = await _cardService.AddCardAsync(card);

                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        var imageData = memoryStream.ToArray();
                        var image = await _cardService.AddCardImageAsync(imageData, imageFile.FileName);
                        await _cardService.AssignImageToCardAsync(cardId, image.Id);
                    }
                }

                return RedirectToAction(nameof(Details), new { id = cardId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(card);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
                return NotFound();

            return View(card);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Card card)
        {
            if (id != card.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(card);

            var updated = await _cardService.UpdateCardAsync(card);
            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Details), new { id = id });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
                return NotFound();

            return View(card);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _cardService.DeleteCardAsync(id);
            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string name, string packName)
        {
            var cards = await _cardService.SearchCardsAsync(name, packName);
            return View(nameof(Index), cards);
        }
    }
}
