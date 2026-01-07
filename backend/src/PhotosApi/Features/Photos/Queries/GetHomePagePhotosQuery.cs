using MediatR;
using PhotosApi.Contracts;

namespace PhotosApi.Features.Photos.Queries;

public record GetHomePagePhotosQuery(Guid PhotoId) : IRequest<HomePagePhotoDto>;

// public class GetHomePagePhotosQueryHandler(
//     PhotosDbContext context,
//     IStorageRepository storage,
//     
//     )
