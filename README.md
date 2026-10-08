# E-Library Management (ASP.NET Web Forms · SQL Server · Bootstrap 4 · AI Librarian)

## Run it
1. **Database** – open `database/schema.sql` in SSMS and run it (creates `ElibraryDB`, tables, views, procedures, sample data).
2. **Connection** – edit the `con` connection string in `ELibraryManagement/Web.config` to point at your SQL Server instance.
3. **Open** `ELibraryManagement.sln` in Visual Studio 2019/2022, let NuGet restore, press F5.
4. **Sign in** – admin: `admin` / `Admin@123` (Admin Login in the footer) · member: `demo` / `Demo@1234`. **Change both immediately.**
5. **AI Librarian (optional)** – copy `Web.Secrets.config.example` to `Web.Secrets.config` and add your Anthropic API key
   (or set the `ANTHROPIC_API_KEY` environment variable). Without a key the page falls back to keyword search.

## What changed from v1
- **Security:** all SQL parameterized (SQL injection fixed), PBKDF2 password hashing, role checks on every admin/member page
  (`AdminPage`, `MemberPage`), login lockout (5 tries / 10 min), generic error messages (details go to `Trace`), validated cover uploads, security headers.
- **Features completed:** book inventory (add/update/delete + cover upload), book issue/return with fines, member profile & password change,
  public catalog, only *Active* members can log in.
- **Code quality:** one data-access class, shared page base classes, no duplicated connection strings, leftovers and build output removed, `.gitignore` added.
- **AI layer:** `Services/AiService.cs` + `askai.aspx` – ask for recommendations in plain English; answers are grounded in your catalog,
  rate-limited per user, and the model output is HTML-encoded before display.
- **Look:** new fonts (Inter + Playfair Display via Google Fonts), refreshed palette, new hero/banner/AI illustrations.

See `docs/DB_DESIGN.md` for the ER diagram and database objects.
