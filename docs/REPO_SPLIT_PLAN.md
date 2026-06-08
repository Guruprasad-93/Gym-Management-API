# Repository Split Plan — API vs UI

## Current layout (monorepo)

| Path | Purpose | Files (tracked) |
|------|---------|-----------------|
| `Backend/` | ASP.NET Core 8 API, Application, Domain, Infrastructure, IntegrationTests | 658 |
| `Frontend/gym-app/` | Angular 19 SPA | 373 |
| `docs/` | Project docs, test cases | shared |
| Root `*.md`, `Dockerfile`, `docker-compose.yml` | API deployment & module docs | API-related |

## Angular project identity

| Item | Location |
|------|----------|
| Angular CLI config | `Frontend/gym-app/angular.json` |
| NPM manifest | `Frontend/gym-app/package.json`, `package-lock.json` |
| TypeScript config | `Frontend/gym-app/tsconfig.json`, `tsconfig.app.json`, `tsconfig.spec.json` |
| Dev proxy to API | `Frontend/gym-app/proxy.conf.json` |
| Application source | `Frontend/gym-app/src/` (359 files) |
| Static assets | `Frontend/gym-app/public/` |
| VS Code tasks | `Frontend/gym-app/.vscode/` |

## Destination: `Gym-Management-UI` repository

**373 files** from `Frontend/gym-app/` will be copied to the **repository root** (flatten `Frontend/gym-app/` prefix).

Full path list: [`ANGULAR_FILES_TO_MOVE.txt`](./ANGULAR_FILES_TO_MOVE.txt)

### Top-level breakdown (after flattening)

| Path in UI repo | Count | Description |
|-----------------|-------|-------------|
| `src/` | 359 | Components, services, routes, styles, environments |
| `.vscode/` | 3 | Launch/tasks/extensions |
| `angular.json` | 1 | Angular workspace |
| `package.json` / `package-lock.json` | 2 | Dependencies |
| `tsconfig*.json` | 3 | TypeScript |
| `proxy.conf.json` | 1 | `/api` → `localhost:5088` |
| `public/` | 1 | `favicon.ico` |
| `.editorconfig`, `.gitignore`, `README.md` | 3 | Project config |

### `src/app` feature areas (all move)

- `core/` — guards, interceptors, services, constants
- `features/auth/`, `register/`, `login/`
- `features/super-admin/`
- `features/gym-admin/` — all admin modules
- `features/trainer/`, `features/member/`
- `features/public-website/`
- `layout/` — sidebar, header, main layout
- `shared/` — components, models, styles

### Excluded from UI repo (gitignored, not copied)

- `node_modules/`
- `dist/`
- `.angular/cache/`

## Stays in `Gym-Management-API` repository

- Entire `Backend/` folder (solution, API, tests, SQL scripts)
- `Dockerfile`, `docker-compose.yml` (API + SQL Server)
- Root implementation/deployment `*.md` documents
- `docs/` (including this plan and file manifest)
- `.env.example`, root `.gitignore` (API-focused)

## Notes

1. `angular.json` references `../../../Plugins/HTML Templates/ahana-master` — that folder is **not in git**. The asset entry will be removed in the UI repo to avoid build errors.
2. No code is deleted: frontend source is **copied** to UI repo, then **removed from API repo tracking** only.
3. UI repo URL: https://github.com/Guruprasad-93/Gym-Management-UI.git
4. API repo URL: https://github.com/Guruprasad-93/Gym-Management-API.git

## Status (completed)

| Step | Result |
|------|--------|
| Copy `Frontend/gym-app/` → UI repo root | Done — `g:\GymManagementSystem-UI` (373 files) |
| Push UI repo | Done — https://github.com/Guruprasad-93/Gym-Management-UI |
| Remove `Frontend/` from API repo tracking | Done — `git rm -r --cached`; local files retained |
| API `README.md` | Points to UI repo |
