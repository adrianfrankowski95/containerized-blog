namespace Blog.Services.Blogging.API.Application.Commands.Models;

public record RestaurantReviewPostTranslationDTO(
    string Language,
    string Title,
    string Content,
    string Description,
    IList<Guid> TagIds,

    IList<string> RestaurantCuisines,
    string RestaurantAddressCountry,
    string RestaurantAddressZipCode,
    string RestaurantAddressCity,
    string RestaurantAddressStreet);