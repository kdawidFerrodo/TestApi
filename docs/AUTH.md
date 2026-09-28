# Logowanie i dostęp do endpointów

## Model

| | Access token | Refresh token |
|---|---|---|
| Format | JWT podpisany HMAC-SHA256 | losowe 64 bajty (base64url), nieprzezroczyste |
| Czas życia | 15 min (`Jwt:AccessTokenLifetimeMinutes`) | 7 dni (`Jwt:RefreshTokenLifetimeDays`) |
| Gdzie sprawdzany | każdy request, bez bazy (podpis + `exp`) | tylko `/api/auth/refresh`, w bazie |
| W bazie | nic | tylko hash SHA-256 |
| Wysyłany | nagłówek `Authorization: Bearer <token>` | body `/refresh` i `/logout` |

Access token jest bezstanowy, więc nie da się go odwołać - dlatego żyje krótko. Refresh token jest
stanowy (w bazie), więc można go unieważnić: wylogowanie, blokada konta, zmiana hasła.

## Przepływ

1. `POST /api/auth/login` `{ userName, password }` → `{ accessToken, refreshToken, ...ExpiresAt }`
2. Klient wysyła `Authorization: Bearer <accessToken>` do każdego endpointu.
3. API zwraca `401` (token wygasł) → klient wywołuje `POST /api/auth/refresh` `{ refreshToken }`,
   dostaje **nową parę** tokenów i ponawia request. Stary refresh token przestaje działać (rotacja).
4. `POST /api/auth/logout` `{ refreshToken }` - kończy bieżącą sesję (urządzenie).
   `POST /api/auth/logout-all` (z access tokenem) - kończy wszystkie sesje użytkownika.

### Rotacja i wykrywanie kradzieży

Każde logowanie tworzy "rodzinę" (`FamilyId`) refresh tokenów. Przy odświeżeniu stary token jest
oznaczany jako zastąpiony. Jeżeli ktoś użyje zastąpionego tokenu ponownie, to znaczy, że token wyciekł
(albo użył go złodziej, albo prawowity klient po złodzieju) - unieważniana jest cała rodzina i obie strony
muszą zalogować się ponownie.

Klient musi serializować odświeżanie (jedno wywołanie `/refresh` naraz, pozostałe requesty czekają na
wynik) - dwa równoległe odświeżenia tym samym tokenem są traktowane jak kradzież.

## Uprawnienia

Endpointy sprawdzają **uprawnienia**, nie role:

```csharp
[HasPermission(Permissions.Invoices.Read)]
public IActionResult GetInvoices() { ... }
```

- Katalog uprawnień: `Domain/Authorization/Permissions.cs`.
- W bazie: `Role` ↔ `RolePermissions` ↔ `UserRoles`. Rola to nazwany zestaw uprawnień, więc zmiana
  zakresu roli nie wymaga zmian w kodzie.
- Uprawnienia trafiają do access tokenu przy logowaniu i przy każdym odświeżeniu, więc zmiana uprawnień
  działa najpóźniej po 15 minutach.
- Domyślnie **każdy** endpoint wymaga zalogowania (fallback policy). Publiczne oznaczamy `[AllowAnonymous]`.

## Co zostało do zrobienia

- `Persistence/Repositories/UserRepository.cs` - zapytania o użytkownika (login + role + uprawnienia).
  Hash hasła tworzony przez `IPasswordHasher.Hash`.
- `InMemoryRefreshTokenRepository` → tabela `RefreshTokens` (unikalny indeks na `TokenHash`, indeks na
  `FamilyId` i `UserId`) + okresowe usuwanie wygasłych wierszy. `TryRevokeAsync` musi być atomowe
  (`UPDATE ... WHERE RevokedAt IS NULL`).
- Blokada konta po N nieudanych logowaniach.
- Po zmianie hasła / blokadzie konta: `IRefreshTokenRepository.RevokeAllForUserAsync`.

## Konfiguracja klucza

`Jwt:SigningKey` (min. 32 bajty) nigdy nie trafia do repozytorium. Aplikacja nie wystartuje bez niego.

```bash
# dev
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -base64 48)" --project FerrodoERPApi
# prod: zmienna środowiskowa Jwt__SigningKey albo Key Vault
```

Klucz w `appsettings.Development.json` jest tylko do lokalnego developmentu.
