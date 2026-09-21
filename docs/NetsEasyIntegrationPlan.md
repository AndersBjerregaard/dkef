# Nets Easy Integration Plan and Handoff

## Purpose

This document is the persistent handoff and execution state for integrating Nets Easy checkout into DKEF event sign-up flows.

Use this file as the source of truth when continuing work in new agent sessions that do not have prior chat context.

## Goal

Make event sign-up payment-aware:

- Free events: keep current direct sign-up flow.
- Paid events: require completed Nets Easy checkout before sign-up is created.

## Core Decisions

- Capture strategy: autocapture. User is charged at checkout completion.
- Test environment behavior: do not require webhook integration during development.
- Live environment behavior: webhook events are the source of truth for payment state.
- Monetary storage strategy: store event sign-up price as integer minor units (ore), nullable.
- Pricing UX strategy: admins input Danish kroner in the UI and frontend converts to ore (minor units) before sending to backend/Nets.
- `parseKronerInputToMinor` behavior: trims spaces, accepts Danish decimal comma, removes thousand separators, and only accepts whole-kroner amounts (`500`, `500,00` valid; `500,50` invalid).

## Nets Easy Documentation

- Nets Easy official documentation for Checkout JS SDK can be found at `docs/CheckoutJSSDK.md`
- Nets Easy official documentation for Payment API can be found at `docs/PaymentAPI.md`

## Current System Snapshot

- Existing Nets POC route in frontend: `/payment`.
- Existing POC backend session endpoint: `POST /payments/nexi/poc-session`.
- Existing event sign-up endpoint: `POST /contents/events/{id}/sign-ups` (creates sign-up immediately).
- No price field on events existed before this integration work.

## Rollout Plan

### Step 1 - Event price groundwork (no checkout gating yet)

Scope:

- Add optional event sign-up price field in backend domain model and DTO.
- Add persistence migration.
- Add admin UI fields for create/edit event price.
- Display event price in event details.
- Do not change sign-up behavior yet.

Status: Verified by user.

### Step 2 - Paid sign-up session endpoint

Scope:

- Add endpoint to initialize a Nets payment for a specific paid event and user.
- Keep free-event path unchanged.
- Add local payment tracking record for correlation and idempotency.

Status: Verified by user.

### Step 3 - Frontend paid flow from event page

Scope:

- Branch sign-up button behavior by event price.
- Free events call existing sign-up endpoint.
- Paid events launch checkout flow tied to event-specific session.

Status: Verified by user.

### Step 4 - Payment confirmation and sign-up creation

Scope:

- Add confirm endpoint after checkout completion.
- Verify payment server-to-server before creating sign-up.
- Ensure idempotent confirmation handling.

Status: Verified by user.

### Step 5 - Webhook and production reliability

Scope:

- Add webhook receiver for live environment.
- Treat webhook state as source of truth in live.
- Add reconciliation visibility for admins.

Status: Implemented in code. Awaiting manual API verification.

## Implementation State Tracker

Use these checkboxes to track done work over time.

### Step 1 checklist

- [x] Add `SignUpPriceMinor` to event domain model.
- [x] Add `SignUpPriceMinor` to event DTO with non-negative validation.
- [x] Add AutoMapper mapping for the field.
- [x] Add and apply EF Core migration for `ContentsContext`.
- [x] Add admin create-event input for optional price in kroner and convert to ore in payload.
- [x] Add admin edit-event input for optional price in kroner and convert to ore in payload.
- [x] Include price in event create/update payloads.
- [x] Show price label on event detail page (`Gratis` when null or zero).
- [x] Run and pass backend build verification.
- [x] Run and pass frontend build/type-check verification.
- [x] Manual UI verification by user.

### Step 2 checklist

