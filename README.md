# Readier

Readier는 **늦지 않기 위한 출발/준비 시간 추천 웹 앱**입니다.  
일정의 시작 시각, 출발지/목적지, 이동 시간, 준비 시간을 바탕으로 사용자가 언제 준비를 시작하고 언제 출발해야 하는지 계산해 보여줍니다.

## 핵심 기능

- 일정 생성/수정/삭제
- 출발지/목적지 기반 이동 시간 계산(Kakao API 연동)
- 준비 시작 시각 / 출발 시각 자동 계산
- 설정 탭의 알림 옵션(부담 없는 문구, 추가 리마인더, 미리보기 알림)
- 로컬 우선 저장 + 로그인 시 사용자 범위 저장(DB 연결 시)

## 기술 스택

- .NET 9 / ASP.NET Core (Razor Components, Interactive Server)
- C#
- Entity Framework Core (SQL Server)
- JavaScript Browser Notification API

## 로컬 실행

```bash
dotnet restore Readier.sln
dotnet build Readier.sln
dotnet run --project Readier/Readier.csproj
```

앱에서 DB 저장을 사용하려면 아래 중 하나로 연결 문자열을 설정하면 됩니다.

- `READIER_DB_CONNECTION` (권장)
- `ConnectionStrings__ReadierDb`
- Azure App Service Connection String 이름 `readierdb` (`SQLAZURECONNSTR_readierdb`/`SQLCONNSTR_readierdb`)

  
Kakao/Google 연동 기능은 관련 API 키/시크릿 환경 변수 설정이 필요합니다.