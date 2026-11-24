# Approved editable site-text inventory

**Approval: accepted on 2026-11-24**, including all baseline and layout follow-up
decisions; see the [approval record](#approval-record). Prepared for
[work item 01](01-field-inventory.md); this document is the approved source for
the package-local registries in 02. Item 01 itself changed no application behavior.

## Reading the inventory

There are **73 fields in four groups**: `about` (15), `home` (27), `membership`
(5), and `member-advantages` (26). Group order is the order here. Sections and
fields follow their first appearance in each table, top to bottom; this is the
proposed deterministic editor order, never alphabetical or administrator-managed.
Section labels are Danish. Repeated section labels denote the same section.

Source paths below are relative to `src/dkef-vue/src/components/`:

- **A** = [AboutComponent.vue](../../src/dkef-vue/src/components/AboutComponent.vue)
- **H** = [HomeComponent.vue](../../src/dkef-vue/src/components/HomeComponent.vue)
- **M** = [MemberAdvantages.vue](../../src/dkef-vue/src/components/MemberAdvantages.vue)

`A:9`, for example, identifies the current literal at line 9 to replace with a
binding to that row's group/key in the later integration. Line numbers describe
the reviewed source, not persistent identifiers. Shared rows have two bindings.
Defaults preserve spelling, punctuation, and case in the source, with HTML
whitespace collapsed to one space and surrounding indentation removed. CSS
uppercase is presentation, not uppercase stored wording. Checkmarks, bullets,
and the two contact prefixes are not part of values.

### Proposed validation conventions

- Every row explicitly specifies required status. **All 73 initial slots are
  required**: none has an agreed empty-state presentation, and empty paragraphs,
  headings, or fixed entries would leave unexplained gaps. Optional fields remain
  supported by the later generic model, but are not selected here.
- `text` means a single-line input; `multiline` means a textarea. `plain` means
  plain text with no additional semantic format rule. No HTML rendering, URLs as
  navigation configuration, or arbitrary keys. Single-line fields reject line
  breaks; multiline fields may contain LF line breaks, preserved by component-owned
  presentation in the later integrations. Normalize CRLF to LF before validation.
- Required values must contain a non-whitespace character after the established
  DTO sanitization; whitespace-only and sanitized-empty values fail. Do not silently
  truncate. Maximums apply to the sanitized value, including spaces and LF, counted
  as UTF-16 code units (JavaScript string length / .NET string length). Existing
  defaults contain no supplementary characters after removing template checkmarks.
  Item 02 must make client/server normalization and sanitization parity executable.
- `email` means one bare mailbox address, no display name, prefix, spaces, or line
  breaks, validated on both client and API; use the API's email-validation convention
  and matching contract tests in 02. Do not accept `Email: info@dkef.dk` as a value.
- `phone` means display-only digits, spaces, `+`, `-`, and parentheses, with at
  least one digit. It is not a Danish-only eight-digit business rule.
- Numerical limits below are editorial bounds, **not a guarantee of layout fit**.
  See the [layout review](#layout-review-and-follow-up) before accepting them.

## `about` — Om foreningen

| Key | Exact default | Danish editor label | Danish section | Input | Required | Max | Format | Source binding |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `page.title` | Om Elektroteknisk forening | Sideoverskrift | Sidehoved | text | yes | 60 | plain | A:9 |
| `page.subtitle` | Vi sætter strøm til elteknik-branchen | Underoverskrift | Sidehoved | text | yes | 60 | plain | A:12 |
| `introduction.title` | Om foreningen | Overskrift om foreningen | Foreningen | text | yes | 60 | plain | A:18 |
| `introduction.description` | Hos Elektroteknisk forening samler vi elbranchen på tværs af faglige nicher, virksomheder og alle med en interesse for området. Vi er en traditionsrig forening, men vi er samtidig et moderne og dynamisk samlingspunkt, der nysgerrigt opsøger nye indsigter og perspektiver. Vores mål er at give vores medlemmer stærke muligheder for at skabe netværk og udvikle sig fagligt gennem både sparring med branchekollegaer og arrangementer. | Beskrivelse af foreningen | Foreningen | multiline | yes | 450 | plain | A:21–28 |
| `sections.title` | Sektioner | Overskrift over sektionsbilledet | Foreningen | text | yes | 60 | plain | A:33 |
| `statistics.members.label` | Medlemmer | Etiket for medlemstal | Nøgletal | text | yes | 40 | plain | A:45 |
| `statistics.sections.label` | Sektioner | Etiket for antal sektioner | Nøgletal | text | yes | 40 | plain | A:51 |
| `statistics.founded.label` | Siden | Etiket for stiftelsesår | Nøgletal | text | yes | 40 | plain | A:57 |
| `secretariat.title` | Sekretariat | Overskrift for sekretariatet | Sekretariat | text | yes | 40 | plain | A:66 |
| `secretariat.name` | Elektroteknisk forening | Navn | Sekretariat | text | yes | 60 | plain | A:69 |
| `secretariat.address` | Elleholmvej 28C | Adresse | Sekretariat | text | yes | 60 | plain | A:72 |
| `secretariat.postal-city` | 5250 Odense SV | Postnummer og by | Sekretariat | text | yes | 60 | plain | A:75 |
| `secretariat.phone` | 42 57 70 69 | Telefonnummer | Sekretariat | text | yes | 25 | phone | A:78; prefix `Tlf. ` stays in template |
| `secretariat.email` | info@dkef.dk | E-mailadresse | Sekretariat | text | yes | 80 | email | A:81; prefix `Email: ` stays in template |
| `contact.title` | Kontakt os | Overskrift over kontaktformularen | Kontakt | text | yes | 40 | plain | A:94 |

## `home` — Forside

| Key | Exact default | Danish editor label | Danish section | Input | Required | Max | Format | Source binding |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `hero.eyebrow` | Elektroteknisk forening | Overlinje | Sidehoved | text | yes | 60 | plain | H:27–29 |
| `hero.title` | Dit netværk i elbranchen | Hovedoverskrift | Sidehoved | text | yes | 100 | plain | H:30 |
| `hero.description` | Udvid din faglige horisont, bliv en del af fællesskabet og modtag sparring fra erfarne hænder. | Introduktion | Sidehoved | multiline | yes | 300 | plain | H:32–35 |
| `companies.title` | Vores medlemsvirksomheder | Overskrift over medlemsvirksomheder | Medlemsvirksomheder | text | yes | 60 | plain | H:54–56 |
| `network.eyebrow` | Netværk | Overlinje | Netværk | text | yes | 60 | plain | H:107 |
| `network.title` | Sparring og relationer på tværs af branchen | Overskrift | Netværk | text | yes | 100 | plain | H:108–110 |
| `network.description` | Med over 550 medlemmer er du garanteret nye forbindelser og indsigter inden for forskellige eltekniske områder. | Beskrivelse | Netværk | multiline | yes | 300 | plain | H:112–115 |
| `network.action-label` | Medlemsfordele | Knaptekst | Netværk | text | yes | 32 | plain | H:117–122 |
| `development.eyebrow` | Udvikling | Overlinje | Faglig udvikling | text | yes | 60 | plain | H:131 |
| `development.title` | Fokus på faglig udvikling | Overskrift | Faglig udvikling | text | yes | 100 | plain | H:132 |
| `development.description` | Bliv klogere på de nyeste tendenser og andre specialeområder ved at deltage i arrangementer og virksomhedsbesøg. | Beskrivelse | Faglig udvikling | multiline | yes | 300 | plain | H:134–137 |
| `development.action-label` | Bliv medlem | Knaptekst | Faglig udvikling | text | yes | 32 | plain | H:139–144 |
| `reasons.eyebrow` | Hvorfor? | Overlinje | Grunde til medlemskab | text | yes | 60 | plain | H:157 |
| `reasons.title` | 3 gode grunde til at være medlem | Overskrift | Grunde til medlemskab | text | yes | 100 | plain | H:158–160 |
| `reasons.description` | Stadig i tvivl? Lad os lige skitsere, hvad du får ud af et medlemskab. | Introduktion | Grunde til medlemskab | multiline | yes | 300 | plain | H:162–164 |
| `reasons.action-label` | Bliv medlem | Knaptekst | Grunde til medlemskab | text | yes | 32 | plain | H:166–171 |
| `reasons.sparring.title` | Sparring | Overskrift om sparring | Grunde til medlemskab | text | yes | 60 | plain | H:178 |
| `reasons.sparring.description` | Diskuter og udveksl ideer med andre | Beskrivelse af sparring | Grunde til medlemskab | multiline | yes | 150 | plain | H:179 |
| `reasons.network.title` | Netværk | Overskrift om netværk | Grunde til medlemskab | text | yes | 60 | plain | H:185 |
| `reasons.network.description` | Få nye forbindelser — og måske venskaber | Beskrivelse af netværk | Grunde til medlemskab | multiline | yes | 150 | plain | H:186 |
| `reasons.development.title` | Faglig udvikling | Overskrift om faglig udvikling | Grunde til medlemskab | text | yes | 60 | plain | H:192 |
| `reasons.development.description` | Styrk kompetencer og opnå ny viden | Beskrivelse af faglig udvikling | Grunde til medlemskab | multiline | yes | 150 | plain | H:193 |
| `plans.title` | Vores medlemstyper | Overskrift over medlemskort | Medlemskab | text | yes | 100 | plain | H:205 |
| `plan.title` | Bliv medlem | Overskrift på medlemskort | Medlemskab | text | yes | 60 | plain | H:213 |
| `plan.description` | Giv dig selv de bedste betingelser for et stærkt uddannelsesforløb. | Beskrivelse på medlemskort | Medlemskab | multiline | yes | 200 | plain | H:219–221 |
| `plan.activities-description` | Deltag i studieture, del ud af din viden og styrk dit arbejdsnetværk. | Beskrivelse af aktiviteter | Medlemskab | multiline | yes | 200 | plain | H:222–224 |
| `plan.action-label` | Bliv medlem | Knaptekst på medlemskort | Medlemskab | text | yes | 32 | plain | H:225–230 |

## `membership` — Fælles medlemsfordele

These are deliberately shared editorial values, not automatic string deduplication.
Both components bind the same keys; the five entries and their order remain fixed.

| Key | Exact default | Danish editor label | Danish section | Input | Required | Max | Format | Source bindings |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `benefits.development` | Faglig udvikling | Faglig udvikling | Medlemsfordele | text | yes | 60 | plain | H:232; M:47 |
| `benefits.network` | Styrk dit netværk | Netværk | Medlemsfordele | text | yes | 60 | plain | H:233; M:48 |
| `benefits.mentoring` | Mentorordning | Mentorordning | Medlemsfordele | text | yes | 60 | plain | H:234; M:49 |
| `benefits.events` | Studieture og arrangementer | Studieture og arrangementer | Medlemsfordele | text | yes | 60 | plain | H:235; M:50 |
| `benefits.magazine` | Modtag EL + Energi bladet | Fagblad | Medlemsfordele | text | yes | 60 | plain | H:236; M:51 |

## `member-advantages` — Medlemsfordele

| Key | Exact default | Danish editor label | Danish section | Input | Required | Max | Format | Source binding |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `page.title` | Medlemsfordele | Sideoverskrift | Sidehoved | text | yes | 100 | plain | M:15 |
| `introduction.eyebrow` | Medlemskab | Overlinje | Introduktion | text | yes | 60 | plain | M:21 |
| `introduction.title` | Bliv medlem af Elektroteknisk forening i dag | Overskrift | Introduktion | text | yes | 100 | plain | M:22–24 |
| `introduction.description` | Udvid dit netværk, opnå nye kompetencer, og accelerér din udvikling som studerende i elbranchen med overskuelige priser | Beskrivelse | Introduktion | multiline | yes | 300 | plain | M:25–28 |
| `introduction.action-label` | Bliv medlem | Knaptekst | Introduktion | text | yes | 32 | plain | M:29–34 |
| `membership.title` | Medlemskab | Overskrift over medlemsfordele | Medlemskab | text | yes | 60 | plain | M:44 |
| `student-offers.title` | Tilbud til studerende | Overskrift over studentertilbud | Studentertilbud | text | yes | 60 | plain | M:54 |
| `student-offers.mentoring` | Mentorordning | Mentorordning for studerende | Studentertilbud | text | yes | 60 | plain | M:57–59 |
| `student-offers.network-meetings` | Fagligt aktuelle netværksmøder | Netværksmøder | Studentertilbud | text | yes | 60 | plain | M:60–62 |
| `student-offers.project-network` | Netværksdatabase til projektskrivning | Projektnetværk | Studentertilbud | text | yes | 60 | plain | M:63–65 |
| `student-offers.jobs` | Måske det er her du finder dit kommende job | Jobmuligheder | Studentertilbud | text | yes | 60 | plain | M:66–68 |
| `student-offers.included-benefits` | Inkl. alle fordele som andre medlemmer | Øvrige medlemsfordele | Studentertilbud | text | yes | 60 | plain | M:69–71 |
| `network.eyebrow` | Netværk | Overlinje | Netværk | text | yes | 60 | plain | M:85 |
| `network.title` | Få adgang til netværksdatabase | Overskrift | Netværk | text | yes | 100 | plain | M:86–88 |
| `network.description` | Søger du specifikke kompetencer og indsigter i elbranchen? Som medlem i Elektroteknisk forening har du adgang til en komplet oversigt over andre medlemmer, som du kan søge sparring og ekspertise hos. | Beskrivelse | Netværk | multiline | yes | 300 | plain | M:89–93 |
| `events.eyebrow` | Arrangementer | Overlinje | Arrangementer | text | yes | 60 | plain | M:111–113 |
| `events.title` | Deltag i arrangementer og studieture | Overskrift | Arrangementer | text | yes | 100 | plain | M:114–116 |
| `events.description` | Vil du styrke din faglighed og få et bredt kendskab til elbranchen? I Elektroteknisk forening afholder vi studieture og besøger virksomheder, så du kan udvikle dine kompetencer og opnå ny viden. | Beskrivelse | Arrangementer | multiline | yes | 300 | plain | M:117–121 |
| `other-benefits.eyebrow` | Fordele | Overlinje | Andre fordele | text | yes | 60 | plain | M:139 |
| `other-benefits.title` | Andre gode grunde til at være med i Elektroteknisk forening | Overskrift | Andre fordele | text | yes | 100 | plain | M:140–142 |
| `mentoring.title` | Bliv en del af vores mentorordning | Overskrift om mentorordning | Andre fordele | text | yes | 100 | plain | M:158–160 |
| `mentoring.description` | Som erfarent medlem i Elektroteknisk forening kan du dele ud af din viden ved at tage en studerende i elbranchen under dine vinger — og som studerende kan du sætte skub på din faglige udvikling med en personlig mentor. | Beskrivelse af mentorordning | Andre fordele | multiline | yes | 300 | plain | M:161–165 |
| `sparring.title` | Søg sparring på tværs af branchen | Overskrift om sparring | Andre fordele | text | yes | 100 | plain | M:181–183 |
| `sparring.description` | Lad dig inspirere af andre, og lad andre inspirere af dig. Elektroteknisk forening giver dig den ideelle platform til at udveksle og diskutere faglige indsigter med branchekollegaer fordelt over et væld af fagnicher. | Beskrivelse af sparring | Andre fordele | multiline | yes | 300 | plain | M:184–188 |
| `magazine.title` | Få gratis abonnement på EL+ENERGI | Overskrift om fagblad | Andre fordele | text | yes | 100 | plain | M:204–206 |
| `magazine.description` | Som medlem i Elektroteknisk forening modtager du hvert kvartal gratis den seneste udgave af fagbladet EL+ENERGI hvori du kan finde nyheder og faglige informationer om alt, der rør sig inden for el, energi og installation. | Beskrivelse af fagblad | Andre fordele | multiline | yes | 300 | plain | M:207–211 |

## Editorial boundaries proposed for approval

| Topic | Proposed decision |
| --- | --- |
| Sharing | Only the five `membership` fields are shared. Home reason cards, longer Member Advantages cards, student mentoring, matching headings, and repeated CTA labels retain separate keys. A shared-benefits save intentionally changes both pages. |
| About statistics | `+550`, `5`, and `1903` stay code-owned (A:48,54,60). Only their labels are editable. No new statistic or data binding. |
| Home member-count claim | Allow the complete `home/network.description`, including “over 550 medlemmer”, as editorial copy. This does not update About's count, a data-derived total, or a business rule. The administrator must keep the claim accurate; no synchronization is promised. Requires explicit human acceptance. |
| Review claim | Exclude the whole `⭐ 4.8/5 stjerner af vores 100+ anmeldelser.` claim (H:37–39). |
| Price | Exclude `700,-` and the complete `inkl. moms pr. år` suffix (H:215–218). Editing membership descriptions never changes fees, billing, tax treatment, or eligibility. |
| Contact | Store bare `info@dkef.dk` and `42 57 70 69`; templates own `Email: ` and `Tlf. ` with a space. Preserve the current visible output exactly. Values are display-only: do not bind them to contact-form recipients, backend mail configuration, authentication identities, or new `mailto:`/`tel:` actions. |
| Nested components | Exclude `ContactFormularComponent` internals, destinations, validation, and submission behavior; only A:94 is selected. Exclude all of `MigrationBanner` and the disabled newsletter. No nested form submission during inventory validation. |
| Structure/media | Exclude images, alt text, logos, icons, checkmarks, bullet characters, navigation targets, list size/order, card count/order, element types, styling, and layout controls. Headings remain wording, not structural controls; the three-reasons structure remains fixed. |

## Implementation baseline proposed for approval

Accept or amend these together with the rows, rather than treating the request to
prepare this inventory as approval:

1. Reuse Vue, authentication, `BaseModal.vue`, API client, and PostgreSQL; no CMS,
   separate service, cache infrastructure, or additional EF context.
2. Standalone `SiteTexts` entity/table in `ContentsContext`, not `BaseContent` or
   the event/news feed. Defaults stay in code; only overrides are persisted.
3. Package-local JSON definitions with modest duplication and an automated parity
   check, compatible with the existing package-local Docker build contexts; no
   code generator or shared runtime file requiring build-context changes.
4. Public `GET /SiteTexts/{group}`, Admin-only `PATCH /SiteTexts/{group}`; return
   overrides, not merged defaults or audit identities. Updates contain changed
   key/value pairs, validate all submitted entries, and save atomically per group.
5. Separate, clearly labelled page-text and shared-membership editing actions on
   Home and Member Advantages. No misleading two-group “save page” transaction.
6. Last-committed-write-wins on overlapping submitted keys; omitted keys untouched.
   No ETags, revisions, optimistic-locking UI, or cross-browser synchronization.
7. No restore-default UI. Missing override uses the default; an optional empty
   override would not be a reset, although no initial row allows an empty value.
8. Failed public loads retain readable defaults; editors require a successful group
   load before saving. Local working edits isolate cancellation and failed saves;
   successful saves publish immediately to shared in-memory state.

## Layout review and follow-up

Validation findings and approval record are maintained below. Limits deliberately
deviate from generic starting values: About uses shorter labels/headings and a
450-unit introduction instead of 800; compact Home cards use 150/200-unit
paragraphs; the other paragraphs use 300, not 800. Contact email is capped at 80
for this display slot, not the theoretical maximum mailbox length. Full-length
input may still contain unbreakable words or many line breaks; do not claim a
length limit alone prevents overflow.

### Validation performed

Reviewed against source at `2c56f0f` on 2026-11-24, on the existing
`feat/text-modification` branch (based on fetched `origin/master`). Only Markdown
is changed. No dependencies were installed, no API or database was started, and
no migrations or form submissions were run. Application build/lint/unit tests
were not run for this documentation-only change; source/default and browser
checks below validate the inventory itself.

- Temporary Python inspection parsed the tables: 73 unique group/key pairs,
  expected group counts, all required/default values nonempty, and all defaults
  within their maximums. It stripped tags and collapsed source whitespace at
  every referenced range: **78/78 exact matches**, including both copies of each
  shared benefit and the code-owned prefixes/markers. Default phone/email pass
  their proposed formats. About's introduction is **430 units**, below 450.
- Bun **1.4.4**, Vite **6.0.11**, Playwright CLI **0.1.25**, preinstalled headless
  Chromium, repository `.playwright/cli.config.json`, fresh in-memory session
  `field-inventory`. Confirmed `.env.development` is `VITE_MODE=Development` with
  mock API target before starting `bun dev --host 127.0.0.1` at
  `http://127.0.0.1:5173`. No staging/production target or authentication used.
- Visited `/`, `/about`, and `/advantages` at **1440×900**, **390×900**, and
  **320×900**. For each page/width, checked defaults, simultaneous maximum-length
  replacements, and unbroken-word probes: **27 page/viewport/probe cases**. Each
  case located all selected slots in `main` by tag, exact normalized text, and
  source occurrence (32 Home, 15 About, 31 Member Advantages).
- Replacements were ephemeral DOM `textContent` changes only, reset by navigation;
  no source edits or saves. Spaced probes repeat `Faglig viden og stærke netværk `
  and slice to each field's exact maximum. Email uses 60 `a` characters, `@`, 16
  `b` characters, and `.dk` (80 total); phone uses `+45 12 34 56 78 90 12 345`
  (25 total). Keep template prefixes/checkmarks/bullets. Unbroken probes use `W`
  repeated to each maximum; `W` phone/email probes are deliberately invalid format
  stress tests, **not examples the proposed validator should accept**.
- Compared element/text bounds and parent/viewport bounds, then visually inspected
  full-page screenshots of defaults and maximum examples at 1440 and 390 for all
  three pages. Geometry checks at 320 supplement that visual review. Screenshots
  and scripts remain temporary artifacts outside commits; the findings here are
  the durable handoff. This tests current single-paragraph rendering, not the
  future editor, server validation, or future preserved multiline presentation.
- Console inspection showed **no browser errors or warnings**; the request list
  showed no non-static API requests. Development mocks suffice for these static
  text/layout checks only; they cannot validate the future save flow.
- Local Markdown file links and `git diff --check` passed. The browser session
  was closed and only the task's Vite server was stopped after validation.

### Findings — not a layout pass

| Area | Default wording | Maximum/long-word examples | Proposed handoff |
| --- | --- | --- | --- |
| About introduction (`h-80`, 45% column) | Fits at 1440; at 390 and 320 the 430-unit paragraph extends above/below the 320px box, overlapping headings/statistics. | 450-unit spaced prose has the same narrow-screen failure. Unbroken prose/headings also spread outside their columns. | A lower maximum cannot fix the existing default while preserving it. Seek approval for content-safe responsive stacking/natural minimum height and wrapping in 07, without new editable structure. |
| About secretariat (`h-96`, 45% column) | Readable on desktop; narrow layout is cramped and the portrait/content extends beyond the nominal box. | 80-unit **valid bare email** spills out of the column even at desktop; maximum contact lines push the portrait into the footer at 390. | In 07, propose safe word wrapping, shrinkable columns, and content-driven minimum height instead of a fixed cap. Retest the complete contact block, including the unchanged portrait and adjacent form. Do not silently drop fields or truncate contact details. |
| Home content and shared list | Selected slots fit at 1440/390 with normal wrapping. At 320, `Vores medlemstyper` extends beyond the viewport in the fixed membership-card section. | Spaced maximums grow sections/cards without measured text clipping at 1440/390. Unbroken values overflow many headings, paragraphs, and fixed-width cards; CTAs can widen outside a narrow viewport. | In 08, propose wrapping/min-width constraints and CTA containment that keep the same cards/routes. Check the `w-80` membership card at 320; no price change. |
| Member Advantages content and lists | Selected text fits at desktop and narrow widths within its current elements. | Spaced maximums grow sections/cards without measured text clipping; unbroken values overflow both promotional copy and lists/cards. | In 09, propose safe wrapping/shrinkability for editable slots, fixed entries and buttons. Preserve card/list membership and order. |
| Shared page shell (excluded) | At 390, document scroll width is 485px on all three routes; at 320 it is 433px. Footer columns visibly collide in default screenshots. | This baseline overflow exists independently of injected text; unbroken probes add much larger overflow. | Record as an existing shell issue, not permission to refactor the shared footer or theme in this work item. Future layout validation must distinguish shell overflow from editable-slot regressions. |

**Recommendation for human review:** retain the proposed field set and limits
only together with explicit authorization for the narrowly scoped text-containment
work above in 07–09. Administrators would still control wording, not layout. If
that work is not accepted, keep the affected slots/limits unresolved rather than
claiming the existing layout safely supports arbitrary valid input. The reviewed
spaced samples are illustrative, not exhaustive: long Danish compounds, many LF
line breaks, and worst-case letter widths require repeat checks after integration.
This recommendation was accepted in the approval record below; implementation
and repeat layout validation remain in 07–09, not 02.

## Approval record

- Inventory and baseline: **approved**.
- Human reviewer/date: **2026-11-24**.
- Accepted changes: **all**; all decisions here approved.