- [x] Add authenticated endpoint `POST /payments/nexi/events/{eventId}/session`.
- [x] Keep free-event sign-up path unchanged (`POST /contents/events/{id}/sign-ups`).
- [x] Guard paid-session endpoint for non-paid events (returns bad request for free events).
- [x] Reuse pending payment session for same user/event/amount/currency when available.
- [x] Create Nexi payment session for paid event and return checkout session DTO.
- [x] Persist local payment tracking record (`EventSignUpPayments`) with `Pending` status.
- [x] Add EF Core migration for payment tracking table in `ContentsContext`.
- [x] Run and pass backend build verification.
- [x] Run and pass frontend build/type-check verification (DTO contract alignment).
- [x] Manual API verification by user (happy path + free-event guard + already-signed-up conflict).

### Step 3 checklist

- [x] Branch sign-up action by event price on `SpecificEventView`.
- [x] Keep free events on existing direct sign-up endpoint.
- [x] Use paid session endpoint `POST /payments/nexi/events/{eventId}/session` for paid events.
- [x] Embed Nets Checkout JS SDK in event page for paid events.
- [x] Listen for `payment-completed` event and surface completion feedback.
- [x] Preserve auth-first behavior (prompt login before signup/payment).
- [x] Run and pass frontend build/type-check verification.
- [x] Manual UI verification by user (free + paid flows).

### Step 4 checklist

- [x] Add authenticated confirm endpoint `POST /payments/nexi/events/{eventId}/confirm`.
- [x] Verify payment server-to-server via Nexi `GET /v1/payments/{paymentId}`.
- [x] Validate amount/currency against local payment record before sign-up creation.
- [x] Create event sign-up only after verified paid state.
- [x] Handle idempotency when sign-up already exists (return existing sign-up response).
- [x] Update local payment tracking status (`Pending` -> `Completed`/`Failed`).
- [x] Wire frontend `payment-completed` callback to call confirm endpoint.
- [x] Run and pass backend build verification.
- [x] Run and pass frontend build/type-check verification.
- [x] Manual verification by user (paid happy path + retry/idempotency path + non-paid mismatch guard).

### Step 5 checklist

- [x] Add webhook subscriptions on event payment creation request (`notifications`).
- [x] Add anonymous webhook receiver endpoint `POST /payments/nexi/webhooks`.
- [x] Require exact `Authorization` header match when `NexiCheckout:WebhookAuthorization` is configured.
- [x] Parse webhook envelope (`event`, `data.paymentId`, optional `data.amount`).
- [x] Handle at-least-once delivery safely via idempotent update logic.
- [x] Mark local payment `Completed` for positive payment webhook events.
- [x] Ensure sign-up is created from webhook completion path when missing.
- [x] Mark local payment `Failed` for failure/cancel webhook events.
- [x] Add admin reconciliation endpoint `GET /payments/nexi/events/reconciliation`.
- [x] Add config keys for webhook URL and authorization token.
- [x] Run and pass backend build verification.
- [ ] Manual verification by user (live-like webhook calls + reconciliation query).

## Step 2 Implementation Notes

- New endpoint: `POST /payments/nexi/events/{eventId}/session` (requires authenticated user).
- Validation before creating/reusing payment:
  - event exists,
  - event is not started,
  - sign-up deadline not passed,
  - event has paid price (`SignUpPriceMinor > 0`),
  - user is not already signed up.
- Local payment tracking model/table:
  - `EventSignUpPayments` with fields: `EventId`, `ContactId`, `PaymentId`, `AmountMinor`, `Currency`, `Status`, `CreatedAt`, `UpdatedAt`.
  - unique index on `PaymentId`.
  - query index on `EventId + ContactId + Status`.
- Idempotency/correlation behavior:
  - if existing pending local payment exists for same user/event/amount/currency, endpoint returns that session data instead of creating a new Nexi payment.
  - otherwise creates new payment at Nexi and stores local `Pending` row.

## Step 3 Implementation Notes

- Frontend route/component touched: `src/dkef-vue/src/views/SpecificEventView.vue`.
- Flow branching:
  - free events: unchanged direct `signUpForEvent` behavior.
  - paid events: call paid-session endpoint and render embedded checkout in-page.
