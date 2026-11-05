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

        public async Task<IActionResult> Index(string searchName, string searchPack, string searchArtist)
        {
            var cards = await _cardService.SearchCardsAsync(searchName, searchPack);

            // Optional secondary filter in-memory for artist if needed, or expand your repository search
            if (!string.IsNullOrWhiteSpace(searchArtist))
            {
                cards = cards.Where(c => c.Artist != null && c.Artist.Contains(searchArtist, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewData["SearchName"] = searchName;
            ViewData["SearchPack"] = searchPack;
            ViewData["SearchArtist"] = searchArtist;

            return View(cards);
        }

        public async Task<IActionResult> Details(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
                return NotFound();

            return View(card);
        }

        [HttpPost]
        public async Task<IActionResult> ImportCsv(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid CSV file.");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Pass the IFormFile directly
                await _cardService.ImportCardsFromCsvAsync(csvFile);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error importing CSV: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(string uniqueId, string name, string? artist,
    string? packName, int pokedexNo, string? type, IFormCollection form, IFormFile? imageFile)
        {
            try
            {
                var card = new Card
                {
                    UniqueID = uniqueId,
                    Name = name,
                    Artist = artist,
                    PackName = packName,
                    PokedexNo = pokedexNo,
                    Type = type
                };

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
                return View();
            }
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null)
                return NotFound();

            return View(card);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Card card, IFormFile? imageFile)
        {
            if (id != card.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(card);

            try
            {
                // If a new image is uploaded during edit, save and update the image reference
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        var imageData = memoryStream.ToArray();
                        var image = await _cardService.AddCardImageAsync(imageData, imageFile.FileName);
                        card.CardImageId = image.Id;
                    }
                }

                var updated = await _cardService.UpdateCardAsync(card);
                if (!updated)
                    return NotFound();

                return RedirectToAction(nameof(Index), new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(card);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCards()
        {
            var cards = await _cardService.GetAllCardsAsync();
            return Json(cards);
        }

        [HttpGet]
        public async Task<IActionResult> GetImage(int id)
        {
            var card = await _cardService.GetCardByIdAsync(id);
            if (card == null || card.CardImageId == 0)
            {
                // Return a placeholder image
                return BadRequest();
            }

            var image = await _cardService.GetCardImageAsync(card.CardImageId);
            if (image == null)
                return NotFound();

            return File(image.ImageData, image.MimeType ?? "image/png");
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
