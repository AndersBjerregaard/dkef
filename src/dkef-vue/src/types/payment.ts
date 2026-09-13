export interface NexiCheckoutSessionDto {
  eventId: string
  paymentId: string
  amountMinor: number
  currency: string
  checkoutKey: string
  checkoutJsUrl: string
  language: string
}
