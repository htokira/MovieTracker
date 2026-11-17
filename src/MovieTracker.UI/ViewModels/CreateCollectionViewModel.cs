using System.Windows.Input;
using MovieTracker.UI.Services;

namespace MovieTracker.UI.ViewModels;

public class CreateCollectionViewModel : BaseViewModel
{
    private readonly ITrackingService _tracking;

    private string _name = string.Empty;
    private string? _description;
    private bool _isPublic = true;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string? Description { get => _description; set => SetProperty(ref _description, value); }

    public bool IsPublic
    {
        get => _isPublic;
        set
        {
            if (SetProperty(ref _isPublic, value))
                OnPropertyChanged(nameof(PrivacyHint));
        }
    }

    public string PrivacyHint => IsPublic
        ? "Колекцію бачитимуть усі користувачі."
        : "Колекцію бачитимете лише ви.";

    public ICommand CreateCommand { get; }
    public ICommand GoBackCommand { get; }

    public CreateCollectionViewModel(ITrackingService tracking)
    {
        _tracking = tracking;

        CreateCommand = new Command(async () => await CreateAsync());
        GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
    }

    private async Task CreateAsync()
    {
        if (IsBusy) return;
        ErrorMessage = null;

        var name = Name.Trim();
        if (name.Length is < 1 or > 100)
        {
            ErrorMessage = "Назва має містити від 1 до 100 символів.";
            return;
        }

        var description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

        IsBusy = true;
        try
        {
            var error = await _tracking.CreateCollectionAsync(name, description, IsPublic);
            if (error is not null)
            {
                ErrorMessage = error;
                return;
            }

            await Shell.Current.DisplayAlertAsync("Готово", "Колекцію створено.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
