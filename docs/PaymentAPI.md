Payment API
===========

The Easy Payment API provides methods for managing one-time payments and subscriptions (recurring payments).  
  
Payment objects are the main entities that the Easy platform centers around. Whenever a customer initiates either a one-time purchase or a subscription, a new payment object is created. See the [Create payment](#v1-payments-post) method for more details.  
  
A payment object is always associated with the following entities:

*   **Merchant** - the webshop that sells products. You provide the merchant identity by using the integration keys associated with your merchant account. Or, if you are a Nexi Group partner and use the keys belonging to a partner account, you can provide an optional `merchantNumber` to identify the merchant.
*   **Customer** - a private or a business consumer that places the order. Checkout enables customers to collect customer data if the customer's consent is obtained. This data makes future purchases easier. Checkout collects customer data directly from the customer. However, it is also possible for you to provide customer data that will initiate consumer information for the payment.
*   **Order** - defines what the customer will be charged for. The order is always provided by you when [creating a payment object](#v1-payments-post) and can later be updated during the checkout using the [update order method](#v1-payments-paymentid-orderitems-put).

  
A payment object also contains information about the checkout, such as **shipping options**, **payment methods**, and **currencies**.  
  
You can track the **status changes** of a payment by using [webhooks](#webhooks). The events that can be subscribed to roughly correspond to the different states you can find in the [payment section in Easy Portal](https://portal.dibspayment.eu/payments). If you are new to Easy, we recommend spending some time in the Easy Portal to familiarize yourself with the platform and what to expect from the API.

Scroll down for code samples, example requests and responses.  
Select a language for code samples from the tabs or the mobile navigation menu.  

Payments
--------

The methods listed under this section handle a single payment object. [Create a new payment](#v1-payments-post) object whenever your customer places a new order. This will reserve the amount specified in the order. The payment object can be updated during the checkout using the methods: [Update reference information](#v1) and [Update order](#v1).  
  
When you ship the order, you should [charge the payment](#v1-payments). And if you need, the API also allows you to [cancel a payment](#v1) or [refund a customer](#v1-payments-refund-post). \\n\\n **Important note:** By default, our system does not automatically charge/capture the amount which has been reserved. Depending on your business model, you should decide on the way how to handle this process. It is our recommendation to do the setup for immediate charge/capture.

### Create payment

`POST /v1/payments`

Initializes a new payment object that becomes the object used throughout the checkout flow for a particular customer and order. Creating a payment object is the first step when you intend to accept a payment from your customer. Entering the amount 100 corresponds to 1 unit of the currency entered, such as e.g. 1 NOK. Typically you provide the following information:

*   The **order details** including order items, total amount, and currency.
*   **Checkout page settings**, which specify what type of integration you want: a checkout page **embedded** on your site or a pre-built checkout page **hosted** by Nexi Group. You can also specify data about your customer so that your customer only needs to provide payment details on the checkout page.

Optionally, you can also provide information regarding:

*   **Notifications** if you want to be notified through **webhooks** when the status of the payment changes.
*   **Fees** added when using **payment methods** such as invoice.
*   **Charge** set to true so you can enable autocapture for **subscriptions**.

On success, this method returns a `paymentId` that can be used in subsequent requests to refer to the newly created payment object. Optionally, the response object will also contain a `hostedPaymentPageUrl`, which is the URL you should redirect to if using a hosted pre-built checkout page.

**Checkout session lifetime:** A checkout session is valid for **48 hours** by default. You can optionally provide `checkout.expiresAt` to set a custom UTC expiration. If the customer has not completed the payment before the returned expiration, you must create a new payment and redirect the customer to the new checkout URL.

#### Parameters

*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   orderobjectrequired
    
    Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
    
    *   itemsarrayrequired
        
        A list of order items. At least one item must be specified.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    *   amountinteger (int32)required
        
        The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
        
    *   currencystringrequired
        
        The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
        
    *   referencestringoptional
        
        A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    
*   checkoutobjectrequired
    
    Defines the behavior and style of the checkout page.
    
    *   urlstringoptional
        
        Specifies where the checkout will be loaded if using an embedded checkout page. See also the `integrationType` property. The maximum length is 256 characters. The following special characters are not supported: `<,>,’,”,\\`
        
    *   integrationTypestringoptional
        
        Determines whether the checkout should be embedded in your webshop or if the checkout should be hosted by Nexi Group on a separate page. Valid values are: `'EmbeddedCheckout'` (default), or `'HostedPaymentPage'`. Please note that the string values are **case sensitive**.
        
    *   returnUrlstringoptional
        
        Specifies where your customer will return after a completed payment when using a hosted checkout page. See also the `integrationType` property.
        
    *   cancelUrlstringoptional
        
        Specifies where your customer will return after a canceled payment when using a hosted checkout page. See also the `integrationType` property.
        
    *   consumerobjectoptional
        
        Contains information about the customer. If provided, this information will be used for initiating the consumer data of the payment object. See also the property `merchantHandlesConsumerData` which controls what fields to show on the checkout page.
        
        *   referencestringoptional
            
            The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &,  
            ///
            
        *   emailstringoptional
            
            The email address.
            
        *   shippingAddressobjectoptional
            
            *   addressLine1stringrequired
                
                The primary address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   addressLine2stringoptional
                
                An additional address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   postalCodestringrequired
                
                The postal code. Postal codes per each country: **NOR, NO** - A four-digit code, for example, 0025. **SWE, SE** - A five-digit code, for example, 11455. **DNK, DK** - A four-digit code, for example, 2600. **Other** - Must be between 1 and 12 characters, the following special characters are not supported: <, >, ', ", &, \\
                
            *   citystringrequired
                
                The city. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   countrystringrequired
                
                A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   billingAddressobjectoptional
            
            *   addressLine1stringrequired
                
                The primary address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   addressLine2stringoptional
                
                An additional address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   postalCodestringrequired
                
                The postal code. Postal codes per each country: **NOR, NO** - A four-digit code, for example, 0025. **SWE, SE** - A five-digit code, for example, 11455. **DNK, DK** - A four-digit code, for example, 2600. **Other** - Must be between 1 and 12 characters, the following special characters are not supported: <, >, ', ", &, \\
                
            *   citystringrequired
                
                The city. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   countrystringrequired
                
                A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   phoneNumberobjectoptional
            
            An international phone number.
            
            *   prefixstringoptional
                
                The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                
            *   numberstringoptional
                
                The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                
            
        *   privatePersonobjectoptional
            
            The name of a natural person.
            
            *   firstNamestringoptional
                
                The first name (also known as given name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   lastNamestringoptional
                
                The last name (also known as surname/family name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   companyobjectoptional
            
            A business consumer.
            
            *   namestringoptional
                
                The name of the company. Must be between 1 and 128 characters.
                
            *   contactobjectoptional
                
                The name of a natural person.
                
                *   firstNamestringoptional
                    
                    The first name (also known as given name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                *   lastNamestringoptional
                    
                    The last name (also known as surname/family name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                
            
        
    *   termsUrlstringrequired
        
        The URL to the terms and conditions of your webshop. The following special characters are not supported: `<,>,’,”,\\`
        
    *   merchantTermsUrlstringoptional
        
        The URL to the privacy and cookie settings of your webshop. The following special characters are not supported: `<,>,’,”,\\`
        
    *   shippingCountriesarrayoptional
        
        An array of countries that limits the set of countries available for shipping. If left unspecified, [all countries supported by Easy Checkout](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes) will be available for shipping on the checkout page.
        
        *   countryCodestringoptional
            
            A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). Important: For Klarna payments, the `countryCode` field is mandatory. If not provided, Klarna will not be available as a payment method. The following special characters are not supported: <, >, ', ", &, \\
            
        
    *   shippingobjectoptional
        
        *   countriesarrayoptional
            
            *   countryCodestringoptional
                
                A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). Important: For Klarna payments, the `countryCode` field is mandatory. If not provided, Klarna will not be available as a payment method. The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   merchantHandlesShippingCostbooleanoptional
            
            If set to `true`, the payment order is required to be updated (using the [Update order](#v1-payments-paymentid-orderitems-put) method) with `shipping.costSpecified` set to `true` before the customer can complete a purchase. Defaults to `false` if not specified.
            
        *   enableBillingAddressbooleanoptional
            
            If set to `true`, the customer is provided an option to specify separate addresses for billing and shipping on the checkout page. If set to `false`, the billing address is used as the shipping address.
            
        
    *   consumerTypeobjectoptional
        
        Configures which consumer types should be accepted. Defaults to 'B2C'.
        
        These options are ignore if the property `merchantHandlesConsumerData` is set to `true`.
        
        *   defaultstringoptional
            
            The checkout form defaults to this consumer type when first loaded.
            
        *   supportedTypesarrayoptional
            
            The array of consumer types that should be supported on the checkout page. Allowed values are: 'B2B' and 'B2C'.
            
        
    *   chargebooleanoptional
        
        If set to `true`, the transaction will be charged automatically after the reservation has been accepted. Default value is `false` if not specified.
        
    *   publicDevicebooleanoptional
        
        If set to `true`, the checkout will not load any user data, and also the checkout will not remember the current consumer on this device. Default value is `false` if not specified.
        
    *   merchantHandlesConsumerDatabooleanoptional
        
        Allows you to initiate the checkout with customer data so that your customer only need to provide payment details. It is possible to exclude all consumer and company information from the payment (only for certain payment methods) when it is set to true. If you still want to add consumer information to the payment you need to use the `consumer` object (either a `privatePerson` or a `company`, not both).
        
    *   appearanceobjectoptional
        
        Defines the appearance of the checkout page.
        
        *   displayOptionsobjectoptional
            
            Controls what is displayed on the checkout page.
            
            *   showMerchantNamebooleanoptional
                
                If set to `true`, displays the merchant name above the checkout. Default value is `true` when using a `HostedPaymentPage`.
                
            *   showOrderSummarybooleanoptional
                
                If set to `true`, displays the order summary above the checkout. Default value is `true` when using a `HostedPaymentPage`.
                
            
        *   textOptionsobjectoptional
            
            Controls what text is displayed on the checkout page.
            
            *   completePaymentButtonTextstringoptional
                
                Overrides payment button text. The following predefined values are allowed: `pay`, `purchase`, `order`, `book`, `reserve`, `signup`, `subscribe`, `accept`. The payment button text is localized.
                
            
        
    *   countryCodestringoptional
        
        Merchant's three-letter checkout country code (ISO 3166-1), for example GBR. See also the [list of supported languages](/nexi-checkout/en-EU/api/#currency-and-amount). Important: For Klarna payments, the `countryCode` field is mandatory. If not provided, Klarna will not be available as a payment method. The following special characters are not supported: <, >, ', ", &, \\
        
    *   expiresAtstring (date-time)optional
        
        Optional UTC date and time when the checkout expires, up to 45 days after creation. If omitted, the checkout expires 48 hours after creation.
        
    
*   merchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   subscriptionobjectoptional
    
    Defines the duration and interval when creating or updating a [subscription](#subscriptions).
    
    *   subscriptionIdstring (uuid)optional
        
        The identifier of the subscription to be updated. If omitted, a new subscription will be created.
        
    *   endDatestring (date-time)optional
        
        The date and time when the subscription expires. It is not possible to charge this subscription after this date. The field has three components: date, time, and time zone (offset from GMT). For example: 2021-07-02T12:00:00.0000+02:00
        
    *   intervalinteger (int32)optional
        
        Defines the minimum number of days between each recurring charge. This interval commences from either the day the subscription was created or the most recent subscription charge, whichever is later. An interval value of 0 means that there are no payment interval restrictions.
        
    *   allowVariableAmountbooleanoptional
        
        Allow variable amount for the subscription.
        
    
*   unscheduledSubscriptionobjectoptional
    
    Defines the payment as one that should initiate or update an unscheduled card on file agreement
    
    *   createbooleanoptional
        
        A flag indicating if a new unscheduled card on file agreement should be created. Can be omitted when updating an existing unscheduled card on file agreement.
        
    *   unscheduledSubscriptionIdstring (uuid)optional
        
        The identifier of the unscheduled card on file agreement to be updated. If omitted, a new unscheduled card on file agreement will be created.
        
    
*   paymentMethodsConfigurationarrayoptional
    
    Specifies payment methods configuration to be used for this payment, ignored if empty or null. All available and configured payment methods are enabled by default.
    
    *   namestringoptional
        
        The name of the payment method or payment type to be configured for payment. If the specified payment method is not configured correctly in the merchant configurations then this won't take effect. Payment type cannot be specified alongside payment methods that belong to it, if it happens the request will fail with an error. Possible payment methods values: "Visa", "MasterCard", "Dankort", "AmericanExpress", "Forbrugsforeningen", "PayPal", "Vipps", "MobilePay", "Swish", "Arvato", "EasyInvoice", "EasyInstallment", "EasyCampaign", "RatePayInvoice", "RatePayInstallment", "RatePaySepa", "Sofort", "Trustly", "ApplePay", "Klarna", "GooglePay". Possible payment types values: "Card", "Invoice", "Installment", "A2A", "Wallet".
        
    *   enabledbooleanoptional
        
        Indicates that the specified payment method/type is allowed to be used for this payment, defaults to true. If one or more payment method/type is configured in the parent array then this value will be considered false for any other payment method that the parent array doesn't cover.
        
    
*   paymentMethodsarrayoptional
    
    *   namestringoptional
        
        The name of the payment method. Possible value currently is: 'easy-invoice'.
        
    *   feeobjectoptional
        
        Represents a line of a customer order. An order item refers to a product that the customer has bought. A product can be anything from a physical product to an online subscription or shipping.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    
*   myReferencestringoptional
    
    Merchant payment reference The maximum length is 36 characters. The following special characters are not supported: <, >, ', ", &, \\
    
*   additionalPaymentMethodDataobjectoptional
    
    *   rivertyobjectoptional
        
        *   orderReferencestringoptional
            
            Optional order reference that is later mapped to the parentTransactionReference in Riverty. It must be unique across all payments and have a length less than 128 characters.
            
        
    *   payPalobjectoptional
        
        *   orderReferencestringoptional
            
            Order reference that is later mapped to the InvoiceId in PayPal. If not provided we will generate it ourselves. It must be unique across all payments and have a length less than 128 characters.
            
        
    

### Request body

    {
        "order": {
            "items": [
                {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            ],
            "amount": 0,
            "currency": "string",
            "reference": "string"
        },
        "checkout": {
            "url": "string",
            "integrationType": "string",
            "returnUrl": "string",
            "cancelUrl": "string",
            "consumer": {
                "reference": "string",
                "email": "string",
                "shippingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string"
                },
                "billingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string"
                },
                "phoneNumber": {
                    "prefix": "string",
                    "number": "string"
                },
                "privatePerson": {
                    "firstName": "string",
                    "lastName": "string"
                },
                "company": {
                    "name": "string",
                    "contact": {
                        "firstName": "string",
                        "lastName": "string"
                    }
                }
            },
            "termsUrl": "string",
            "merchantTermsUrl": "string",
            "shippingCountries": [
                {
                    "countryCode": "string"
                }
            ],
            "shipping": {
                "countries": [
                    {
                        "countryCode": "string"
                    }
                ],
                "merchantHandlesShippingCost": true,
                "enableBillingAddress": true
            },
            "consumerType": {
                "default": "string",
                "supportedTypes": [
                    "string"
                ]
            },
            "charge": true,
            "publicDevice": true,
            "merchantHandlesConsumerData": true,
            "appearance": {
                "displayOptions": {
                    "showMerchantName": true,
                    "showOrderSummary": true
                },
                "textOptions": {
                    "completePaymentButtonText": "string"
                }
            },
            "countryCode": "string",
            "expiresAt": "2019-08-24T14:15:22Z"
        },
        "merchantNumber": "string",
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "subscription": {
            "subscriptionId": "d079718b-ff63-45dd-947b-4950c023750f",
            "endDate": "2019-08-24T14:15:22Z",
            "interval": 0,
            "allowVariableAmount": true
        },
        "unscheduledSubscription": {
            "create": true,
            "unscheduledSubscriptionId": "92143051-9e78-40af-a01f-245ccdcd9c03"
        },
        "paymentMethodsConfiguration": [
            {
                "name": "string",
                "enabled": true
            }
        ],
        "paymentMethods": [
            {
                "name": "string",
                "fee": {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            }
        ],
        "myReference": "string",
        "additionalPaymentMethodData": {
            "riverty": {
                "orderReference": "string"
            },
            "payPal": {
                "orderReference": "string"
            }
        }
    }

#### Responses

*   201Createdoptional
    
    *   paymentIdstringrequired
        
        The identifier (UUID) of the newly created payment object. Use this identifier in subsequent requests when referring to the new payment. The checkout session expiration is provided in Nexi.Checkout.PaymentApi.Contracts.V1.CreatePayment.CreatePaymentResponse.ExpiresAt.
        
    *   expiresAtstring (date-time)optional
        
        The UTC date and time when the checkout expires.
        
    *   hostedPaymentPageUrlstringoptional
        
        The URL your website should redirect to if using a hosted pre-built checkout page.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   201
*   400
*   500

    {
        "paymentId": "string",
        "expiresAt": "2019-08-24T14:15:22Z",
        "hostedPaymentPageUrl": "string"
    }

### Retrieve payment

`GET /v1/payments/{paymentId}`

Retrieves the details of an existing payment. The \`paymentId\` is obtained from Nexi Group when \[creating a payment object\](#create-payment).

Rate Limiting
-------------

This endpoint is subject to rate limiting. Merchants are limited to 30 calls to retrieve a given payment per hour. Exceeding this limit will result in throttling, and further requests will be denied until the rate limit window resets.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Responses

*   200OKoptional
    
    *   paymentobjectoptional
        
        Contains all information about a payment.
        
        *   paymentIdstring (uuid)required
            
            The payment identifier (a UUID).
            
            example: 4cfe36094bc548fa961ab9a93754138b
        *   summaryobjectoptional
            
            Summarizes the reserved, charged, refunded, and canceled amounts associated with a payment.
            
            *   reservedAmountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) that has been reserved in the customer's bank account at the time of the purchase to make sure there are sufficient funds to charge the payment. See also the [Create payment](#v1-payments-post) method.
                
            *   reservedSurchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) that has been reserved on top of the base amount.
                
            *   chargedAmountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) that has been charged. See also the [Charge payment](#v1-payments-paymentid-charges-post) method.
                
            *   chargedSurchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) that has been charged on top of the base amount.
                
            *   refundedAmountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) that has been refunded. See also the [Refund payment](#v1-payments-paymentid-refunds-post) method.
                
            *   refundedSurchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) that has been refunded on top of the base amount.
                
            *   cancelledAmountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) that has been cancelled. See also the [Cancel payment](#v1-payments-paymentid-cancels-post) method.
                
            *   cancelledSurchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) that has been cancelled on top of the base amount.
                
            
        *   consumerobjectoptional
            
            *   shippingAddressobjectoptional
                
                *   addressLine1stringoptional
                    
                    The primary address line.
                    
                *   addressLine2stringoptional
                    
                    An additional address line.
                    
                *   receiverLinestringoptional
                    
                    The name (or company name) of the customer.
                    
                *   postalCodestringoptional
                    
                    The postal code.
                    
                *   citystringoptional
                    
                    The city.
                    
                *   countrystringoptional
                    
                    A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported languages](../api-overview).
                    
                *   phoneNumberobjectoptional
                    
                    An international phone number.
                    
                    *   prefixstringoptional
                        
                        The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                        
                    *   numberstringoptional
                        
                        The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                        
                    
                
            *   companyobjectoptional
                
                *   merchantReferencestringoptional
                *   namestringoptional
                    
                    The company name.
                    
                *   registrationNumberstringoptional
                *   contactDetailsobjectoptional
                    
                    Information about the contact person for a company.
                    
                    *   firstNamestringoptional
                        
                        The first name (also known as given name).
                        
                    *   lastNamestringoptional
                        
                        The last name (also known as surname/family name).
                        
                    *   emailstringoptional
                        
                        The email address.
                        
                    *   phoneNumberobjectoptional
                        
                        An international phone number.
                        
                        *   prefixstringoptional
                            
                            The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                            
                        *   numberstringoptional
                            
                            The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                            
                        
                    
                
            *   privatePersonobjectoptional
                
                *   merchantReferencestringoptional
                *   dateOfBirthstring (date-time)optional
                    
                    The date on which the customer was born.
                    
                *   firstNamestringoptional
                    
                    The first name (also known as given name).
                    
                *   lastNamestringoptional
                    
                    The last name (also known as surname/family name).
                    
                *   emailstringoptional
                    
                    The email address.
                    
                *   phoneNumberobjectoptional
                    
                    An international phone number.
                    
                    *   prefixstringoptional
                        
                        The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                        
                    *   numberstringoptional
                        
                        The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                        
                    
                
            *   billingAddressobjectoptional
                
                *   addressLine1stringoptional
                    
                    The primary address line.
                    
                *   addressLine2stringoptional
                    
                    An additional address line.
                    
                *   receiverLinestringoptional
                    
                    The name (or company name) of the customer.
                    
                *   postalCodestringoptional
                    
                    The postal code.
                    
                *   citystringoptional
                    
                    The city.
                    
                *   countrystringoptional
                    
                    A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported languages](../api-overview).
                    
                *   phoneNumberobjectoptional
                    
                    An international phone number.
                    
                    *   prefixstringoptional
                        
                        The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                        
                    *   numberstringoptional
                        
                        The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                        
                    
                
            
        *   paymentDetailsobjectoptional
            
            *   paymentTypestringoptional
                
                The type of payment. Possible values are: 'CARD', 'INVOICE', 'A2A', 'INSTALLMENT', 'WALLET', and 'PREPAID-INVOICE'.
                
            *   paymentMethodstringoptional
                
                The payment method, for example Visa or Mastercard.
                
            *   invoiceDetailsobjectoptional
                
                *   invoiceNumberstringoptional
                
            *   cardDetailsobjectoptional
                
                *   maskedPanstringoptional
                    
                    A masked version of the PAN (Primary Account Number). At maximum, only the first six and last four digits of the account number are displayed.
                    
                *   expiryDatestringoptional
                    
                    The four-digit expiration date of the payment card. The format should be: MMYY.
                    
                
            
        *   orderDetailsobjectrequired
            
            *   amountinteger (int32)required
                
                The total base [amount](../#currency-and-amount) of the order, for example 10000. Must be higher than 0.
                
            *   currencystringrequired
                
                The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
                
            *   referencestringoptional
                
                The reference to recognize this order. Usually a number sequence provided when [creating](#v1-payments-create-payment-post) or [updating](#v1-update-order-put) the payment.
                
            
        *   checkoutobjectrequired
            
            *   urlstringrequired
                
                The URL to the hosted or embedded checkout page.
                
            *   cancelUrlstringoptional
                
                The URL to the page responsible for handling a canceled checkout.
                
            
        *   createdstring (date-time)required
            
            The date and time when the payment was initiated.
            
        *   expiresAtstring (date-time)optional
            
            The UTC date and time when the checkout expires. Only present for payments created with expiration support.
            
        *   refundsarrayoptional
            
            An array of all the refunds associated with this payment.
            
            *   refundIdstring (uuid)optional
                
                A unique identifier of this refund.
                
                example: 48e579af8b6c4e59b00dde20907413e7
            *   amountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) of the refund.
                
            *   surchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) of the refund. It might not be populated until the operation is completed.
                
            *   statestringoptional
                
                The current state of the refund. Possible values are: 'Pending', 'Cancelled', 'Failed', 'Completed', 'Expired'.
                
            *   lastUpdatedstring (date-time)optional
                
                The date and time when the refund was last updated.
                
            *   orderItemsarrayoptional
                
                The list of returned and canceled order items that are associated with the refund. At least one order item is required.
                
                *   referencestringrequired
                    
                    A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                    
                *   namestringrequired
                    
                    The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                    
                *   quantitynumber (double)required
                    
                    The quantity of the product. The value can not be negative.
                    
                *   unitstringrequired
                    
                    The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                *   unitPriceinteger (int32)required
                    
                    The price per unit excluding VAT. Note: The amount can be negative.
                    
                *   taxRateinteger (int32)optional
                    
                    The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
                    
                *   taxAmountinteger (int32)optional
                    
                    The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
                    
                *   grossTotalAmountinteger (int32)required
                    
                    The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
                    
                *   netTotalAmountinteger (int32)required
                    
                    The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
                    
                *   imageUrlstringoptional
                    
                    Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
                    
                
            
        *   chargesarrayoptional
            
            *   chargeIdstring (uuid)optional
                
                A unique identifier of the charge.
                
                example: e63cd87c549a43168a5fd3f4cd37d413
            *   amountinteger (int32)optional
                
                The base [amount](../#currency-and-amount) of the charge.
                
            *   surchargeAmountinteger (int32)optional
                
                The surcharge [amount](../#currency-and-amount) of the charge. It might not be populated until the operation is completed.
                
            *   createdstring (date-time)optional
                
                The date and time when the charge was initiated.
                
            *   orderItemsarrayoptional
                
                The array of order items associated with the charge.
                
                *   referencestringrequired
                    
                    A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                    
                *   namestringrequired
                    
                    The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                    
                *   quantitynumber (double)required
                    
                    The quantity of the product. The value can not be negative.
                    
                *   unitstringrequired
                    
                    The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                *   unitPriceinteger (int32)required
                    
                    The price per unit excluding VAT. Note: The amount can be negative.
                    
                *   taxRateinteger (int32)optional
                    
                    The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
                    
                *   taxAmountinteger (int32)optional
                    
                    The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
                    
                *   grossTotalAmountinteger (int32)required
                    
                    The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
                    
                *   netTotalAmountinteger (int32)required
                    
                    The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
                    
                *   imageUrlstringoptional
                    
                    Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
                    
                
            
        *   terminatedstring (date-time)optional
            
            The date and time of termination. Only present if the payment has been terminated.
            
        *   subscriptionobjectoptional
            
            The subscription identifier.
            
            *   idstring (uuid)optional
                
                The subscription identifier (a UUID).
                
            
        *   unscheduledSubscriptionobjectoptional
            
            The unscheduled subscription identifier.
            
            *   unscheduledSubscriptionIdstring (uuid)optional
                
                The unscheduled subscription identifier (a UUID).
                
            
        *   myReferencestringoptional
            
            Merchant payment reference
            
        *   paymentAccountReferencestringoptional
            
            The merchant can use this field to recognize if the same cardholder is re-visiting.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   429Too Many Requestsoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "payment": {
            "paymentId": "4cfe36094bc548fa961ab9a93754138b",
            "summary": {
                "reservedAmount": 0,
                "reservedSurchargeAmount": 0,
                "chargedAmount": 0,
                "chargedSurchargeAmount": 0,
                "refundedAmount": 0,
                "refundedSurchargeAmount": 0,
                "cancelledAmount": 0,
                "cancelledSurchargeAmount": 0
            },
            "consumer": {
                "shippingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "receiverLine": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string",
                    "phoneNumber": {
                        "prefix": "string",
                        "number": "string"
                    }
                },
                "company": {
                    "merchantReference": "string",
                    "name": "string",
                    "registrationNumber": "string",
                    "contactDetails": {
                        "firstName": "string",
                        "lastName": "string",
                        "email": "string",
                        "phoneNumber": {
                            "prefix": "string",
                            "number": "string"
                        }
                    }
                },
                "privatePerson": {
                    "merchantReference": "string",
                    "dateOfBirth": "2019-08-24T14:15:22Z",
                    "firstName": "string",
                    "lastName": "string",
                    "email": "string",
                    "phoneNumber": {
                        "prefix": "string",
                        "number": "string"
                    }
                },
                "billingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "receiverLine": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string",
                    "phoneNumber": {
                        "prefix": "string",
                        "number": "string"
                    }
                }
            },
            "paymentDetails": {
                "paymentType": "string",
                "paymentMethod": "string",
                "invoiceDetails": {
                    "invoiceNumber": "string"
                },
                "cardDetails": {
                    "maskedPan": "string",
                    "expiryDate": "string"
                }
            },
            "orderDetails": {
                "amount": 0,
                "currency": "string",
                "reference": "string"
            },
            "checkout": {
                "url": "string",
                "cancelUrl": "string"
            },
            "created": "2019-08-24T14:15:22Z",
            "expiresAt": "2019-08-24T14:15:22Z",
            "refunds": [
                {
                    "refundId": "48e579af8b6c4e59b00dde20907413e7",
                    "amount": 0,
                    "surchargeAmount": 0,
                    "state": "string",
                    "lastUpdated": "2019-08-24T14:15:22Z",
                    "orderItems": [
                        {
                            "reference": "string",
                            "name": "string",
                            "quantity": 0.1,
                            "unit": "string",
                            "unitPrice": 0,
                            "taxRate": 0,
                            "taxAmount": 0,
                            "grossTotalAmount": 0,
                            "netTotalAmount": 0,
                            "imageUrl": "string"
                        }
                    ]
                }
            ],
            "charges": [
                {
                    "chargeId": "e63cd87c549a43168a5fd3f4cd37d413",
                    "amount": 0,
                    "surchargeAmount": 0,
                    "created": "2019-08-24T14:15:22Z",
                    "orderItems": [
                        {
                            "reference": "string",
                            "name": "string",
                            "quantity": 0.1,
                            "unit": "string",
                            "unitPrice": 0,
                            "taxRate": 0,
                            "taxAmount": 0,
                            "grossTotalAmount": 0,
                            "netTotalAmount": 0,
                            "imageUrl": "string"
                        }
                    ]
                }
            ],
            "terminated": "2019-08-24T14:15:22Z",
            "subscription": {
                "id": "497f6eca-6276-4993-bfeb-53cbbbba6f08"
            },
            "unscheduledSubscription": {
                "unscheduledSubscriptionId": "92143051-9e78-40af-a01f-245ccdcd9c03"
            },
            "myReference": "string",
            "paymentAccountReference": "string"
        }
    }

### Update reference information

`PUT /v1/payments/{paymentId}/referenceinformation`

Updates the specified payment object with a new `reference` string and a `checkoutUrl`.

If you instead want to update the **order** of a payment object, use the [Update order](#update-order-items) method.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   checkoutUrlstringrequired
*   referencestringrequired

### Request body

    {
        "checkoutUrl": "string",
        "reference": "string"
    }

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Update order

`PUT /v1/payments/{paymentId}/orderitems`

Updates the order for the specified payment. This endpoint makes it possible to change the order on the checkout page _after_ the payment object has been created. This is typically used when managing destination-based shipping costs at the checkout.

This endpoint can only be used as long as the checkout has not yet been completed by the customer. (See the [payment.checkout.completed](/nexi-checkout/en-EU/api/webhooks/#checkout-completed) event.)

#### Parameters

*   paymentIdstring (uuid)required
    
    The payment identifier.
    

#### Request body

Expand all

*   amountinteger (int32)optional
    
    The base [amount](../#currency-and-amount), for example 10000.
    
*   itemsarrayoptional
    
    The array of order items.
    
    *   referencestringrequired
        
        A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   namestringrequired
        
        The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   quantitynumber (double)required
        
        The quantity of the product. The value can not be negative.
        
    *   unitstringrequired
        
        The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    *   unitPriceinteger (int32)required
        
        The price per unit excluding VAT. Note: The amount can be negative.
        
    *   taxRateinteger (int32)optional
        
        The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
        
    *   taxAmountinteger (int32)optional
        
        The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
        
    *   grossTotalAmountinteger (int32)required
        
        The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
        
    *   netTotalAmountinteger (int32)required
        
        The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
        
    *   imageUrlstringoptional
        
        Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
        
    
*   shippingobjectoptional
    
    *   costSpecifiedbooleanoptional
    
*   paymentMethodsarrayoptional
    
    Specifies an array of invoice fees added to the total price when invoice is used as the payment method.
    
    *   namestringoptional
    *   feeobjectoptional
        
        Represents a line of a customer order. An order item refers to a product that the customer has bought. A product can be anything from a physical product to an online subscription or shipping.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    

### Request body

    {
        "amount": 0,
        "items": [
            {
                "reference": "string",
                "name": "string",
                "quantity": 0.1,
                "unit": "string",
                "unitPrice": 0,
                "taxRate": 0,
                "taxAmount": 0,
                "grossTotalAmount": 0,
                "netTotalAmount": 0,
                "imageUrl": "string"
            }
        ],
        "shipping": {
            "costSpecified": true
        },
        "paymentMethods": [
            {
                "name": "string",
                "fee": {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            }
        ]
    }

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Update myReference

`PUT /v1/payments/{paymentId}/myreference`

Updates myReference field on payment. The myReference can be used if you want to create a myReference ID that can be used in your own accounting system to keep track of the actions connected to the payment.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   myReferencestringoptional
    
    Merchant payment reference The maximum length is 36 characters. The following special characters are not supported: <, >, ', ", &, \\
    

### Request body

    {
        "myReference": "string"
    }

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Terminate payment

`PUT /v1/payments/{paymentId}/terminate`

Terminates an ongoing checkout session. A payment can only be terminated **before** the checkout has completed ([see the `payment.checkout` event](#webhooks)). Use this method to prevent a customer from having multiple open payment sessions simultaneously.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   405Method Not Allowedoptional
*   409Conflictoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Cancel payment

`POST /v1/payments/{paymentId}/cancels`

Important notes:

*   Both full and partial cancellations are supported.
*   Partial cancellations are only available for card payments and wallets.
*   Partial cancellations are temporarily unavailable in combination with partial refunds
*   If the \`amount\` does not match the total amount of the order, it is assumed that a partial cancellation is intended.
*   Once a payment has been (fully or partially) captured, it can no longer be fully canceled. Only the remaining uncaptured amount can be partially canceled.
*   After a payment is canceled, its status cannot be changed.
*   Nexi Group will not charge a fee for a canceled payment.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   amountinteger (int32)required
    
    The base [amount](../#currency-and-amount) to be cancelled.
    
*   orderItemsarrayoptional
    
    The order items to be canceled.
    
    Note! OrderItems must be provided if the partial cancel is intended. For a full cancellation, they can be omitted or provided in full.
    
    *   referencestringrequired
        
        A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   namestringrequired
        
        The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   quantitynumber (double)required
        
        The quantity of the product. The value can not be negative.
        
    *   unitstringrequired
        
        The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    *   unitPriceinteger (int32)required
        
        The price per unit excluding VAT. Note: The amount can be negative.
        
    *   taxRateinteger (int32)optional
        
        The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
        
    *   taxAmountinteger (int32)optional
        
        The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
        
    *   grossTotalAmountinteger (int32)required
        
        The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
        
    *   netTotalAmountinteger (int32)required
        
        The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
        
    *   imageUrlstringoptional
        
        Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
        
    

### Request body

    {
        "amount": 0,
        "orderItems": [
            {
                "reference": "string",
                "name": "string",
                "quantity": 0.1,
                "unit": "string",
                "unitPrice": 0,
                "taxRate": 0,
                "taxAmount": 0,
                "grossTotalAmount": 0,
                "netTotalAmount": 0,
                "imageUrl": "string"
            }
        ]
    }

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Charge payment

`POST /v1/payments/{paymentId}/charges`

Charges the specified payment. Charge a payment on the same day as you ship the matching order.A payment can be fully charged or partially charged:

*   **Full charge**: Your customer will be charged the total amount of the payment. The `amount` must be specified in the request body and is required to match the total amount of the payment.
*   **Partial charge**: Only charge for a subset of the order items. In this case you have to provide the `amount` and the `orderItems` you want to charge in the request body.

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier.
    
*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the charge you are attempting. Must be between 1 and 64 characters.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   amountinteger (int32)required
    
    The base [amount](../#currency-and-amount) to be charged.
    
*   orderItemsarrayoptional
    
    The order items list to charge for. Only required for partial charges.
    
    *   referencestringrequired
        
        A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   namestringrequired
        
        The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   quantitynumber (double)required
        
        The quantity of the product. The value can not be negative.
        
    *   unitstringrequired
        
        The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    *   unitPriceinteger (int32)required
        
        The price per unit excluding VAT. Note: The amount can be negative.
        
    *   taxRateinteger (int32)optional
        
        The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
        
    *   taxAmountinteger (int32)optional
        
        The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
        
    *   grossTotalAmountinteger (int32)required
        
        The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
        
    *   netTotalAmountinteger (int32)required
        
        The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
        
    *   imageUrlstringoptional
        
        Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
        
    
*   shippingobjectoptional
    
    *   trackingNumberstringoptional
        
        The maximum length is 255 characters.
        
    *   providerstringoptional
        
        The maximum length is 4 characters.
        
    
*   finalChargebooleanoptional
    
    Flag to release remaining reservation
    
*   myReferencestringoptional
    
    Merchant payment reference The maximum length is 36 characters. The following special characters are not supported: <, >, ', ", &, \\
    
*   paymentMethodReferencestringoptional
    
    An optional unique reference per payment method, its usage and restrictions are relevant to the payment method used in reserving the payment. Ignored if not specified, otherwise it gets passed to the payment method provider during capturing the payment. Currently, it affects the following payment methods:
    
    *   Riverty/AfterPay/Arvato: It signifies the Riverty invoice number which must be unique and have a maximum character limit of 20.
    

### Request body

    {
        "amount": 0,
        "orderItems": [
            {
                "reference": "string",
                "name": "string",
                "quantity": 0.1,
                "unit": "string",
                "unitPrice": 0,
                "taxRate": 0,
                "taxAmount": 0,
                "grossTotalAmount": 0,
                "netTotalAmount": 0,
                "imageUrl": "string"
            }
        ],
        "shipping": {
            "trackingNumber": "string",
            "provider": "string"
        },
        "finalCharge": true,
        "myReference": "string",
        "paymentMethodReference": "string"
    }

#### Responses

*   201Createdoptional
    
    *   chargeIdstringrequired
    *   invoiceobjectoptional
        
        *   invoiceNumberstringoptional
            
            The invoice number.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   402Payment Requiredoptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   201
*   400
*   402
*   500

    {
        "chargeId": "string",
        "invoice": {
            "invoiceNumber": "string"
        }
    }

### Retrieve charge

`GET /v1/charges/{chargeId}`

#### Parameters

*   chargeIdstringrequired
    
    The identifier of the existing charge (a UUID).
    

#### Responses

*   200OKoptional
    
    *   chargeIdstring (uuid)required
        
        The charge identifier (a UUID).
        
        example: fddeb94322904c12b2c80f43e14dba43
    *   amountinteger (int32)required
        
        The base [amount](../#currency-and-amount) of the charge.
        
    *   surchargeAmountinteger (int32)optional
        
        The surcharge [amount](../#currency-and-amount) of the charge. It might not be populated until the operation is completed.
        
    *   invoiceDetailsobjectoptional
        
        Information about a publicly accessible invoice.
        
        *   linkstringoptional
            
            The URL of an invoice that is publicly accessible.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "chargeId": "fddeb94322904c12b2c80f43e14dba43",
        "amount": 0,
        "surchargeAmount": 0,
        "invoiceDetails": {
            "link": "string"
        }
    }

### Refund charge

`POST /v1/charges/{chargeId}/refunds`

Refunds a previously settled transaction (a charged payment). The refunded amount will be transferred back to the customer's account. The required `chargeId` is returned from the [Charge payment method](#charge-payment)

A settled transaction can be fully or partially refunded:

*   Full refund requires only the `amount` to be specified in the request body.
*   Partial refund requires the `amount` and the `orderItems` to be refunded.

#### Parameters

*   chargeIdstringrequired
*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the refund you are attempting. Must be between 1 and 64 characters.
    

#### Request body

Expand all

*   amountinteger (int32)required
    
    The base [amount](../#currency-and-amount) to be refunded.
    
*   orderItemsarrayoptional
    
    *   referencestringrequired
        
        A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   namestringrequired
        
        The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   quantitynumber (double)required
        
        The quantity of the product. The value can not be negative.
        
    *   unitstringrequired
        
        The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    *   unitPriceinteger (int32)required
        
        The price per unit excluding VAT. Note: The amount can be negative.
        
    *   taxRateinteger (int32)optional
        
        The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
        
    *   taxAmountinteger (int32)optional
        
        The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
        
    *   grossTotalAmountinteger (int32)required
        
        The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
        
    *   netTotalAmountinteger (int32)required
        
        The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
        
    *   imageUrlstringoptional
        
        Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
        
    
*   myReferencestringoptional
    
    Merchant payment reference
    

### Request body

    {
        "amount": 0,
        "orderItems": [
            {
                "reference": "string",
                "name": "string",
                "quantity": 0.1,
                "unit": "string",
                "unitPrice": 0,
                "taxRate": 0,
                "taxAmount": 0,
                "grossTotalAmount": 0,
                "netTotalAmount": 0,
                "imageUrl": "string"
            }
        ],
        "myReference": "string"
    }

#### Responses

*   201Createdoptional
    
    *   refundIdstringrequired
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   201
*   400
*   500

    {
        "refundId": "string"
    }

### Refund payment

`POST /v1/payments/{paymentId}/refunds`

Refunds a previously settled payment. The refunded amount will be transferred back to the customer's account.

A settled payment can be fully or partially refunded. Full refund requires only the `amount` to be specified in the request body. Partial refund requires the `amount` and the `orderItems` to be refunded. This end-point is not supported for these payment methods:

*   Arvato
*   PayPal
*   RatePayInvoice
*   RatePaySepa
*   RatePayInstallment
*   EasyInvoice
*   EasyCampaign
*   EasyInstallment

#### Parameters

*   paymentIdstringrequired
*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the refund you are attempting. Must be between 1 and 64 characters.
    
*   CommercePlatformTagstringoptional
    
    An identifier of the ecommerce platform.
    

#### Request body

Expand all

*   amountinteger (int32)required
    
    The base [amount](../#currency-and-amount) to be refunded.
    
*   orderItemsarrayoptional
    
    *   referencestringrequired
        
        A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   namestringrequired
        
        The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
        
    *   quantitynumber (double)required
        
        The quantity of the product. The value can not be negative.
        
    *   unitstringrequired
        
        The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    *   unitPriceinteger (int32)required
        
        The price per unit excluding VAT. Note: The amount can be negative.
        
    *   taxRateinteger (int32)optional
        
        The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
        
    *   taxAmountinteger (int32)optional
        
        The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
        
    *   grossTotalAmountinteger (int32)required
        
        The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
        
    *   netTotalAmountinteger (int32)required
        
        The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
        
    *   imageUrlstringoptional
        
        Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
        
    
*   myReferencestringoptional
    
    Merchant payment reference
    

### Request body

    {
        "amount": 0,
        "orderItems": [
            {
                "reference": "string",
                "name": "string",
                "quantity": 0.1,
                "unit": "string",
                "unitPrice": 0,
                "taxRate": 0,
                "taxAmount": 0,
                "grossTotalAmount": 0,
                "netTotalAmount": 0,
                "imageUrl": "string"
            }
        ],
        "myReference": "string"
    }

#### Responses

*   201Createdoptional
    
    *   refundIdstringrequired
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   201
*   400
*   500

    {
        "refundId": "string"
    }

### Retrieve refund

`GET /v1/refunds/{refundId}`

Retrieves the details of an existing refund. The `refundId` is obtained from Nexi Group when [creating a new refund](#v1-charges-chargeid-refunds-post). The primary usage of this method is to retrieve invoice details of a refund.

#### Parameters

*   refundIdstringrequired
    
    The identifier of the existing refund (a UUID).
    

#### Responses

*   200OKoptional
    
    *   refundIdstring (uuid)required
        
        The refund identifier (a UUID).
        
        example: 391a715deb1a4c9cb7b5ad6cbd021ec2
    *   amountinteger (int32)required
        
        The base [amount](../#currency-and-amount) of the refund.
        
    *   surchargeAmountinteger (int32)optional
        
        The surcharge [amount](../#currency-and-amount) of the refund. It might not be populated until the operation is completed.
        
    *   invoiceDetailsobjectoptional
        
        Information about a publicly accessible invoice.
        
        *   linkstringoptional
            
            The URL of an invoice that is publicly accessible.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "refundId": "391a715deb1a4c9cb7b5ad6cbd021ec2",
        "amount": 0,
        "surchargeAmount": 0,
        "invoiceDetails": {
            "link": "string"
        }
    }

### Cancel pending refund

`POST /v1/pending-refunds/{refundId}/cancel`

Cancels a pending refund. A refund can be in a pending state when there are not enough funds in the merchant's account to make the refund.

The `refundId` is returned when [creating a new refund](#create-refund).

#### Parameters

*   refundIdstringrequired
    
    The identifier of the pending refund (a UUID).
    

#### Responses

*   204No Contentoptional
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   405Method Not Allowedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   400
*   500

    {
        "errors": {
            "property1": [
                "string"
            ],
            "property2": [
                "string"
            ]
        }
    }

### Get payment methods for Merchant

`GET /v1/paymentmethods`

#### Parameters

*   MerchantNumberstringoptional
*   Currencystringoptional
*   Enabledbooleanoptional

#### Responses

*   200OKoptional
    
    *   namestringoptional
    *   paymentTypestringoptional
    *   currencystringoptional
    *   enabledbooleanoptional
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    [
        {
            "name": "string",
            "paymentType": "string",
            "currency": "string",
            "enabled": true
        }
    ]

Subscriptions
-------------

Subscriptions allow you to charge your customers on a regular basis, for example a monthly subscription for a product the customer must pay for every month.  
  
When a subscription is charged, a new payment object is created to represent the purchase of the subscription product.  
  
It is possible to verify and charge multiple subscriptions in bulk using the [Bulk charge subscriptions method](#v1-subscription-charges-post).

### Retrieve subscription

`GET /v1/subscriptions/{subscriptionId}`

Retrieves an existing subscription by a `subscriptionId`. The `subscriptionId` can be obtained from the [Retrieve payment](#get-payment) method.

#### Parameters

*   subscriptionIdstring (uuid)required
    
    The subscription identifier (a UUID).
    
*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Responses

*   200OKoptional
    
    *   subscriptionIdstring (uuid)required
        
        The subscription identifier.
        
        example: c0e200edc7304ccf9bb214c7d251ee23
    *   frequencyinteger (int32)optional
    *   intervalinteger (int32)required
        
        Defines the minimum number of days between each recurring charge. This interval commences from either the day the subscription was created or the most recent subscription charge, whichever is later. An interval value of 0 means that there are no payment interval restrictions.
        
    *   endDatestring (date-time)required
        
        Refers to the date and time the subscription will expire. The field has three components: date, time, and time zone (offset from GMT), for example: 2021-07-02T12:00:00.0000+02:00.
        
    *   paymentDetailsobjectrequired
        
        *   paymentTypestringrequired
            
            The type of payment. Possible values are: 'CARD', 'INVOICE', 'A2A', 'INSTALLMENT', 'WALLET', and 'PREPAID-INVOICE'.
            
        *   paymentMethodstringrequired
            
            The payment method. For example Visa or Mastercard.
            
        *   cardDetailsobjectrequired
            
            *   expiryDatestringrequired
                
                The four-digit expiration date of the payment card. The format should be: MMYY.
                
            *   maskedPanstringrequired
                
                A masked version of the PAN (Primary Account Number). At maximum, only the first six and last four digits of the account number are displayed.
                
            
        
    *   importErrorobjectoptional
        
        Represents an error that occurred during the import of a subscription from an external ecommerce system.
        
        *   importStepsResponseCodestringoptional
            
            The error code.
            
        *   importStepsResponseSourcestringoptional
            
            The source of the error.
            
        *   importStepsResponseTextstringoptional
            
            The error message.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "subscriptionId": "c0e200edc7304ccf9bb214c7d251ee23",
        "frequency": 0,
        "interval": 0,
        "endDate": "2019-08-24T14:15:22Z",
        "paymentDetails": {
            "paymentType": "string",
            "paymentMethod": "string",
            "cardDetails": {
                "expiryDate": "string",
                "maskedPan": "string"
            }
        },
        "importError": {
            "importStepsResponseCode": "string",
            "importStepsResponseSource": "string",
            "importStepsResponseText": "string"
        }
    }

### Charge subscription

`POST /v1/subscriptions/{subscriptionId}/charges`

Charges a single subscription. The `subscriptionId` can be obtained from the [Retrieve payment](#get-payment) method. On success, this method creates a new payment object and performs a charge of the specified amount. Both the new `paymentId` and `chargeId` are returned in the response body.

#### Parameters

*   subscriptionIdstring (uuid)required
    
    The subscription identifier (a UUID) returned from the [Retrieve payment](#v1-payments-paymentId-get) method.
    
*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the charge you are attempting. Must be between 1 and 64 characters.
    
*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Request body

Expand all

*   orderobjectrequired
    
    Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
    
    *   itemsarrayrequired
        
        A list of order items. At least one item must be specified.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    *   amountinteger (int32)required
        
        The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
        
    *   currencystringrequired
        
        The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
        
    *   referencestringoptional
        
        A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   myReferencestringoptional

### Request body

    {
        "order": {
            "items": [
                {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            ],
            "amount": 0,
            "currency": "string",
            "reference": "string"
        },
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "myReference": "string"
    }

#### Responses

*   200OKoptional
    
    *   paymentIdstring (uuid)required
        
        The payment identifier of the new payment object created when charging for the subscription.
        
        example: 2e1d7fd86e214947a0c4a1284786acd8
    *   chargeIdstring (uuid)required
        
        A unique identifier of the charge.
        
        example: 33cbc84b20eb43a4b4c4e9b2d0d19fa6
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   402Payment Requiredoptional
*   404Not Foundoptional
*   409Conflictoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "paymentId": "2e1d7fd86e214947a0c4a1284786acd8",
        "chargeId": "33cbc84b20eb43a4b4c4e9b2d0d19fa6"
    }

### Retrieve subscription charge status

`GET /v1/subscriptions/{subscriptionId}/charges/status`

Retrieves an existing subscription charge status by a `subscriptionId`. The `subscriptionId` can be obtained from the [Retrieve payment](#get-payment) method.

#### Parameters

*   subscriptionIdstring (uuid)required
    
    The subscription identifier (a UUID).
    
*   Idempotency-Keystringrequired
    
    A string that uniquely identifies the charge you are attempting. Must be between 1 and 64 characters.
    
*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Responses

*   200OKoptional
    
    *   paymentIdstring (uuid)required
        
        The payment identifier of the new payment object created when charging for the unscheduled subscription.
        
        example: 583170b81702482e82e91b6d36311b3f
    *   chargeIdstring (uuid)required
        
        A unique identifier of the charge.
        
        example: 002c8a931603436f8dbbdcbf88954818
    *   completedbooleanrequired
        
        Whether the charge was completed.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional

*   200
*   400

    {
        "paymentId": "583170b81702482e82e91b6d36311b3f",
        "chargeId": "002c8a931603436f8dbbdcbf88954818",
        "completed": true
    }

### Retrieve subscription by external reference

`GET /v1/subscriptions`

Retrieves a subscription matching the specified `externalReference`. This method can only be used for retrieving subscriptions that have been imported from a payment platform other than Checkout. Subscriptions created within Checkout do not have an `externalReference` value set.

#### Parameters

*   externalReferencestringoptional
    
    The external reference to search for.
    
*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Responses

*   200OKoptional
    
    *   subscriptionIdstring (uuid)required
        
        The subscription identifier.
        
        example: c0e200edc7304ccf9bb214c7d251ee23
    *   frequencyinteger (int32)optional
    *   intervalinteger (int32)required
        
        Defines the minimum number of days between each recurring charge. This interval commences from either the day the subscription was created or the most recent subscription charge, whichever is later. An interval value of 0 means that there are no payment interval restrictions.
        
    *   endDatestring (date-time)required
        
        Refers to the date and time the subscription will expire. The field has three components: date, time, and time zone (offset from GMT), for example: 2021-07-02T12:00:00.0000+02:00.
        
    *   paymentDetailsobjectrequired
        
        *   paymentTypestringrequired
            
            The type of payment. Possible values are: 'CARD', 'INVOICE', 'A2A', 'INSTALLMENT', 'WALLET', and 'PREPAID-INVOICE'.
            
        *   paymentMethodstringrequired
            
            The payment method. For example Visa or Mastercard.
            
        *   cardDetailsobjectrequired
            
            *   expiryDatestringrequired
                
                The four-digit expiration date of the payment card. The format should be: MMYY.
                
            *   maskedPanstringrequired
                
                A masked version of the PAN (Primary Account Number). At maximum, only the first six and last four digits of the account number are displayed.
                
            
        
    *   importErrorobjectoptional
        
        Represents an error that occurred during the import of a subscription from an external ecommerce system.
        
        *   importStepsResponseCodestringoptional
            
            The error code.
            
        *   importStepsResponseSourcestringoptional
            
            The source of the error.
            
        *   importStepsResponseTextstringoptional
            
            The error message.
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "subscriptionId": "c0e200edc7304ccf9bb214c7d251ee23",
        "frequency": 0,
        "interval": 0,
        "endDate": "2019-08-24T14:15:22Z",
        "paymentDetails": {
            "paymentType": "string",
            "paymentMethod": "string",
            "cardDetails": {
                "expiryDate": "string",
                "maskedPan": "string"
            }
        },
        "importError": {
            "importStepsResponseCode": "string",
            "importStepsResponseSource": "string",
            "importStepsResponseText": "string"
        }
    }

### Bulk charge subscriptions

`POST /v1/subscriptions/charges`

Charges multiple subscriptions at once. The request body must contain:

*   A unique string that identifies this bulk charge operation
*   A set of subscription identifiers that should be charged.

To get status updates about the bulk charge you can subscribe to the webhooks for charges and refunds (`payment.charges.*` and `payments.refunds.*`). See also the [webhooks documentation](#webhooks).

#### Parameters

*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Request body

Expand all

*   externalBulkChargeIdstringrequired
    
    A string that uniquely identifies the bulk charge operation. Use this property for enabling safe retries. Must be between 1 and 64 characters.
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   subscriptionsarrayrequired
    
    The array of subscriptions that should be charged. Each item in the array should define either a `subscriptionId` or an `externalReference`, but not both.
    
    *   subscriptionIdstring (uuid)optional
        
        The subscription identifier (a UUID) returned from the [Retrieve payment](#v1-payments-paymentId-get) method.
        
    *   externalReferencestringoptional
        
        An external reference to identify a set of imported subscriptions. This parameter is only used if your subscriptions have been imported from a payment platform other than Checkout.
        
    *   orderobjectrequired
        
        Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
        
        *   itemsarrayrequired
            
            A list of order items. At least one item must be specified.
            
            *   referencestringrequired
                
                A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                
            *   namestringrequired
                
                The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                
            *   quantitynumber (double)required
                
                The quantity of the product. The value can not be negative.
                
            *   unitstringrequired
                
                The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   unitPriceinteger (int32)required
                
                The price per unit excluding VAT. Note: The amount can be negative.
                
            *   taxRateinteger (int32)optional
                
                The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
                
            *   taxAmountinteger (int32)optional
                
                The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
                
            *   grossTotalAmountinteger (int32)required
                
                The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
                
            *   netTotalAmountinteger (int32)required
                
                The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
                
            *   imageUrlstringoptional
                
                Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
                
            
        *   amountinteger (int32)required
            
            The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
            
        *   currencystringrequired
            
            The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
            
        *   referencestringoptional
            
            A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        
    *   myReferencestringoptional
    

### Request body

    {
        "externalBulkChargeId": "string",
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "subscriptions": [
            {
                "subscriptionId": "d079718b-ff63-45dd-947b-4950c023750f",
                "externalReference": "string",
                "order": {
                    "items": [
                        {
                            "reference": "string",
                            "name": "string",
                            "quantity": 0.1,
                            "unit": "string",
                            "unitPrice": 0,
                            "taxRate": 0,
                            "taxAmount": 0,
                            "grossTotalAmount": 0,
                            "netTotalAmount": 0,
                            "imageUrl": "string"
                        }
                    ],
                    "amount": 0,
                    "currency": "string",
                    "reference": "string"
                },
                "myReference": "string"
            }
        ]
    }

#### Responses

*   202Acceptedoptional
    
    *   bulkIdstring (uuid)required
        
        The bulk charge identifier (a UUID). This identifier can be used when [retrieving all charges associated with a bulk charge operation](#v1-subscriptions-charges-bulkid-get).
        
        example: d9049e943b0d44a6a00212cc12115bcc
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   202
*   400
*   500

    {
        "bulkId": "d9049e943b0d44a6a00212cc12115bcc"
    }

### Retrieve bulk charges

`GET /v1/subscriptions/charges/{bulkId}`

Retrieves charges associated with the specified bulk charge operation. The `bulkId` is returned from Nexi Group in the response of the [Bulk charge subscriptions](#v1-subscription-charges-post) method.

This method supports pagination. Specify the range of subscriptions to retrieve by using either `skip` and `take` or `pageNumber` together with `pageSize`. The boolean property named `more` in the response body, indicates whether there are more subscriptions beyond the requested range.

#### Parameters

*   bulkIdstringrequired
    
    The identifier of the bulk charge operation that was returned from the [Bulk charge subscriptions](#v1-subscriptions-charges-post) method.
    
*   skipinteger (int32)optional
    
    The number of subscription entries to skip from the start. Use this property in combination with the `take` property.
    
*   takeinteger (int32)optional
    
    The maximum number of subscriptions to be retrieved. Use this property in combination with the `skip` property.
    
*   pageNumberinteger (int32)optional
    
    The page number to be retrieved. Use this property in combination with the `pageSize` property.
    
*   pageSizeinteger (int32)optional
    
    The size of each page when specify the range of subscriptions using the `pageNumber` property.
    
*   MerchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    

#### Responses

*   200OKoptional
    
    *   pagearrayoptional
        
        *   subscriptionIdstring (uuid)required
            
            The subscription identifier (a UUID) returned from the [Retrieve payment](#v1-payments-paymentid-get) method.
            
            example: 5f4bc0c205604b6a9ceee7c4e5156160
        *   paymentIdstring (uuid)optional
            
            The payment identifier.
            
        *   chargeIdstring (uuid)optional
            
            The charge identifier (a UUID) returned from the [Charge payment](#charge-payment) method.
            
        *   statusstringrequired
            
            The current processing status of the subscription. Possible values are: 'Pending', 'Succeeded', and 'Failed'.
            
        *   messagestringoptional
        *   codestringoptional
        *   sourcestringoptional
        *   externalReferencestringoptional
            
            An external reference to identify a set of imported subscriptions. This parameter is only used if your subscriptions have been imported from a payment platform other than Checkout.
            
        
    *   morebooleanoptional
        
        Indicates whether there are more subscriptions beyond the requested range.
        
    *   statusstringoptional
        
        Indicates whether the operation has completed or is still processing subscriptions. Possible values are 'Done' and 'Processing'.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "page": [
            {
                "subscriptionId": "5f4bc0c205604b6a9ceee7c4e5156160",
                "paymentId": "472e651e-5a1e-424d-8098-23858bf03ad7",
                "chargeId": "aec0aceb-a4db-49fb-b366-75e90229c640",
                "status": "string",
                "message": "string",
                "code": "string",
                "source": "string",
                "externalReference": "string"
            }
        ],
        "more": true,
        "status": "string"
    }

### Verify subscriptions

`POST /v1/subscriptions/verifications`

Verifies the specified set of subscriptions in bulk. The `bulkId` returned from a successful request can be used for querying the status of the subscriptions.

#### Parameters

#### Request body

Expand all

*   externalBulkVerificationIdstringoptional
    
    A string that uniquely identifies the verification operation. Use this property for enabling safe retries. Must be between 1 and 64 characters.
    
*   subscriptionsarrayoptional
    
    The set of subscriptions that should be verified. Each item in the array should define either a `subscriptioId` or an `externalReference`, but not both.
    
    *   subscriptionIdstring (uuid)optional
        
        The identifier of the subscription (a UUID). The `subscriptionId` can be obtained using the [Retrieve payment](#v1-payments-paymentid-get) method.
        
    *   externalReferencestringoptional
        
        An external reference to identify a set of imported subscriptions. This parameter is only used if your subscriptions have been imported from a payment platform other than Checkout.
        
    

### Request body

    {
        "externalBulkVerificationId": "string",
        "subscriptions": [
            {
                "subscriptionId": "d079718b-ff63-45dd-947b-4950c023750f",
                "externalReference": "string"
            }
        ]
    }

#### Responses

*   202Acceptedoptional
    
    *   bulkIdstring (uuid)requiredexample: 330e22031bed45ad8913622e1057e299
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   202
*   400
*   500

    {
        "bulkId": "330e22031bed45ad8913622e1057e299"
    }

### Retrieve bulk verifications

`GET /v1/subscriptions/verifications/{bulkId}`

Retrieves verifications associated with the specified bulk verification operation. The `bulkId` is returned from Nexi Group in the response of the [Verify subscriptions](#v1-subscriptions-verifications-post) method.

This method supports pagination. Specify the range of subscriptions to retrieve by using either `skip` and `take` or `pageNumber` together with `pageSize`. The boolean property named `more` in the response body, indicates whether there are more subscriptions beyond the requested range.

#### Parameters

*   bulkIdstringrequired
    
    The identifier of the bulk verification operation that was returned from the [Verify subscriptions](#v1-subscriptions-verifications-post) method.
    
*   skipinteger (int32)optional
    
    The number of subscription entries to skip from the start. Use this property in combination with the `take` property.
    
*   takeinteger (int32)optional
    
    The maximum number of subscriptions to be retrieved. Use this property in combination with the `skip` property.
    
*   pageNumberinteger (int32)optional
    
    The page number to be retrieved. Use this property in combination with the `pageSize` property.
    
*   pageSizeinteger (int32)optional
    
    The size of each page when specify the range of subscriptions using the `pageNumber` property.
    

#### Responses

*   200OKoptional
    
    *   pagearrayoptional
        
        *   subscriptionIdstring (uuid)required
            
            The identifier of the subscription (a UUID).
            
            example: 80a93c094159487590844ddabb151bdd
        *   externalReferencestringoptional
            
            An external reference to identify a set of imported subscriptions. This parameter is only used if your subscriptions have been imported from a payment platform other than Checkout.
            
        *   statusstringrequired
            
            The current processing status of the subscription. Possible values are: 'Pending', 'Succeeded', and 'Failed'.
            
        *   messagestringoptional
        *   codestringoptional
        *   paymentIdstring (uuid)optional
            
            The payment identifier (a UUID).
            
        
    *   morebooleanoptional
        
        Indicates whether there are more subscriptions beyond the requested range.
        
    *   statusstringoptional
        
        Indicates whether the operation has completed or is still processing subscriptions. Possible values are 'Done' and 'Processing'.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "page": [
            {
                "subscriptionId": "80a93c094159487590844ddabb151bdd",
                "externalReference": "string",
                "status": "string",
                "message": "string",
                "code": "string",
                "paymentId": "472e651e-5a1e-424d-8098-23858bf03ad7"
            }
        ],
        "more": true,
        "status": "string"
    }

UnscheduledSubscriptions
------------------------

Unscheduled subscriptions allow you to charge your customers at an unscheduled time interval with a variable amount, for example an automatic top-up agreement for a rail-card when the consumer drops below a certain stored value.  
  
When an unscheduled subscription is charged, a new payment object is created to represent the purchase of the unscheduled subscription product.  
  
It is possible to verify and charge multiple unscheduled subscriptions in bulk using the [Bulk charge unscheduled subscriptions method](#v1-unscheduled-subscription-charges-post).

### Retrieve unscheduled subscription

`GET /v1/unscheduledsubscriptions/{unscheduledSubscriptionId}`

Retrieves an existing unscheduled subscription by a `unscheduledSubscriptionId`. The `unscheduledSubscriptionId` can be obtained from the [Retrieve payment](#get-payment) method.

#### Parameters

*   unscheduledSubscriptionIdstring (uuid)required
    
    The unscheduled subscription identifier (a UUID).
    

#### Responses

*   200OKoptional
    
    *   unscheduledSubscriptionIdstring (uuid)required
        
        The unscheduled subscription identifier.
        
        example: 98ad5e2905eb455580d134e31fd3e5c7
    *   paymentDetailsobjectrequired
        
        *   paymentTypestringrequired
            
            The type of payment. Possible values are: 'CARD', 'INVOICE', 'A2A', 'INSTALLMENT', 'WALLET', and 'PREPAID-INVOICE'.
            
        *   paymentMethodstringrequired
            
            The payment method. For example Visa or Mastercard.
            
        *   cardDetailsobjectrequired
            
            *   expiryDatestringrequired
                
                The four-digit expiration date of the payment card. The format should be: MMYY.
                
            *   maskedPanstringrequired
                
                A masked version of the PAN (Primary Account Number). At maximum, only the first six and last four digits of the account number are displayed.
                
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "unscheduledSubscriptionId": "98ad5e2905eb455580d134e31fd3e5c7",
        "paymentDetails": {
            "paymentType": "string",
            "paymentMethod": "string",
            "cardDetails": {
                "expiryDate": "string",
                "maskedPan": "string"
            }
        }
    }

### Retrieve unscheduled subscription by external reference

`GET /v1/unscheduledsubscriptions`

Retrieves an unscheduled subscription matching the specified `externalReference`. This method can only be used for retrieving unscheduled subscriptions that have been imported from a payment platform other than Checkout. Unscheduled subscriptions created within Checkout do not have an `externalReference` value set.

#### Parameters

*   externalReferencestringoptional
    
    The external reference to search for.
    

#### Responses

*   200OKoptional
    
    *   unscheduledSubscriptionIdstring (uuid)required
        
        The unscheduled subscription identifier.
        
        example: 98ad5e2905eb455580d134e31fd3e5c7
    *   paymentDetailsobjectrequired
        
        *   paymentTypestringrequired
            
            The type of payment. Possible values are: 'CARD', 'INVOICE', 'A2A', 'INSTALLMENT', 'WALLET', and 'PREPAID-INVOICE'.
            
        *   paymentMethodstringrequired
            
            The payment method. For example Visa or Mastercard.
            
        *   cardDetailsobjectrequired
            
            *   expiryDatestringrequired
                
                The four-digit expiration date of the payment card. The format should be: MMYY.
                
            *   maskedPanstringrequired
                
                A masked version of the PAN (Primary Account Number). At maximum, only the first six and last four digits of the account number are displayed.
                
            
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "unscheduledSubscriptionId": "98ad5e2905eb455580d134e31fd3e5c7",
        "paymentDetails": {
            "paymentType": "string",
            "paymentMethod": "string",
            "cardDetails": {
                "expiryDate": "string",
                "maskedPan": "string"
            }
        }
    }

### Charge unscheduled subscription

`POST /v1/unscheduledsubscriptions/{unscheduledSubscriptionId}/charges`

Charges a single unscheduled subscription. The `unscheduledSubscriptionId` can be obtained from the [Retrieve payment](#get-payment) method. On success, this method creates a new payment object and performs a charge of the specified amount. Both the new `paymentId` and `chargeId` are returned in the response body.

#### Parameters

*   unscheduledSubscriptionIdstring (uuid)required
    
    The unscheduled subscription identifier (a UUID) returned from the [Retrieve payment](#v1-payments-paymentId-get) method.
    
*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the charge you are attempting. Must be between 1 and 64 characters.
    

#### Request body

Expand all

*   orderobjectrequired
    
    Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
    
    *   itemsarrayrequired
        
        A list of order items. At least one item must be specified.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    *   amountinteger (int32)required
        
        The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
        
    *   currencystringrequired
        
        The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
        
    *   referencestringoptional
        
        A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   myReferencestringoptional

### Request body

    {
        "order": {
            "items": [
                {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            ],
            "amount": 0,
            "currency": "string",
            "reference": "string"
        },
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "myReference": "string"
    }

#### Responses

*   200OKoptional
    
    *   paymentIdstring (uuid)required
        
        The payment identifier of the new payment object created when charging for the unscheduled subscription.
        
        example: cde57b2e01814c7e835d003242a377b7
    *   chargeIdstring (uuid)required
        
        A unique identifier of the charge.
        
        example: 79603769681f4acdb838cd513ff32555
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   402Payment Requiredoptional
*   404Not Foundoptional
*   409Conflictoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "paymentId": "cde57b2e01814c7e835d003242a377b7",
        "chargeId": "79603769681f4acdb838cd513ff32555"
    }

### Retrieve unscheduled subscription charge status

`GET /v1/unscheduledsubscriptions/{unscheduledSubscriptionId}/charges/status`

Retrieves an existing unscheduled subscription charge status by a `unscheduledSubscriptionId`. The `unscheduledSubscriptionId` can be obtained from the [Retrieve payment](#get-payment) method.

#### Parameters

*   unscheduledSubscriptionIdstring (uuid)required
    
    The unscheduled subscription identifier (a UUID).
    
*   Idempotency-Keystringrequired
    
    A string that uniquely identifies the charge you are attempting. Must be between 1 and 64 characters.
    

#### Responses

*   200OKoptional
    
    *   paymentIdstring (uuid)required
        
        The payment identifier of the new payment object created when charging for the unscheduled subscription.
        
        example: 583170b81702482e82e91b6d36311b3f
    *   chargeIdstring (uuid)required
        
        A unique identifier of the charge.
        
        example: 002c8a931603436f8dbbdcbf88954818
    *   completedbooleanrequired
        
        Whether the charge was completed.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional

*   200
*   400

    {
        "paymentId": "583170b81702482e82e91b6d36311b3f",
        "chargeId": "002c8a931603436f8dbbdcbf88954818",
        "completed": true
    }

### Bulk charge unscheduled subscriptions

`POST /v1/unscheduledsubscriptions/charges`

Charges multiple unscheduled subscriptions at once. The request body must contain:

*   A unique string that identifies this bulk charge operation
*   A set of unscheduled subscription identifiers that should be charged.

To get status updates about the bulk charge you can subscribe to the webhooks for charges and refunds (`payment.charges.*` and `payments.refunds.*`). See also the [webhooks documentation](#webhooks).

#### Parameters

#### Request body

Expand all

*   externalBulkChargeIdstringoptional
    
    A string that uniquely identifies the bulk charge operation. Use this property for enabling safe retries. Must be between 1 and 64 characters.
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   unscheduledSubscriptionsarrayoptional
    
    The array of unscheduled subscriptions that should be charged. Each item in the array should define either a `subscriptionId` or an `externalReference`, but not both.
    
    *   unscheduledSubscriptionIdstring (uuid)optional
        
        The subscription identifier (a UUID) returned from the [Retrieve payment](#v1-payments-paymentId-get) method.
        
    *   externalReferencestringoptional
        
        An external reference to identify a set of imported subscriptions. This parameter is only used if your unscheduled subscriptions have been imported from a payment platform other than Checkout.
        
    *   orderobjectrequired
        
        Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
        
        *   itemsarrayrequired
            
            A list of order items. At least one item must be specified.
            
            *   referencestringrequired
                
                A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                
            *   namestringrequired
                
                The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
                
            *   quantitynumber (double)required
                
                The quantity of the product. The value can not be negative.
                
            *   unitstringrequired
                
                The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   unitPriceinteger (int32)required
                
                The price per unit excluding VAT. Note: The amount can be negative.
                
            *   taxRateinteger (int32)optional
                
                The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
                
            *   taxAmountinteger (int32)optional
                
                The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
                
            *   grossTotalAmountinteger (int32)required
                
                The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
                
            *   netTotalAmountinteger (int32)required
                
                The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
                
            *   imageUrlstringoptional
                
                Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
                
            
        *   amountinteger (int32)required
            
            The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
            
        *   currencystringrequired
            
            The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
            
        *   referencestringoptional
            
            A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        
    *   myReferencestringoptional
    

### Request body

    {
        "externalBulkChargeId": "string",
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "unscheduledSubscriptions": [
            {
                "unscheduledSubscriptionId": "92143051-9e78-40af-a01f-245ccdcd9c03",
                "externalReference": "string",
                "order": {
                    "items": [
                        {
                            "reference": "string",
                            "name": "string",
                            "quantity": 0.1,
                            "unit": "string",
                            "unitPrice": 0,
                            "taxRate": 0,
                            "taxAmount": 0,
                            "grossTotalAmount": 0,
                            "netTotalAmount": 0,
                            "imageUrl": "string"
                        }
                    ],
                    "amount": 0,
                    "currency": "string",
                    "reference": "string"
                },
                "myReference": "string"
            }
        ]
    }

#### Responses

*   202Acceptedoptional
    
    *   bulkIdstring (uuid)required
        
        The bulk charge identifier (a UUID). This identifier can be used when [retrieving all charges associated with a bulk charge operation](#v1-unscheduled-subscriptions-charges-bulkid-get).
        
        example: 876eb4fec10d41eb819b69dd8934a62b
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   202
*   400
*   500

    {
        "bulkId": "876eb4fec10d41eb819b69dd8934a62b"
    }

### Retrieve bulk unscheduled charges

`GET /v1/unscheduledsubscriptions/charges/{bulkId}`

Retrieves charges associated with the specified bulk charge operation. The `bulkId` is returned from Nexi Group in the response of the [Bulk charge unscheduled subscriptions](#v1-unscheduled-subscription-charges-post) method.

This method supports pagination. Specify the range of subscriptions to retrieve by using either `skip` and `take` or `pageNumber` together with `pageSize`. The boolean property named `more` in the response body, indicates whether there are more subscriptions beyond the requested range.

#### Parameters

*   bulkIdstringrequired
    
    The identifier of the bulk charge operation that was returned from the [Bulk charge unscheduled subscriptions](#v1-unscheduled-subscriptions-charges-post) method.
    
*   skipinteger (int32)optional
    
    The number of subscription entries to skip from the start. Use this property in combination with the `take` property.
    
*   takeinteger (int32)optional
    
    The maximum number of subscriptions to be retrieved. Use this property in combination with the `skip` property.
    
*   pageNumberinteger (int32)optional
    
    The page number to be retrieved. Use this property in combination with the `pageSize` property.
    
*   pageSizeinteger (int32)optional
    
    The size of each page when specify the range of subscriptions using the `pageNumber` property.
    

#### Responses

*   200OKoptional
    
    *   pagearrayoptional
        
        *   unscheduledSubscriptionIdstring (uuid)required
            
            The unscheduled subscription identifier (a UUID) returned from the Retrieve bulk unscheduled subscription charges method.
            
            example: 6da9da087e6641c58568f0b2239152e4
        *   paymentIdstring (uuid)optional
            
            The payment identifier.
            
        *   chargeIdstring (uuid)optional
            
            The charge identifier (a UUID) returned from the [Charge payment](#charge-payment) method.
            
        *   statusstringrequired
            
            The current processing status of the subscription. Possible values are: 'Pending', 'Succeeded', and 'Failed'.
            
        *   messagestringoptional
        *   codestringoptional
        *   sourcestringoptional
        *   externalReferencestringoptional
            
            An external reference to identify a set of imported subscriptions. This parameter is only used if your unscheduled subscriptions have been imported from a payment platform other than Checkout.
            
        
    *   morebooleanoptional
        
        Indicates whether there are more subscriptions beyond the requested range.
        
    *   statusstringoptional
        
        Indicates whether the operation has completed or is still processing subscriptions. Possible values are 'Done' and 'Processing'.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "page": [
            {
                "unscheduledSubscriptionId": "6da9da087e6641c58568f0b2239152e4",
                "paymentId": "472e651e-5a1e-424d-8098-23858bf03ad7",
                "chargeId": "aec0aceb-a4db-49fb-b366-75e90229c640",
                "status": "string",
                "message": "string",
                "code": "string",
                "source": "string",
                "externalReference": "string"
            }
        ],
        "more": true,
        "status": "string"
    }

### Verify cards for unscheduled subscriptions

`POST /v1/unscheduledsubscriptions/verifications`

Verifies the specified set of unscheduled subscriptions in bulk. The `bulkId` returned from a successful request can be used for querying the status of the unscheduled subscriptions.

#### Parameters

#### Request body

Expand all

*   externalBulkVerificationIdstringoptional
    
    A string that uniquely identifies the verification operation. Use this property for enabling safe retries. Must be between 1 and 64 characters.
    
*   unscheduledSubscriptionsarrayoptional
    
    The set of unscheduled subscriptions that should be verified. Each item in the array should define either a `unscheduledSubscriptionId` or an `externalReference`, but not both.
    
    *   unscheduledSubscriptionIdstring (uuid)optional
        
        The identifier of the unscheduled subscription (a UUID). The `unscheduledSubscriptionId` can be obtained using the [Retrieve payment](#v1-payments-paymentid-get) method.
        
    *   externalReferencestringoptional
        
        An external reference to identify a set of imported subscriptions. This parameter is only used if your unscheduled subscriptions have been imported from a payment platform other than Checkout.
        
    

### Request body

    {
        "externalBulkVerificationId": "string",
        "unscheduledSubscriptions": [
            {
                "unscheduledSubscriptionId": "92143051-9e78-40af-a01f-245ccdcd9c03",
                "externalReference": "string"
            }
        ]
    }

#### Responses

*   202Acceptedoptional
    
    *   bulkIdstring (uuid)requiredexample: a7461adebd6d4974bf1941d09af45293
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   202
*   400
*   500

    {
        "bulkId": "a7461adebd6d4974bf1941d09af45293"
    }

### Retrieve bulk verifications for unscheduled subscriptions

`GET /v1/unscheduledsubscriptions/verifications/{bulkId}`

Retrieves verifications associated with the specified bulk unscheduled verification operation. The `bulkId` is returned from Nexi Group in the response of the [Verify unscheduled subscriptions](#v1-unscheduledsubscriptions-verifications-post) method.

This method supports pagination. Specify the range of subscriptions to retrieve by using either `skip` and `take` or `pageNumber` together with `pageSize`. The boolean property named `more` in the response body, indicates whether there are more subscriptions beyond the requested range.

#### Parameters

*   bulkIdstringrequired
    
    The identifier of the bulk verification operation that was returned from the [Verify unscheduled subscriptions](#v1-unscheduledsubscriptions-verifications-post) method.
    
*   skipinteger (int32)optional
    
    The number of unscheduled subscription entries to skip from the start. Use this property in combination with the `take` property.
    
*   takeinteger (int32)optional
    
    The maximum number of unscheduled subscriptions to be retrieved. Use this property in combination with the `skip` property.
    
*   pageNumberinteger (int32)optional
    
    The page number to be retrieved. Use this property in combination with the `pageSize` property.
    
*   pageSizeinteger (int32)optional
    
    The size of each page when specify the range of unscheduled subscriptions using the `pageNumber` property.
    

#### Responses

*   200OKoptional
    
    *   pagearrayoptional
        
        *   unscheduledSubscriptionIdstring (uuid)required
            
            The identifier of the unscheduled subscription (a UUID).
            
            example: ea581e2670ff4fdd8f80c0776ee77bc2
        *   externalReferencestringoptional
            
            An external reference to identify a set of imported unscheduled subscriptions. This parameter is only used if your unscheduled subscriptions have been imported from a payment platform other than Nets Easy.
            
        *   statusstringrequired
            
            The current processing status of the unscheduled subscription. Possible values are: 'Pending', 'Succeeded', and 'Failed'.
            
        *   messagestringoptional
        *   codestringoptional
        *   paymentIdstring (uuid)optional
            
            The payment identifier (a UUID).
            
        
    *   morebooleanoptional
        
        Indicates whether there are more subscriptions beyond the requested range.
        
    *   statusstringoptional
        
        Indicates whether the operation has completed or is still processing subscriptions. Possible values are 'Done' and 'Processing'.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   404Not Foundoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   200
*   400
*   500

    {
        "page": [
            {
                "unscheduledSubscriptionId": "ea581e2670ff4fdd8f80c0776ee77bc2",
                "externalReference": "string",
                "status": "string",
                "message": "string",
                "code": "string",
                "paymentId": "472e651e-5a1e-424d-8098-23858bf03ad7"
            }
        ],
        "more": true,
        "status": "string"
    }

CardPayment
-----------

Secure Card Payments are available for [PCI](https://en.wikipedia.org/wiki/Payment_Card_Industry_Data_Security_Standard) compliant merchants, that wish to reserve or directly capture payments on the behalf of their customers avoiding the [3DS authorization](https://en.wikipedia.org/wiki/3-D_Secure) step, assuming they've already pre-acquired authorizations from their customers in some other legal and compliant form.

### Attempts to reserve or directly charge a secure card payment. This endpoint can be only used by PCI compliant merchants who are able to store, process and send card information securely following PCI-DSS. To access this endpoint, please reach out to NETS through the official support channels. Your request will be reviewed and, if approved, you'll be provided with the required access permissions. Once your access is set up, you’ll be able to start sending requests to the endpoint.

`POST /v1/cardpayments`

#### Parameters

*   Idempotency-Keystringoptional
    
    A string that uniquely identifies the payment attempted. Must be between 1 and 64 characters.
    

#### Request body

Expand all

*   interactionTypeinteger (int32)optional
    
    `0`
    
*   orderobjectrequired
    
    Specifies an order associated with a payment. An order must contain at least one order item. The `amount` of the order must match the sum of the specified order items.
    
    *   itemsarrayrequired
        
        A list of order items. At least one item must be specified.
        
        *   referencestringrequired
            
            A reference to recognize the product, usually the SKU (stock keeping unit) of the product. For convenience in the case of refunds or modifications of placed orders, the reference should be unique for each variation of a product item (size, color, etc). The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   namestringrequired
            
            The name of the product. The maximum length is 128 characters. The following special characters are not supported: `<,>,\\`
            
        *   quantitynumber (double)required
            
            The quantity of the product. The value can not be negative.
            
        *   unitstringrequired
            
            The defined unit of measurement for the product, for example pcs, liters, or kg. The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   unitPriceinteger (int32)required
            
            The price per unit excluding VAT. Note: The amount can be negative.
            
        *   taxRateinteger (int32)optional
            
            The tax/VAT rate (in percentage times 100). For example, the value `2500` corresponds to 25%. Defaults to 0 if not provided. Must be between 0 and 99999. Tax Rate must be applied per unit.
            
        *   taxAmountinteger (int32)optional
            
            The tax/VAT amount (`unitPrice` \* `quantity` \* `taxRate` / 10000). Defaults to 0 if not provided. `taxAmount` should include the total tax amount for the entire order item.
            
        *   grossTotalAmountinteger (int32)required
            
            The total amount including VAT (`netTotalAmount` + `taxAmount`). Note: The amount can be negative.
            
        *   netTotalAmountinteger (int32)required
            
            The total amount excluding VAT (`unitPrice` \* `quantity`). Note: The amount can be negative.
            
        *   imageUrlstringoptional
            
            Url to image of the product. Meant to be configured before checkout is completed. Ignored on later operations like charging, refunding etc. Currently affecting: Riverty Invoice. Supported size: width and height between 100 pixels and 1280 pixels. Supported formats: gif, jpeg(jpg), png, webp.
            
        
    *   amountinteger (int32)required
        
        The total base amount of the order including VAT, if any. (Sum of all `grossTotalAmount`s in the order.) Must be higher than 0.
        
    *   currencystringrequired
        
        The [currency](../#currency-and-amount) of the payment, for example 'SEK'. The following special characters are not supported: <, >, ', ", &, \\
        
    *   referencestringoptional
        
        A reference to recognize this order. Usually a number sequence (order number). The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &, \\
        
    
*   paymentInfoobjectrequired
    
    *   consumerobjectoptional
        
        Contains information about the customer. If provided, this information will be used for initiating the consumer data of the payment object. See also the property `merchantHandlesConsumerData` which controls what fields to show on the checkout page.
        
        *   referencestringoptional
            
            The maximum length is 128 characters. The following special characters are not supported: <, >, ', ", &,  
            ///
            
        *   emailstringoptional
            
            The email address.
            
        *   shippingAddressobjectoptional
            
            The address of a customer (private or business). This parameter will become required whenever the `shippingAddress` is being used.
            
            *   addressLine1stringrequired
                
                The primary address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   addressLine2stringoptional
                
                An additional address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   postalCodestringrequired
                
                The postal code. Postal codes per each country: **NOR, NO** - A four-digit code, for example, 0025. **SWE, SE** - A five-digit code, for example, 11455. **DNK, DK** - A four-digit code, for example, 2600. **Other** - Must be between 1 and 12 characters, the following special characters are not supported: <, >, ', ", &, \\
                
            *   citystringrequired
                
                The city. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   countrystringrequired
                
                A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   billingAddressobjectoptional
            
            The address of a customer (private or business). This parameter will become required whenever the `shippingAddress` is being used.
            
            *   addressLine1stringrequired
                
                The primary address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   addressLine2stringoptional
                
                An additional address line. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   postalCodestringrequired
                
                The postal code. Postal codes per each country: **NOR, NO** - A four-digit code, for example, 0025. **SWE, SE** - A five-digit code, for example, 11455. **DNK, DK** - A four-digit code, for example, 2600. **Other** - Must be between 1 and 12 characters, the following special characters are not supported: <, >, ', ", &, \\
                
            *   citystringrequired
                
                The city. Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   countrystringrequired
                
                A three-letter country code (ISO 3166-1), for example GBR. See also the [list of supported countries](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes). The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   phoneNumberobjectoptional
            
            An international phone number.
            
            *   prefixstringoptional
                
                The [country calling code](https://en.wikipedia.org/wiki/List_of_country_calling_codes), for example +1. Pattern: @ `^[+]\\d{1,3}$`.
                
            *   numberstringoptional
                
                The phone number (without the country code prefix). Pattern: @ `^[0-9]*$`
                
            
        *   privatePersonobjectoptional
            
            The name of a natural person.
            
            *   firstNamestringrequired
                
                The first name (also known as given name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            *   lastNamestringrequired
                
                The last name (also known as surname/family name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                
            
        *   companyobjectoptional
            
            A business consumer.
            
            *   namestringrequired
                
                The name of the company. Must be between 1 and 128 characters.
                
            *   contactobjectoptional
                
                The name of a natural person.
                
                *   firstNamestringrequired
                    
                    The first name (also known as given name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                *   lastNamestringrequired
                    
                    The last name (also known as surname/family name). Must be between 1 and 128 characters. The following special characters are not supported: <, >, ', ", &, \\
                    
                
            
        
    *   chargebooleanoptional
        
        If set to `true`, the transaction will be charged automatically after the reservation has been accepted. Default value is `false` if not specified.
        
    
*   merchantNumberstringoptional
    
    The merchant number. Use this header only if you are a Nexi Group partner and initiating the checkout with your partner keys. If you are using the integration keys for your webshop, there is no need to specify this header.
    
*   notificationsobjectoptional
    
    Notifications allow you to subscribe to status updates for a payment.
    
    *   webHooksarrayoptional
        
        The list of webhooks. The maximum number of webhooks is 32.
        
        *   eventNamestringrequired
            
            The name of the event you want to subscribe to. See [webhooks](#webhooks) for the complete list of events. The following special characters are not supported: <, >, ', ", &, \\
            
        *   urlstringrequired
            
            The callback is sent to this URL. Must be HTTPS to ensure a secure communication. The maximum allowed length of the URL is 256 characters. The following special characters are not supported: <, >, ', ", &, \\
            
        *   authorizationstringoptional
            
            The credentials that will be sent in the HTTP Authorization request header of the callback. Must be between **8** and **64** characters long and contain **alphanumeric** characters.
            
        
    
*   myReferencestringoptional
    
    Merchant payment reference The maximum length is 36 characters. The following special characters are not supported: <, >, ', ", &, \\
    
*   cardDetailsobjectrequired
    
    *   cardNumberstringrequired
        
        The payment card number without spaces.
        
    *   expiryMonthstringrequired
        
        The expiry month in this format: MM, e.g 08 for August or 12 for December.
        
    *   expiryYearstringrequired
        
        The expiry year in this format: YY, e.g 32 for 2032
        
    *   cvcstringrequired
        
        The 3 digits card verification code.
        
    *   cardHolderNamestringoptional
        
        The card holder name as issued on the card.
        
    *   networkstringoptional
    

### Request body

    {
        "interactionType": 0,
        "order": {
            "items": [
                {
                    "reference": "string",
                    "name": "string",
                    "quantity": 0.1,
                    "unit": "string",
                    "unitPrice": 0,
                    "taxRate": 0,
                    "taxAmount": 0,
                    "grossTotalAmount": 0,
                    "netTotalAmount": 0,
                    "imageUrl": "string"
                }
            ],
            "amount": 0,
            "currency": "string",
            "reference": "string"
        },
        "paymentInfo": {
            "consumer": {
                "reference": "string",
                "email": "string",
                "shippingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string"
                },
                "billingAddress": {
                    "addressLine1": "string",
                    "addressLine2": "string",
                    "postalCode": "string",
                    "city": "string",
                    "country": "string"
                },
                "phoneNumber": {
                    "prefix": "string",
                    "number": "string"
                },
                "privatePerson": {
                    "firstName": "string",
                    "lastName": "string"
                },
                "company": {
                    "name": "string",
                    "contact": {
                        "firstName": "string",
                        "lastName": "string"
                    }
                }
            },
            "charge": true
        },
        "merchantNumber": "string",
        "notifications": {
            "webHooks": [
                {
                    "eventName": "string",
                    "url": "string",
                    "authorization": "string",
                    "headers": null
                }
            ]
        },
        "myReference": "string",
        "cardDetails": {
            "cardNumber": "string",
            "expiryMonth": "string",
            "expiryYear": "string",
            "cvc": "string",
            "cardHolderName": "string",
            "network": "string"
        }
    }

#### Responses

*   201Createdoptional
    
    *   paymentIdstringrequired
        
        The identifier (UUID) of the newly created payment. Use this identifier in subsequent request when referring to the payment.
        
    *   idempotencyKeystringoptional
        
        Carries the same value specified in the request header.
        
    
*   400Bad Requestoptional
    
    *   errorsobjectoptional
        
        An array of error messages.
        
    
*   401Unauthorizedoptional
*   500Internal Server Erroroptional
    
    *   messagestringoptional
        
        An internal error message. This message is not meant to be presented to the customer. Instead, this message can be logged and used for debugging purposes.
        
    *   codestringoptional
        
        A numeric error code to be used for debugging purposes.
        
    *   sourcestringoptional
        
        The source of the error, for example: 'internal'.
        
    

*   201
*   400
*   500

    {
        "paymentId": "string",
        "idempotencyKey": "string"
    }

Webhooks
--------

Webhooks are **configured per payment** and can be specified in the request body of the following methods:

*   [Create payment](#create-payment) using the [notifications](#v1-payments-post-body-notifications) property.
*   [Charge subscription](#charge-subscription) using the [notifications](#v1-subscriptions-subscriptionid-charges-post-body-notifications) property.

  
For a complete reference of all available webhooks in Checkout, please see the [Webhooks reference page](/nexi-checkout/en-EU/api/webhooks/). There is also a [guide dedicated to webhooks](/nexi-checkout/en-EU/docs/track-events-using-webhooks/).
