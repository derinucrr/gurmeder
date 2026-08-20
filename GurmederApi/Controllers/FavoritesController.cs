using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GurmederApi.Data;
using GurmederApi.Services;

namespace GurmederApi.Controllers;

/// <summary>
/// Giriş yapmış kullanıcının favori tarifleri, sunucu tarafında saklanır
/// (bkz. Models/User.cs — FavoriteRecipeIds). Giriş yapılmadıysa ön yüz
/// hâlâ localStorage'a düşer (bkz. js/app.js) — bu uç noktalar yalnızca
/// oturum açıkken devreye girer.
/// </summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly GurmederDbContext _context;
    private readonly CurrentUserAccessor _currentUser;

    public FavoritesController(GurmederDbContext context, CurrentUserAccessor currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<int>>> GetAll()
    {
        var user = await _currentUser.GetUserAsync();
        if (user is null) return Unauthorized();
        return Ok(user.FavoriteRecipeIds);
    }

    [HttpPost("{recipeId:int}")]
    public async Task<ActionResult<List<int>>> Add(int recipeId)
    {
        var user = await _currentUser.GetUserAsync();
        if (user is null) return Unauthorized();

        if (!user.FavoriteRecipeIds.Contains(recipeId))
        {
            user.FavoriteRecipeIds.Add(recipeId);
            await _context.SaveChangesAsync();
        }
        return Ok(user.FavoriteRecipeIds);
    }

    [HttpDelete("{recipeId:int}")]
    public async Task<ActionResult<List<int>>> Remove(int recipeId)
    {
        var user = await _currentUser.GetUserAsync();
        if (user is null) return Unauthorized();

        if (user.FavoriteRecipeIds.Remove(recipeId))
        {
            await _context.SaveChangesAsync();
        }
        return Ok(user.FavoriteRecipeIds);
    }
}
