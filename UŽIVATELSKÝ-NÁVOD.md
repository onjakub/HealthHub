# HealthHub - Uživatelský návod

## Obsah
1. [Úvod](#úvod)
2. [Instalace a spuštění](#instalace-a-spuštění)
3. [Přihlášení do systému](#přihlášení-do-systému)
4. [Správa pacientů](#správa-pacientů)
5. [Přidávání diagnóz](#přidávání-diagnóz)
6. [Vyhledávání pacientů](#vyhledávání-pacientů)
7. [Řešení problémů](#řešení-problémů)

## Úvod

HealthHub je moderní systém pro správu pacientů a jejich zdravotních záznamů. Aplikace umožňuje:

- Přidávání a úpravu pacientů
- Správu diagnostických výsledků
- Vyhledávání v seznamu pacientů
- Bezpečné ukládání zdravotních údajů

## Instalace a spuštění

Aplikace bude dostupná na:
- **Webové rozhraní**: http://localhost:5023
- **GraphQL API**: http://localhost:5023/graphql

- Backend běží na: **http://localhost:5023**
- Frontend běží na: **http://localhost:3000**

## Přihlášení do systému

1. Otevřete aplikaci v prohlížeči (http://localhost:5023)
2. Na přihlašovací obrazovce zadejte:
   - **Uživatelské jméno**: libovolné (demo účel)
   - **Heslo**: libovolné (demo účel)

**Poznámka**: V demo režimu aplikace přijímá jakékoli přihlašovací údaje. Token platí 12 hodin.

## Správa pacientů

### Přidání nového pacienta

1. Po přihlášení klikněte na tlačítko **"Add Patient"**
2. Vyplňte povinné údaje:
   - **Jméno** (povinné)
   - **Příjmení** (povinné)
   - **Datum narození** (povinné, nesmí být v budoucnosti)
3. Klikněte na **"Add Patient"** pro uložení

### Úprava existujícího pacienta

1. V seznamu pacientů klikněte na tlačítko **"Edit"** u vybraného pacienta
2. Upravte potřebné údaje
3. Klikněte na **"Update Patient"** pro uložení změn

### Seznam pacientů

- Zobrazuje všechny uložené pacienty
- Automaticky počítá věk z data narození
- Zobrazuje datum vytvoření záznamu

## Přidávání diagnóz

### Pro existujícího pacienta

1. Otevřete detail pacienta kliknutím na **"Edit"**
2. V sekci **"Medical Diagnoses"** vyplňte:
   - **Diagnóza** (povinné)
   - **Klinické poznámky** (volitelné)
3. Klikněte na **"Add Diagnosis"**

### Zobrazení diagnostických výsledků

- Seznam všech diagnóz pacienta
- Časové razítko přidání diagnózy
- Možnost přidání poznámek ke každé diagnóze

## Vyhledávání pacientů

1. V horní části seznamu pacientů použijte vyhledávací pole
2. Zadejte jméno nebo příjmení pacienta
3. Systém filtruje pacienty v reálném čase

## Řešení problémů

### Aplikace se nespustí

**Chyba: Port již používán**
```bash
# Zkontrolujte používané porty
lsof -i :5023
# Ukončete proces pokud je potřeba
kill -9 <PID>
```

**Chyba: Databáze není dostupná**
```bash
# Zkontrolujte stav databáze
docker-compose ps
# Restartujte služby
docker-compose restart
```

### Přihlášení nefunguje

- Zkontrolujte, zda backend běží (http://localhost:8080/health)
- Vyčistěte cache prohlížeče
- Zkuste použít jiné přihlašovací údaje

### Data se nezobrazují

- Zkontrolujte připojení k internetu
- Ověřte, zda je JWT token platný (platnost 12 hodin)
- Zkuste se odhlásit a znovu přihlásit

### GraphQL chyby

- Ověřte, zda máte platný autentizační token
- Zkontrolujte syntaxi GraphQL dotazů v konzoli prohlížeče

## Bezpečnostní doporučení

1. **V produkčním prostředí** změňte výchozí JWT klíč
2. **Implementujte skutečnou autentizaci** namísto demo režimu
3. **Pravidelně zálohujte databázi**
4. **Používejte HTTPS** v produkčním prostředí

## Technické detaily

### Databázové schéma

**Tabulka Patients:**
- ID (UUID)
- Jméno, příjmení
- Datum narození
- Časové razítko vytvoření/aktualizace

**Tabulka DiagnosticResults:**
- ID (UUID)
- ID pacienta (cizí klíč)
- Diagnóza
- Klinické poznámky
- Časové razítko

### API endpointy

- **GraphQL**: `/graphql` (veškerá funkcionalita)
- **Health check**: `/health` (stav aplikace)
- **Authentication**: `/auth/token` (získání JWT)

## Podpora

Pro technickou podporu nebo hlášení chyb kontaktujte vývojový tým prostřednictvím issue trackeru projektu.

---

*Tento návod byl vytvořen pro verzi HealthHub 1.0. Poslední aktualizace: 30.11.2025*

## Rychlý přehled funkcí

| Funkce | Popis | Jak používat |
|--------|-------|-------------|
| Přihlášení | Demo autentizace | Libovolné jméno/heslo |
| Přidání pacienta | Nový záznam | "Add Patient" → vyplnit údaje |
| Úprava pacienta | Editace existujícího | "Edit" u pacienta → změnit údaje |
| Přidání diagnózy | Zdravotní záznam | "Edit" pacienta → "Medical Diagnoses" |
| Vyhledávání | Filtrování pacientů | Pole "Search patients by name..." |
| Odhlášení | Ukončení session | Tlačítko "Logout" vpravo nahoře |

## Klávesové zkratky (pokud podporováno prohlížečem)

- **Enter** v přihlašovacím formuláři → odeslat
- **Esc** v některých formulářích → zrušit
- **Tab** → přechod mezi poli formuláře

## Časté otázky

**Q: Jak dlouho zůstanu přihlášen?**
A: Token platí 12 hodin. Po této době se musíte znovu přihlásit.

**Q: Mohu smazat pacienta?**
A: Ano, funkce pro mazání pacientů je implementována v API.

**Q: Je možné exportovat data?**
A: Export funkcionalita není v současné verzi implementována.

**Q: Jak zálohovat data?**
A: Použijte příkaz `docker-compose exec healthhub-db pg_dump` pro zálohu databáze.