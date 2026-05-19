namespace Readier.Helpers;

public record SurveyOption(string Label, int Minutes);

public record SurveyQuestion(string Key, string Title, IReadOnlyList<SurveyOption> Options);

public static class PreparationSurvey
{
    // 인적사항
    public const string GenderKey = "gender";
    public const string AgeRangeKey = "ageRange";

    // 행동
    public const string ShowerKey = "shower";
    public const string HairWashKey = "hairWash";
    public const string SkincareKey = "skincare";
    public const string MakeupKey = "makeup";
    public const string HairStylingKey = "hairStyling";
    public const string OutfitKey = "outfit";
    public const string BreakfastKey = "breakfast";
    public const string MorningStartKey = "morningStart";
    public const string PaceKey = "pace";

    public static IReadOnlyList<SurveyQuestion> All { get; } = new[]
    {
        new SurveyQuestion(GenderKey, "성별", new[]
        {
            new SurveyOption("답하지 않음", 0),
            new SurveyOption("남자", 0),
            new SurveyOption("여자", 0)
        }),
        new SurveyQuestion(AgeRangeKey, "나이대", new[]
        {
            new SurveyOption("답하지 않음", 0),
            new SurveyOption("10대 이하", 0),
            new SurveyOption("20대", 0),
            new SurveyOption("30대", 0),
            new SurveyOption("40대", 0),
            new SurveyOption("50대 이상", 0)
        }),
        new SurveyQuestion(ShowerKey, "평소 아침 샤워", new[]
        {
            new SurveyOption("안 함", 0),
            new SurveyOption("5분 이내 빠르게", 5),
            new SurveyOption("10분 정도 보통", 10),
            new SurveyOption("15-20분 충분히", 18),
            new SurveyOption("20분 이상 길게", 25)
        }),
        new SurveyQuestion(HairWashKey, "머리 감기 + 말리기", new[]
        {
            new SurveyOption("안 함", 0),
            new SurveyOption("5분 이내", 4),
            new SurveyOption("5-10분", 8),
            new SurveyOption("10-15분", 12),
            new SurveyOption("15분 이상", 18)
        }),
        new SurveyQuestion(SkincareKey, "스킨케어", new[]
        {
            new SurveyOption("안 함", 0),
            new SurveyOption("1-2단계 (3분 이내)", 3),
            new SurveyOption("3-4단계 (5-10분)", 8),
            new SurveyOption("5단계 이상 (10-15분)", 13),
            new SurveyOption("풀 루틴 (15분 이상)", 20)
        }),
        new SurveyQuestion(MakeupKey, "메이크업", new[]
        {
            new SurveyOption("안 함", 0),
            new SurveyOption("베이스만 (5-10분)", 8),
            new SurveyOption("데일리 (15-20분)", 18),
            new SurveyOption("풀 메이크업 (30-40분)", 35),
            new SurveyOption("컨셉/포인트 (40분 이상)", 50)
        }),
        new SurveyQuestion(HairStylingKey, "헤어 스타일링", new[]
        {
            new SurveyOption("자연 건조/방치", 0),
            new SurveyOption("빗질만", 2),
            new SurveyOption("드라이만", 5),
            new SurveyOption("드라이 + 가벼운 스타일링", 12),
            new SurveyOption("본격 스타일링", 20)
        }),
        new SurveyQuestion(OutfitKey, "옷 결정", new[]
        {
            new SurveyOption("전날 미리 정해둠", 1),
            new SurveyOption("아침에 빠르게 (5분)", 5),
            new SurveyOption("평소대로 시간 들여 (10분)", 10),
            new SurveyOption("자주 갈아입거나 한참 고민 (15분 이상)", 18)
        }),
        new SurveyQuestion(BreakfastKey, "아침 식사", new[]
        {
            new SurveyOption("안 먹음", 0),
            new SurveyOption("음료/간식만 (5분)", 5),
            new SurveyOption("간단한 식사 (15분)", 15),
            new SurveyOption("정찬 (25분 이상)", 25)
        }),
        new SurveyQuestion(MorningStartKey, "일어나서 첫 행동까지", new[]
        {
            new SurveyOption("알람 즉시 일어남", 1),
            new SurveyOption("5분 이내", 5),
            new SurveyOption("10분 정도", 10),
            new SurveyOption("15분 이상", 15)
        }),
        new SurveyQuestion(PaceKey, "본인이 보는 아침 페이스", new[]
        {
            new SurveyOption("빠른 편", -3),
            new SurveyOption("평균", 0),
            new SurveyOption("느긋한 편", 5)
        })
    };

    public static SurveyQuestion ByKey(string key)
        => All.First(q => q.Key == key);

    public static int TotalMinutes(IReadOnlyDictionary<string, int> answers)
    {
        var total = 0;
        foreach (var question in All)
        {
            if (!answers.TryGetValue(question.Key, out var idx)) continue;
            if (idx < 0 || idx >= question.Options.Count) continue;
            total += question.Options[idx].Minutes;
        }
        return Math.Max(0, total);
    }
}
