using Microsoft.EntityFrameworkCore;
using MovieTracker.Core.Models;
using MovieTracker.Data;

namespace MovieTracker.API.Services
{
    public interface IMovieSyncService
    {
        /// true, якщо фільм є в БД (або був підтягнутий з TMDB); false, якщо його немає на TMDB
        Task<bool> EnsureMovieAsync(int movieId);
    }

    public class MovieSyncService : IMovieSyncService
    {
        private readonly MovieTrackerContext _db;
        private readonly ITmdbService _tmdbService;

        public MovieSyncService(MovieTrackerContext db, ITmdbService tmdbService)
        {
            _db = db;
            _tmdbService = tmdbService;
        }

        public async Task<bool> EnsureMovieAsync(int movieId)
        {
            if (await _db.Movies.AnyAsync(m => m.Id == movieId)) return true;

            var movieDetail = await _tmdbService.GetMovieDetailsAsync(movieId);
            if (movieDetail == null) return false;

            var genreIds = movieDetail.Genres.Select(g => g.Id).ToList();
            var existingGenres = await _db.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            var genresList = new List<Genre>();

            foreach (var genre in movieDetail.Genres)
            {
                var existingGenre = existingGenres.FirstOrDefault(g => g.Id == genre.Id);
                if (existingGenre != null)
                {
                    genresList.Add(existingGenre);
                }
                else
                {
                    var newGenre = new Genre { Id = genre.Id, Name = genre.Name };
                    _db.Genres.Add(newGenre);
                    genresList.Add(newGenre);
                }
            }

            var personIds = movieDetail.MoviePersons.Select(mp => mp.PersonId).ToList();
            var existingPersons = await _db.Persons.Where(p => personIds.Contains(p.Id)).ToListAsync();
            var moviePersonsList = new List<MoviePerson>();

            foreach (var mpDto in movieDetail.MoviePersons)
            {
                var person = existingPersons.FirstOrDefault(p => p.Id == mpDto.PersonId);
                if (person == null)
                {
                    person = new Person
                    {
                        Id = mpDto.PersonId,
                        FullName = mpDto.FullName,
                        PhotoUrl = mpDto.PhotoUrl
                    };
                    _db.Persons.Add(person);
                    existingPersons.Add(person);
                }
                else
                {
                    person.FullName = mpDto.FullName;
                    person.PhotoUrl = mpDto.PhotoUrl;
                }

                moviePersonsList.Add(new MoviePerson
                {
                    MovieId = movieDetail.Id,
                    Person = person,
                    Role = mpDto.Role
                });
            }

            _db.Movies.Add(new Movie
            {
                Id = movieDetail.Id,
                Title = movieDetail.Title,
                Description = movieDetail.Description,
                ReleaseYear = movieDetail.ReleaseYear ?? 0,
                Rating = movieDetail.Rating,
                MediaType = movieDetail.MediaType,
                PosterUrl = movieDetail.PosterUrl,
                Genres = genresList,
                MoviePersons = moviePersonsList
            });

            await _db.SaveChangesAsync();
            return true;
        }
    }
}