using System.Collections.ObjectModel;
using System.Windows.Input;
using MovieTracker.Core.Dtos;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels
{
    public class CollectionViewModel : BaseViewModel
    {
        private readonly ApiService _api;

        private string _name = string.Empty;
        private string? _description;
        private string _ownerName = string.Empty;

        public string Name { get => _name; private set => SetProperty(ref _name, value); }
        public string OwnerName { get => _ownerName; private set => SetProperty(ref _ownerName, value); }

        public string? Description
        {
            get => _description;
            private set
            {
                if (SetProperty(ref _description, value))
                    OnPropertyChanged(nameof(HasDescription));
            }
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public ObservableCollection<CollectionMovieResponse> Movies { get; } = new();

        public ICommand GoBackCommand { get; }

        public CollectionViewModel(ApiService api)
        {
            _api = api;
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
        }

        public async Task LoadAsync(Guid collectionId)
        {
            if (IsBusy) return;
            ErrorMessage = null;
            IsBusy = true;

            try
            {
                var collection = await _api.GetCollectionAsync(collectionId);
                if (collection is null)
                {
                    ErrorMessage = "Колекцію не знайдено або сервер недоступний.";
                    return;
                }

                Name = collection.Name;
                Description = collection.Description;
                OwnerName = $"Автор: {collection.OwnerName}";

                Movies.Clear();
                foreach (var movie in collection.Movies)
                    Movies.Add(movie);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}