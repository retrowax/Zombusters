# Localization Migration Notes

## Original Legacy (legacy/ZombustersWindows)

### Source files
- `Localization/Strings.resx` — EN base (auto-generated from Visual Studio ResourceManager)
- `Localization/Strings.de.resx` — German
- `Localization/Strings.es.resx` — Spanish
- `Localization/Strings.fr.resx` — French
- `Localization/Strings.it.resx` — Italian

**Note:** No Russian or Chinese in the original. ZH and RU are KMP-port additions.

### Legacy string categories
| Category | Keys | Status in KMP |
|---|---|---|
| In-game HUD | Paused, GameOverString, P1-P4PressStartString | Ported |
| Menu items | BackString, QuitGame, ResumeGame, SelectString, SinglePlayer | Ported |
| Options | MusicVolumeString, SoundEffectVolumeString, SaveAndExitString, SettingsMenuString | Ported |
| Character select | CharacterSelectString, ChangeCharacterMenuString, SwitchReadyMenuString | Ported |
| Extras | CreditsMenuString, ExtrasMenuString, AchievementsMenuString, HowToPlayInGameString | Ported |
| Xbox LIVE / matchmaking | CreateGameMenuString, QuickMatchMenuString, FindSessionMenuString, etc. | **OBSOLETE — not ported** |
| Online session | NetworkBusy, NoSessionsFound, WaitingForPlayersMenuString, LobbyMenuString, etc. | **OBSOLETE — not ported** |
| Achievement | AchievementsMenuString, AchievementsMMString | Ported (as extrasLeaderboard) |
| Trial / purchase | String1 (Would you like to purchase this game?) | **OBSOLETE — not ported** |
| Xbox Dashboard | ReturnToDashboardString, PlayerSignInString | **OBSOLETE — not ported** |

Approximately 40% of legacy strings are Xbox LIVE / matchmaking related and have no equivalent in the offline KMP port.

---

## KMP Port Implementation

### Architecture
- `localization/Localization.kt` — `interface Localization` with all production strings
- `localization/translations/EnglishLocalization.kt` — EN (authoritative)
- `localization/translations/DeutschLocalization.kt` — DE
- `localization/translations/SpanishLocalization.kt` — ES
- `localization/translations/FrenchLocalization.kt` — FR
- `localization/translations/ItalianLocalization.kt` — IT
- `localization/translations/RussianLocalization.kt` — RU (new, not in legacy)
- `localization/translations/ChineseLocalization.kt` — ZH (new, not in legacy)

### Platform implementations
- `expect fun getCurrentLanguage(): AvailableLanguages`
- `@Composable expect fun SetLanguage(language: AvailableLanguages)`
- JVM: `Localization.jvm.kt` — reads `GameSettings.language`, falls back to system locale
- Android: `Localization.android.kt`
- iOS: `Localization.ios.kt`

### Current string coverage
The `Localization` interface has 35 strings covering:
- App name, back, loading
- Game HUD: level, game over, pause/unpause, score, lives, wave, enemies, cleared
- Main menu: new game, extras, options, quit
- Select character screen: heading, level, unavailable, confirm hint
- Options screen: all 5 option labels
- Extras menu: how to play, leaderboard, credits
- Pause menu: resume, how to play, options, restart level, quit to main menu
- Game over: restart wave, restart from beginning, return to menu
- Input hints: press any key, tap to continue, stage cleared, back, ESC—Back

### Scenes using localization
| Scene | Status |
|---|---|
| StartScene | ✅ uses `getCurrentLocalization()` |
| MenuScene | ✅ uses `getCurrentLocalization()` |
| ExtrasMenuScene | ✅ uses `getCurrentLocalization()` |
| OptionsScene | ⚠️ hardcoded EN strings (localization wiring deferred) |
| SelectPlayerScene | ⚠️ hardcoded EN strings (localization wiring deferred) |
| HowToPlayScene | ⚠️ hardcoded EN strings (localization wiring deferred) |
| CreditsScene | N/A — content is text body, not UI labels |
| GameplayScene | ⚠️ HUD labels hardcoded EN (localization wiring deferred) |

### Language selection
- Language stored in `GameSettings.language` (string key: "en"/"de"/"es"/"fr"/"it"/"ru"/"cn")
- OptionsScene shows "LANGUAGE" option but interactive selection UI not yet wired to `GameSettings.language`
- Default: "en"

### Missing / deferred
- Wiring OptionsScene / SelectPlayerScene / HowToPlayScene / GameplayScene HUD to use localized strings
- Language selector in OptionsScene (cycle through available languages, persist selection)
- Verification of DE/ES/FR/IT translations against original .resx content (spot-checked for common strings, full audit pending)