- Nets SDK usage follows docs in `docs/CheckoutJSSDK.md`:
  - initialize `new Dibs.Checkout(...)` with `checkoutKey`, `paymentId`, `containerId`, and `language`.
  - listen to `payment-completed` for user feedback.
  - cleanup checkout listeners on component unmount.
- URL service and store additions:
  - `urlservice.getNexiEventSession(guid)` added.
  - `eventStore.createPaidEventCheckoutSession(id)` added.
- Limitation (expected for Step 3): payment completion currently only shows success feedback; final sign-up creation is deferred to Step 4 confirm flow.

## Step 4 Implementation Notes

- Backend endpoint added: `POST /payments/nexi/events/{eventId}/confirm`.
- Request payload: `{ paymentId: string }`.
- Confirm flow behavior:
  - loads local `EventSignUpPayment` for `eventId + contactId + paymentId`,
  - calls Nexi Payment API `GET /v1/payments/{paymentId}` server-to-server,
  - validates `orderDetails.amount` and `orderDetails.currency` against local payment record,
  - accepts payment as completed when `summary.chargedAmount` or `summary.reservedAmount` covers expected amount,
  - creates `EventSignUp` and marks local payment `Completed`.
- Idempotency behavior:
  - if sign-up already exists, confirm returns existing sign-up response and marks payment `Completed`.
  - duplicate create race is caught and converted to same successful existing-sign-up response.
- Frontend update:
  - `SpecificEventView` now calls confirm endpoint inside `payment-completed` handler,
  - UI now only shows final success once confirm succeeds,
  - includes explicit "confirming payment" loading state and error feedback.

## Step 5 Implementation Notes

- Payment creation now includes webhook subscriptions when `NexiCheckout:WebhookUrl` is configured.
  - Subscribed events:
    - `payment.reservation.created.v2`
    - `payment.charge.created.v2`
    - `payment.reservation.failed`
    - `payment.cancel.created.v2`
    - `payment.checkout.completed`
- Webhook receiver added: `POST /payments/nexi/webhooks`.
  - Endpoint is anonymous and returns `200 OK` for processed/ignored payloads to satisfy Nexi retry semantics.
  - If `NexiCheckout:WebhookAuthorization` is set, incoming `Authorization` header must match exactly.
- Webhook processing behavior:
  - parses envelope fields `event` + `data.paymentId` (+ optional `data.amount.amount` and `data.amount.currency`),
  - locates local `EventSignUpPayment` by `paymentId`,
  - for completed events, marks payment `Completed` and ensures `EventSignUp` exists,
  - for failed/cancel events, marks payment `Failed` unless already `Completed`,
  - logs mismatches on amount/currency but does not hard-fail webhook processing.
- Admin reconciliation endpoint added:
  - `GET /payments/nexi/events/reconciliation?take=50&skip=0&status=Pending|Completed|Failed`
  - role requirement: `Admin`
  - returns `DomainCollection<EventPaymentReconciliationItemDto>` with event/payment status visibility.
- New configuration keys (under `NexiCheckout`):
  - `WebhookUrl` (empty disables webhook subscription payloads)
  - `WebhookAuthorization` (optional shared secret echoed in webhook `Authorization` header)

## Known Quirks and Constraints

- Frontend uses Bun, not npm/yarn.
- Frontend user-facing text must remain Danish.
- `src/dkef-vue/src/services/urlservice.ts` has limited Development-mode mocks; API-backed flows need non-development mode.
- Backend startup runs automatic migrations for multiple contexts.
- New migrations for contents must be created with explicit context and output directory.

## Environment Notes for Nets

- Test API base URL currently configured in backend app settings.
- Checkout JS URL and keys are provided through `NexiCheckout` config.
- For local development, keys can be overridden in `appsettings.Local.json`.

## Open Questions

- None at this step.
