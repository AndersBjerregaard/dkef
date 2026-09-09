# Frontend Supplementary Context — `src/dkef-vue/`

## Language

The end users of this application exclusively speak Danish.
**All user-facing text must be hard-coded in Danish.** This includes labels, buttons, headings,
badges, placeholder text, error messages, and any other strings rendered in the UI.
Non-user-facing text (code comments, console/log messages) may be in English.

---

## Source Directory Map

```
src/
├── assets/          Static assets (images, global CSS)
├── components/      Feature and base UI components (see inventory below)
│   └── __tests__/   Vitest unit tests — one .spec.ts per component
├── router/          Vue Router config (index.ts) — all routes defined here
├── services/        apiservice.ts (Axios), urlservice.ts (URL construction)
├── stores/          Pinia stores — one file per domain
├── types/           TypeScript interfaces and DTOs — one file per domain
└── views/           Thin route-level wrapper components
```

---

## View → Route Inventory

All views are lazy-loaded except `HomeView` (eager). Auth behaviour is controlled by route
`meta` flags enforced in the navigation guard in `router/index.ts`.

| View | Path | Auth |
|---|---|---|
| `HomeView.vue` | `/` | Public |
| `AboutView.vue` | `/about` | Public |
| `MemberAdvantagesView.vue` | `/advantages` | Public |
| `EventsAndNewsView.vue` | `/events-and-news` | Public |
| `SpecificEventView.vue` | `/events-and-news/:id` | Public |
| `SpecificNewsView.vue` | `/news/:id` | Public |
| `SpecificGeneralAssemblyView.vue` | `/general-assemblies/:id` | Public |
| `ContactView.vue` | `/contact` | Public |
| `MembersView.vue` | `/members` | `requiresAuth: true` |
| `RegisterView.vue` | `/register` | `guest: true` (redirects home if logged in) |
| `ForgotPasswordView.vue` | `/forgot-password` | `guest: true` |
| `ResetPasswordView.vue` | `/reset-password` | `guest: true` |

There is no `/login` route — login is handled inline within the auth flow (the guard redirects
to `{ name: 'login' }` which does not currently exist as a named route; be aware of this).

---

## Component Inventory

**Base / reusable:**
- `BaseModal.vue` — wraps `@headlessui/vue` `Dialog` + `TransitionRoot`; accepts `isOpen`,
  `title`, `isLoading`, `maxWidth` props; emits `close`; has a default slot and a `footer`
  slot. **All new modals must extend this component, not re-implement dialog logic.**
- `LoadingButton.vue` — button with a loading spinner state

**Display components:**
- `HomeComponent.vue`, `AboutComponent.vue`, `MemberAdvantages.vue` — static content sections
- `EventComponent.vue` — card for a single event; links to `/events-and-news/:id`
- `NewsComponent.vue` — card for a single news item
- `GeneralAssemblyComponent.vue` — card for a single general assembly
- `EventsAndNewsComponent.vue` — combined feed list
- `MembersComponent.vue`, `MemberComponent.vue`, `MemberHeaderComponent.vue` — member list/detail
- `ContactComponent.vue`, `ContactFormularComponent.vue` — contact page and form
- `NewsLetterComponent.vue` — newsletter signup section
- `AuthNav.vue` — navigation bar auth state (login/logout controls)
- `HelloWorld.vue`, `WelcomeItem.vue` — scaffolded placeholder components

**Admin edit modals (require `isAdmin` from `authStore`):**
- `EditEventModal.vue` — create/edit an event via `eventStore.updateEvent`
- `EditNewsModal.vue` — create/edit a news item
- `EditGeneralAssemblyModal.vue` — create/edit a general assembly

---

## Store Inventory

| Store file | Store id | Style | Persisted | Purpose |
|---|---|---|---|---|
| `authStore.ts` | `auth` | Composition API | Yes — `accessToken`, `refreshToken`, `user` | Auth state, JWT decode, token refresh |
| `eventStore.ts` | `event` | Options API | No | Event fetching and caching by id |
| `newsStore.ts` | `news` | Options API | No | News fetching and caching |
| `generalAssemblyStore.ts` | `generalAssembly` | Options API | No | General assembly fetching |
| `feedStore.ts` | `feed` | Options API | No | Combined feed (`/feed` endpoint) |
| `themeStore.ts` | `theme` | — | — | Dark/light theme toggle |
| `counter.ts` | `counter` | — | — | Scaffolded placeholder, not used in production |

Note: `eventStore`, `newsStore`, `feedStore`, and `generalAssemblyStore` use the **Options API**
store style (with `state`, `getters`, `actions`), unlike `authStore` which uses Composition API.
Follow the existing style of the store you are modifying.

---

## URL Service

**Always use `@/services/urlservice.ts` to obtain API URLs — never hardcode paths.**

`urlservice` switches on `VITE_MODE` to support mock data in development. When
`VITE_MODE === 'Development'`, most endpoints throw `'Unimplemented!'`; only `getContacts()`
returns a static `/contacts.json` mock. All other modes (Docker, k8s, staging) hit the real API.

Available URL builder functions: `getContacts`, `updateContact`, `getContactAuthorize`,
`getEvents`, `getEvent`, `postEvent`, `updateEvent`, `getEventPresignedUrl`,
`getNews`, `getNewsItem`, `postNews`, `updateNews`, `getNewsPresignedUrl`,
`getGeneralAssemblies`, `getGeneralAssembly`, `postGeneralAssembly`, `updateGeneralAssembly`,
`getGeneralAssemblyPresignedUrl`, `getFeed`.

---

## Tailwind Theming

The app uses **semantic CSS custom property tokens** rather than raw Tailwind color classes.
Always use these tokens for new UI — do not use raw colors like `bg-gray-800` or `bg-navy-900`.

| Token class | Usage |
|---|---|
| `bg-theme-mute` | Card / surface backgrounds |
| `bg-theme-soft` | Subtle background (image placeholders, etc.) |
| `text-theme-text` | Secondary / muted body text |
| `text-theme-muted` | Icon and de-emphasised text |
| `text-theme-accent` | Accent / highlight colour |
| `border-theme-border` | All borders |

A custom `navy` palette is also defined (`navy-600` through `navy-950`) for deep background use.
Amber (`amber-500`) is used as the interactive hover/focus highlight colour.
