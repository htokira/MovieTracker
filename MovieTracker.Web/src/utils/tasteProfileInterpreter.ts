import type {
  TasteProfileDto,
  TasteProfileInterpretation,
} from "@/types/tasteProfile";

const GenreIds = {
  Action: 28,
  Adventure: 12,
  Animation: 16,
  Comedy: 35,
  Crime: 80,
  Documentary: 99,
  Drama: 18,
  Family: 10751,
  Fantasy: 14,
  History: 36,
  Horror: 27,
  Music: 10402,
  Mystery: 9648,
  Romance: 10749,
  ScienceFiction: 878,
  Thriller: 53,
  War: 10752,
  Western: 37,
} as const;

interface ArchetypeDefinition {
  name: string;
  description: string;
  genreIds: number[];
}

const archetypes: ArchetypeDefinition[] = [
  {
    name: "Майстер інтриг",
    description:
      "Тебе захоплюють загадки, несподівані повороти та складні історії. Ти любиш фільми, які змушують будувати теорії до самого фіналу.",
    genreIds: [GenreIds.Mystery, GenreIds.Thriller, GenreIds.Crime],
  },
  {
    name: "Мрійник",
    description:
      "Ти любиш виходити за межі реальності. Фантастичні світи, неймовірні пригоди та незвичайні історії — саме те, що робить перегляд особливим.",
    genreIds: [
      GenreIds.Fantasy,
      GenreIds.ScienceFiction,
      GenreIds.Adventure,
      GenreIds.Animation,
    ],
  },
  {
    name: "Шукач емоцій",
    description:
      "Для тебе кіно — це передусім почуття та переживання. Ти цінуєш сильні історії, глибоких персонажів і романтичні сюжети.",
    genreIds: [GenreIds.Drama, GenreIds.Romance, GenreIds.Music],
  },
  {
    name: "Адреналіновий глядач",
    description:
      "Ти не любиш нудьгувати перед екраном. Динаміка, напружені протистояння та захопливі пригоди — твоя стихія.",
    genreIds: [GenreIds.Action, GenreIds.Adventure, GenreIds.Thriller],
  },
  {
    name: "Поціновувач сміху",
    description:
      "Ти знаєш, що хороший фільм здатен покращити будь-який день. Легкі сюжети, гумор і тепла атмосфера займають особливе місце у твоїй колекції.",
    genreIds: [GenreIds.Comedy, GenreIds.Family, GenreIds.Animation],
  },
  {
    name: "Допитливий дослідник",
    description:
      "Тобі цікаво відкривати нові сторони світу через кіно. Реальні події, історичні сюжети та незвичайні теми не залишають тебе байдужим.",
    genreIds: [
      GenreIds.Documentary,
      GenreIds.History,
      GenreIds.War,
      GenreIds.Western,
    ],
  },
  {
    name: "Шукач темряви",
    description:
      "Ти не боїшся похмурих історій і сильних вражень. Атмосфера напруги, страху та невідомості робить перегляд для тебе особливо цікавим.",
    genreIds: [GenreIds.Horror, GenreIds.Thriller, GenreIds.Mystery],
  },
];

const universalArchetype: ArchetypeDefinition = {
  name: "Кіноман-універсал",
  description:
    "Твій кінематографічний смак не обмежується одним напрямом. Ти відкритий до різних історій та жанрів.",
  genreIds: [],
};

function determineArchetype(profile: TasteProfileDto): ArchetypeDefinition {
  const positivePreferences = profile.genrePreferences.filter(
    (g) => g.preferenceScore > 0.15 && g.confidence >= 0.2
  );

  if (positivePreferences.length === 0) {
    return universalArchetype;
  }

  let best = universalArchetype;
  let bestScore = -Infinity;
  let bestMatched = 0;

  for (const archetype of archetypes) {
    const matchingGenres = positivePreferences.filter((g) =>
      archetype.genreIds.includes(g.genreId)
    );
    const score = matchingGenres.reduce((sum, g) => sum + g.preferenceScore, 0);

    if (score > bestScore) {
      best = archetype;
      bestScore = score;
      bestMatched = matchingGenres.length;
    }
  }

  if (bestScore < 0.3 || bestMatched === 0) {
    return universalArchetype;
  }

  return best;
}

