namespace Questlog.Application.Users;

public interface IFavoriteService
{
    /// <summary>A user's pinned favourite games, in showcase order.</summary>
    Task<IReadOnlyList<FavoriteGameDto>> GetFavoritesAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Replace the current user's favourites with the given games (max 4, in order).</summary>
    Task<IReadOnlyList<FavoriteGameDto>> SetOwnFavoritesAsync(SetFavoritesRequest request, CancellationToken ct = default);
}
