namespace Readier.Models;

public enum LiveActivityPhase
{
    PrepStartingSoon,
    LeaveStartingSoon,
    InTransit,
    Completed
}

public sealed record LiveActivitySnapshot(
    LiveActivityPhase Phase,
    string Title,
    string Subtitle,
    DateTime TargetTime,
    bool ContainsSensitiveLocation);

public static class LiveActivityUxSpec
{
    public static IReadOnlyList<LiveActivitySnapshot> ExampleStates { get; } = new[]
    {
        new LiveActivitySnapshot(
            LiveActivityPhase.PrepStartingSoon,
            "준비 시작이 곧 다가와요",
            "가볍게 몸을 풀고 준비를 시작해요",
            DateTime.MinValue,
            false),
        new LiveActivitySnapshot(
            LiveActivityPhase.LeaveStartingSoon,
            "출발 시간이 가까워요",
            "지금 출발하면 여유 있게 도착할 수 있어요",
            DateTime.MinValue,
            false),
        new LiveActivitySnapshot(
            LiveActivityPhase.InTransit,
            "이동 중",
            "안전하게 이동하세요",
            DateTime.MinValue,
            false),
        new LiveActivitySnapshot(
            LiveActivityPhase.Completed,
            "일정 완료",
            "좋아요, 오늘 일정 하나를 마쳤어요",
            DateTime.MinValue,
            false)
    };
}