function determineTitle(reviewsCount: number): string {
  if (reviewsCount === 0) return "Початківець";
  if (reviewsCount <= 4) return "Кінолюбитель";
  if (reviewsCount <= 14) return "Знавець кіно";
  if (reviewsCount <= 29) return "Кінокритик";
  if (reviewsCount <= 49) return "Майстер кіно";
  return "Грандмайстер кіно";
}

function determineTitleDescription(reviewsCount: number): string {
  if (reviewsCount === 0) {
    return "Твоя кінематографічна історія тільки починається. Оцінюй фільми, щоб відкрити свій смаковий профіль.";
  }
  if (reviewsCount <= 4) {
    return "Ти вже почав формувати власну кіноколекцію вражень. Попереду ще багато цікавих відкриттів.";
  }
  if (reviewsCount <= 14) {
    return "Ти вже маєш власні кінематографічні вподобання та поступово відкриваєш нові історії.";
  }
  if (reviewsCount <= 29) {
    return "Твій досвід дозволяє впевнено орієнтуватися у світі кіно. Ти вже знаєш, що саме тобі подобається.";
  }
  if (reviewsCount <= 49) {
    return "Ти переглянув чимало фільмів і сформував досить виразний кінематографічний смак.";
  }
  return "Твоя кіноколекція вражень заслуговує на повагу. Ти справжній знавець кінематографічного світу!";
}

function hasNoDiversityData(profile: TasteProfileDto): boolean {
  return profile.reviewsCount === 0 || profile.genrePreferences.length === 0;
}

function determineDiversityLabel(profile: TasteProfileDto): string {
  if (hasNoDiversityData(profile)) {
    return "Ще не визначено";
  }

  const diversity = profile.tasteDiversity;

  if (diversity < 0.3) return "Вузький фокус";
  if (diversity < 0.6) return "Збалансований смак";
  if (diversity < 0.8) return "Відкритий до нового";
  return "Вічний експериментатор";
}

function determineDiversityDescription(profile: TasteProfileDto): string {
  if (hasNoDiversityData(profile)) {
    return "Поки що недостатньо даних, щоб визначити різноманіття твого смаку. Оцінюй фільми, і ми поступово сформуємо твій профіль.";
  }

  const diversity = profile.tasteDiversity;

  if (diversity < 0.3) {
    return "Ти маєш чіткі жанрові вподобання та найчастіше обираєш знайомі й перевірені напрямки. Якість для тебе важливіша за різноманіття.";
  }
  if (diversity < 0.6) {
    return "Твій смак поєднує кілька улюблених напрямків. Ти маєш свої вподобання, але водночас не проти спробувати щось інше.";
  }
  if (diversity < 0.8) {
    return "Ти відкритий до різних кінематографічних жанрів. Твоя колекція не обмежується одним напрямом, а кожен новий фільм може стати відкриттям.";
  }
  return "Ти справжній кінематографічний експериментатор! Твій інтерес охоплює безліч жанрів, і ти не боїшся виходити за межі звичних уподобань.";
}

export function interpretTasteProfile(
  profile: TasteProfileDto
): TasteProfileInterpretation {
  const archetype = determineArchetype(profile);

  return {
    archetype: archetype.name,
    archetypeDescription: archetype.description,

    title: determineTitle(profile.reviewsCount),
    titleDescription: determineTitleDescription(profile.reviewsCount),

    diversityLabel: determineDiversityLabel(profile),
    diversityDescription: determineDiversityDescription(profile),
  };
}