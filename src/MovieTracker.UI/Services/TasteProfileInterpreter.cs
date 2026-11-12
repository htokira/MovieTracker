using MovieTracker.Core.Dtos;
using MovieTracker.UI.Models;

namespace MovieTracker.UI.Services;

public class TasteProfileInterpreter
{
    private static class GenreIds
    {
        public const int Action = 28;
        public const int Adventure = 12;
        public const int Animation = 16;
        public const int Comedy = 35;
        public const int Crime = 80;
        public const int Documentary = 99;
        public const int Drama = 18;
        public const int Family = 10751;
        public const int Fantasy = 14;
        public const int History = 36;
        public const int Horror = 27;
        public const int Music = 10402;
        public const int Mystery = 9648;
        public const int Romance = 10749;
        public const int ScienceFiction = 878;
        public const int Thriller = 53;
        public const int War = 10752;
        public const int Western = 37;
    }

    private class ArchetypeDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public HashSet<int> GenreIds { get; set; } = new();
    }

    private static readonly List<ArchetypeDefinition> Archetypes =
    [
        new ArchetypeDefinition
        {
            Name = "Майстер інтриг",
            Description = "Тебе захоплюють загадки, несподівані повороти та складні історії. Ти любиш фільми, які змушують будувати теорії до самого фіналу.",
            GenreIds =
            [
                GenreIds.Mystery,
                GenreIds.Thriller,
                GenreIds.Crime
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Мрійник",
            Description = "Ти любиш виходити за межі реальності. Фантастичні світи, неймовірні пригоди та незвичайні історії — саме те, що робить перегляд особливим.",
            GenreIds =
            [
                GenreIds.Fantasy,
                GenreIds.ScienceFiction,
                GenreIds.Adventure,
                GenreIds.Animation
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Шукач емоцій",
            Description = "Для тебе кіно — це передусім почуття та переживання. Ти цінуєш сильні історії, глибоких персонажів і романтичні сюжети.",
            GenreIds =
            [
                GenreIds.Drama,
                GenreIds.Romance,
                GenreIds.Music
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Адреналіновий глядач",
            Description = "Ти не любиш нудьгувати перед екраном. Динаміка, напружені протистояння та захопливі пригоди — твоя стихія.",
            GenreIds =
            [
                GenreIds.Action,
                GenreIds.Adventure,
                GenreIds.Thriller
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Поціновувач сміху",
            Description = "Ти знаєш, що хороший фільм здатен покращити будь-який день. Легкі сюжети, гумор і тепла атмосфера займають особливе місце у твоїй колекції.",
            GenreIds =
            [
                GenreIds.Comedy,
                GenreIds.Family,
                GenreIds.Animation
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Допитливий дослідник",
            Description = "Тобі цікаво відкривати нові сторони світу через кіно. Реальні події, історичні сюжети та незвичайні теми не залишають тебе байдужим.",
            GenreIds =
            [
                GenreIds.Documentary,
                GenreIds.History,
                GenreIds.War,
                GenreIds.Western
            ]
        },

        new ArchetypeDefinition
        {
            Name = "Шукач темряви",
            Description = "Ти не боїшся похмурих історій і сильних вражень. Атмосфера напруги, страху та невідомості робить перегляд для тебе особливо цікавим.",
            GenreIds =
            [
                GenreIds.Horror,
                GenreIds.Thriller,
                GenreIds.Mystery
            ]
        }
    ];

    public TasteProfileInterpretation Interpret(TasteProfileDto profile)
    {
        return new TasteProfileInterpretation
        {
            Archetype = DetermineArchetype(profile),
            ArchetypeDescription = DetermineArchetypeDescription(profile),

            Title = DetermineTitle(profile.ReviewsCount),
            TitleDescription = DetermineTitleDescription(profile.ReviewsCount),

            DiversityLabel = DetermineDiversityLabel(profile),
            DiversityDescription = DetermineDiversityDescription(profile)
        };
    }

    private string DetermineArchetype(TasteProfileDto profile)
    {
        var positivePreferences = profile.GenrePreferences
            .Where(g => g.PreferenceScore > 0.15 && g.Confidence >= 0.2)
            .ToList();

        if (positivePreferences.Count == 0)
            return "Кіноман-універсал";

        var scores = Archetypes.Select(archetype =>
        {
            var matchingGenres = positivePreferences
                .Where(g => archetype.GenreIds.Contains(g.GenreId))
                .ToList();

            return new
            {
                Archetype = archetype,
                Score = matchingGenres.Sum(g => g.PreferenceScore),
                MatchedGenres = matchingGenres.Count
            };
        })
        .OrderByDescending(x => x.Score)
        .ToList();

        var best = scores.First();

        if (best.Score < 0.3 || best.MatchedGenres == 0)
            return "Кіноман-універсал";

        return best.Archetype.Name;
    }

    private string DetermineArchetypeDescription(TasteProfileDto profile)
    {
        var archetypeName = DetermineArchetype(profile);

        return Archetypes.FirstOrDefault(a => a.Name == archetypeName)
            ?.Description
            ?? "Твій кінематографічний смак не обмежується одним напрямом. Ти відкритий до різних історій та жанрів.";
    }

    private string DetermineTitle(int reviewsCount)
    {
        return reviewsCount switch
        {
            0 => "Початківець",
            <= 4 => "Кінолюбитель",
            <= 14 => "Знавець кіно",
            <= 29 => "Кінокритик",
            <= 49 => "Майстер кіно",
            _ => "Грандмайстер кіно"
        };
    }

    private string DetermineTitleDescription(int reviewsCount)
    {
        return reviewsCount switch
        {
            0 => "Твоя кінематографічна історія тільки починається. Оцінюй фільми, щоб відкрити свій смаковий профіль.",
            <= 4 => "Ти вже почав формувати власну кіноколекцію вражень. Попереду ще багато цікавих відкриттів.",
            <= 14 => "Ти вже маєш власні кінематографічні вподобання та поступово відкриваєш нові історії.",
            <= 29 => "Твій досвід дозволяє впевнено орієнтуватися у світі кіно. Ти вже знаєш, що саме тобі подобається.",
            <= 49 => "Ти переглянув чимало фільмів і сформував досить виразний кінематографічний смак.",
            _ => "Твоя кіноколекція вражень заслуговує на повагу. Ти справжній знавець кінематографічного світу!"
        };
    }

    private string DetermineDiversityLabel(TasteProfileDto profile)
    {
        if (profile.ReviewsCount == 0 ||
            profile.GenrePreferences.Count == 0)
        {
            return "Ще не визначено";
        }

        return profile.TasteDiversity switch
        {
            < 0.3 => "Вузький фокус",
            < 0.6 => "Збалансований смак",
            < 0.8 => "Відкритий до нового",
            _ => "Вічний експериментатор"
        };
    }

    private string DetermineDiversityDescription(TasteProfileDto profile)
    {
        if (profile.ReviewsCount == 0 ||
            profile.GenrePreferences.Count == 0)
        {
            return "Поки що недостатньо даних, щоб визначити різноманіття твого смаку. Оцінюй фільми, і ми поступово сформуємо твій профіль.";
        }

        return profile.TasteDiversity switch
        {
            < 0.3 => "Ти маєш чіткі жанрові вподобання та найчастіше обираєш знайомі й перевірені напрямки. Якість для тебе важливіша за різноманіття.",
            < 0.6 => "Твій смак поєднує кілька улюблених напрямків. Ти маєш свої вподобання, але водночас не проти спробувати щось інше.",
            < 0.8 => "Ти відкритий до різних кінематографічних жанрів. Твоя колекція не обмежується одним напрямом, а кожен новий фільм може стати відкриттям.",
            _ => "Ти справжній кінематографічний експериментатор! Твій інтерес охоплює безліч жанрів, і ти не боїшся виходити за межі звичних уподобань."
        };
    }
}