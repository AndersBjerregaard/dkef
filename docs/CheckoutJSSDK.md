Checkout JS SDK
===============

The Checkout JavaScript SDK is a frontend library provided by Nexi Group. It defines a `Checkout` object that dynamically builds a checkout page when embedding Nexi Checkout on your website. The `Checkout` object also handles the communication between the frontend of your site and Checkout server APIs. The script `checkout.js` should be loaded from the following URLs:

Environment

JavaScript URL

Test

[https://test.checkout.dibspayment.eu/v1/checkout.js?v=1](https://test.checkout.dibspayment.eu/v1/checkout.js?v=1)

Live

[https://checkout.dibspayment.eu/v1/checkout.js?v=1](https://checkout.dibspayment.eu/v1/)

Constructors
------------

### Checkout()

Constructs a new `Checkout` object.

During construction, the `Checkout` object communicates with Nexi Checkout server APIs and also adds an `iframe` which embeds a checkout page. The container element on your site in which the `iframe` is added to, is specified when [creating the payment object](/nexi-checkout/en-EU/api/payment-v1/#v1-payments-post) using the [Payment API](/nexi-checkout/en-EU/api/payment-v1/) (backend).

Please note:

*   You should not add the `iframe` yourself. The `iframe` is added by the `Checkout` object inside your container element.
*   The container element cannot be located in a shadow DOM during the initialization of the checkout.
*   The container element for the iframe must not be embedded in another iframe.
*   Please make sure to allow to get the `paymentId` from the response URLs query string when initiating the Checkout, as shown [here](/nexi-checkout/en-EU/docs/web-integration/integrate-checkout-on-your-website-embedded/#build-step-4-generate-checkout-iframe-using-the-checkout-js-sdk).

    var checkout = new Dibs.Checkout(checkoutOptions);

#### Parameters

*   checkoutOptionsobjectrequired
    
    You are required to pass parameters for identifying **your site** and the **ongoing payment session**. Optionally, you can provide parameters that affect the **UI** of the checkout page.
    
    *   checkoutKeystringrequired
        
        Identifies your site (webshop), see [integration keys](/nexi-checkout/en-EU/docs/access-your-integration-keys/)
        
    *   paymentIdstringrequired
        
        The `paymentId` token that identifies the ongoing payment session. The `paymentId` is returned from the Payment API when [creating a new payment object](/nexi-checkout/en-EU/api/payment-v1/#v1-payments-post) from the backend of your site. See [Payment API](/nexi-checkout/en-EU/api/payment-v1/)
        
    *   partnerMerchantNumberstringoptional
        
        The identifier of a merchant that can be used by a Nexi Group partner managing multiple merchants.
        
    *   containerIdstringoptional
        
        ID of the DOM element on your site where the `iframe` will be loaded. Defaults to `dibs-checkout-content` if not specified.The container element cannot be located in a shadow DOM during the checkout initialization.
        
    *   languagestringoptional
        
        Language used on the checkout page. Defaults to `en-GB` if not specified. See the list of [supported languages](/nexi-checkout/en-EU/api/#localization).
        
    *   themeobjectoptional
        
        A dictionary that specifies UI properties such as colors, fonts, and button styles.
        
        *   textColorstringoptional
            
            Affects all text except links. Panel text can be overridden separately.
            
        *   primaryColorstringoptional
            
            Affects the pay button, default button outlines, and radio buttons.
            
        *   linkColorstringoptional
            
            Color to be used for links. Links with text can be overridden separately.
            
        *   backgroundColorstringoptional
            
            Background color. Any CSS color.
            
        *   fontFamilystringoptional
            
            Any [Google font](https://fonts.google.com). String, default `'Roboto'`
            
        *   placeholderColorstringoptional
            
            Placeholder color. Any CSS color.
            
        *   outlineColorstringoptional
            
            Color of the outline and separation lines for panels.
            
        *   primaryOutlineColorstringoptional
            
            Border color for radio buttons and checkboxes.
            
        *   panelColorstringoptional
            
            Panel color. Any CSS color.
            
        *   panelTextColorstringoptional
            
            Text color within the panel.
            
        *   panelLinkColorstringoptional
            
            Link color within the panel.
            
        *   buttonRadiusnumberoptional
            
            Radius of the buttons, default 0.
            
        *   buttonTextColorstringoptional
            
            Text color for all buttons. Any CSS color.
            
        *   buttonFontWeightmixedoptional
            
            [Font weight](https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight) for buttons. Number (for example `'500'`) or string (for example `'bold'`).
            
        *   buttonFontStylestringoptional
            
            [Font style](https://developer.mozilla.org/en-US/docs/Web/CSS/font-style) such as `'italic'` and `'oblique'`. Default value is `'normal'`.
            
        *   buttonTextTransformstringoptional
            
            [Text transform](https://developer.mozilla.org/en-US/docs/Web/CSS/text-transform). String, default `'none'`.
            
        *   useLightIconsbooleanoptional
            
            Use light icons for dark background, default `false`.
            
        
    

#### Example

The following example embeds a checkout `iframe` in the DOM element on your site with id `'checkout-container-div'`. The display language is set to English and buttons will have slightly rounded corners:

### Example: Construct checkout object

    var checkoutOptions = {
        checkoutKey: "test-checkout-key-003345054979023400345",
        paymentId : "8b464458f2524bc39fe5d31deb8bedc1",
        containerId : "checkout-container-div",
        language: "en-GB",
        theme: {
            buttonRadius: "5px"
        }
    };
    var checkout = new Dibs.Checkout(checkoutOptions);

Methods
-------

The `Checkout` object contains the following methods.

### setLanguage()

Changes the display language on an active checkout session.

    checkout.setLanguage(language);

#### Parameters

*   languagestringrequired
    
    A string specifying the language. See the list of [supported language](/nexi-checkout/en-EU/api/#localization).
    

### setTheme()

Changes the UI theme on an active checkout session. You can use this method to specify UI properties such as colors, fonts, and button styles. Note that this method has no effect if you have defined a custom theme in the Checkout Portal Checkout styler.

    checkout.setTheme(theme);

#### Parameters

*   themeobjectrequired
    
    A dictionary that specifies UI properties such as colors, fonts, and button styles.
    
    *   textColorstringoptional
        
        Affects all text except links. Panel text can be overridden separately.
        
    *   primaryColorstringoptional
        
        Affects the pay button, default button outlines, and radio buttons.
        
    *   linkColorstringoptional
        
        Color to be used for links. Links with text can be overridden separately.
        
    *   backgroundColorstringoptional
        
        Background color. Any CSS color.
        
    *   fontFamilystringoptional
        
        Any [Google font](https://fonts.google.com). String, default `'Roboto'`
        
    *   placeholderColorstringoptional
        
        Placeholder color. Any CSS color.
        
    *   outlineColorstringoptional
        
        Color of the outline and separation lines for panels.
        
    *   primaryOutlineColorstringoptional
        
        Border color for radio buttons and checkboxes.
        
    *   panelColorstringoptional
        
        Panel color. Any CSS color.
        
    *   panelTextColorstringoptional
        
        Text color within the panel.
        
    *   panelLinkColorstringoptional
        
        Link color within the panel.
        
    *   buttonRadiusnumberoptional
        
        Radius of the buttons, default 0.
        
    *   buttonbackgroundColorstringoptional
        
        Background color for all buttons. Any CSS color.
        
    *   buttonTextColorstringoptional
        
        Text color for all buttons. Any CSS color.
        
    *   buttonFontWeightmixedoptional
        
        [Font weight](https://developer.mozilla.org/en-US/docs/Web/CSS/font-weight) for buttons. Number (for example `'500'`) or string (for example `'bold'`).
        
    *   buttonFontStylestringoptional
        
        [Font style](https://developer.mozilla.org/en-US/docs/Web/CSS/font-style) such as `'italic'` and `'oblique'`. Default value is `'normal'`.
        
    *   buttonTextTransformstringoptional
        
        [Text transform](https://developer.mozilla.org/en-US/docs/Web/CSS/text-transform). String, default `'none'`.
        
    *   useLightIconsbooleanoptional
        
        Use light icons for dark background, default `false`.
        
    

### send()

Sends an event with a name that will be triggered within the checkout. Currently, the only event supported is `'payment-order-finalized'`, where the value should be a boolean set to `true`. This event continues the pay flow.

    checkout.send(eventName, value);

#### Parameters

*   eventNamestringrequired
    
    A string identifying the event to be sent. Currently, the only supported event is: `'payment-order-finalized'`
    
*   valueanyoptional
    
    An optional value associated with the event. Type depends on the event.
    

#### Example

An example of usage for the event `'payment-order-finalized'` is when you listen to the event `'pay-initialized'`. By listening to this event, the checkout flow will not proceed the payment when your customer clicks _pay_ unless you send the `'payment-order-finalized'` event.

### Example: Proceed checkout flow

    checkout.on('pay-initialized', function(response) {
      // Complete the desired operations such as update payment
      // ...
      checkout.send('payment-order-finalized', true);
    });

When this code snippet is run, the `Checkout` object will continue the payment flow.

### freezeCheckout()

Temporarily freezes (pauses) the checkout by disabling the payment button. Call this method before updating the order items of an active checkout session. Once the order items have been updated, you should resume the checkout session by calling `thawCheckout()`.

    checkout.freezeCheckout();

### thawCheckout()

Resumes a frozen checkout and triggers a reloading of the payment information. If this method is invoked after the order items has been updated, the amount will also be updated to the correct amount.

    checkout.thawCheckout();

### cleanup()

Removes all event listeners.

    checkout.cleanup();

Events
------

This section describes the checkout events that you can listen for and handle programmatically. Attach your event handler functions by using using the `Checkout.on()` method.

    checkout.on(event_name, event_handler);

The `event_name` parameter is the name of the event. The `event_handler` parameter is a function that will be called when the event is triggered. The `event_hander` function is always passed a single object. The properties of this parameter object varies depending on the event that has been triggered. The properties of each parameter object are documented under each event below.

### pay-initialized

Triggers when your customer clicks the pay button.

    checkout.on('pay-initialized', function(paymentId) {});

#### Parameters

*   paymentIdstringrequired
    
    The payment identifier
    

#### Example

The following example attaches an event handler that will be called whenever the `'pay-initialized'` event is triggered.

### Example: Attach event handler

    checkout.on('pay-initialized', function(paymentId) {
      // Complete the desired operations such as update payment
      // ...
    
      checkout.send('payment-order-finalized', true);
    });

### payment-completed

Triggers after a successful payment. This is the event that you should listen to in order to redirect your customer to a "Payment Success" page.

    checkout.on('payment-completed', function(response) {});

#### Parameters

*   responseobjectrequired
    
    The event handler parameter.
    
    *   paymentIdstringrequired
        
        The payment identifier.
        
    

#### Example

### Example: Redirect after payment completed

    checkout.on('payment-completed', function(response) {
      var paymentId = response['paymentId'];
      // Register a successful payment in your DB using the paymentId
      // ...
    
      // Redirect your customer to a "Payment success" page
      window.location = '/PaymentSuccessful';
    });

### address-changed

Triggers whenever your customer updates address information. This is the event that you should listen to in order to update shipping cost based on your customer's location.

    checkout.on('address-changed', function(address) {});

#### Parameters

*   addressobjectrequired
    
    The updated address of the customer.
    
    *   paymentIdstringrequired
        
        The payment identifier
        
    *   countryCodestringoptional
        
        The country code. See the [complete list of supported country codes](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes).
        
    *   postalCodestringoptional
        
        The postal code.
        
    

#### Example

This example demonstrates how to update shipping cost whenever the customer updates the address.

### Example: Update shipping cost

    checkout.on('address-changed', function(address) { 
      if (address) {                // Address has changed
        checkout.freezeCheckout();  // Freeze checkout while updating shipping cost
    
        var zip = address.postalCode;
        var country = address.countryCode;
    
        calculateShippingCost(zip, country).then(function(response) {
          // Call "update cart" method on the Payment API 
          // with the updated shipping cost.
          // ...
    
          checkout.thawCheckout();  // Resume checkout
        });
      }
    });

### applepay-contact-updated

Triggers whenever your customer updates contact information. This is the event that you should listen to in order to update shipping cost based on your customer's location.

    checkout.on('applepay-contact-updated', function(address) {});

#### Parameters

*   addressobjectrequired
    
    The updated address of the customer.
    
    *   countryCodestringoptional
        
        The country code. See the [complete list of supported country codes](/nexi-checkout/en-EU/api/#country-codes-and-phone-prefixes).
        
    *   postalCodestringoptional
        
        The postal code.
        
    

#### Example

This example demonstrates how to update shipping cost whenever the customer updates the address.

### Example: Update shipping cost

    checkout.on('applepay-contact-updated', function(address) {
    if (address) {                // Address has changed
    
      var zip = address.postalCode;
      var country = address.countryCode;
    
      calculateShippingCost(zip, country).then(function(response) {
          // Call "update cart" method on the Nexi Payment API
          // with the updated shipping cost.
          // ...
    
          // Wait for "update cart" on the Nexi Payment API to succeed.
    
          // Get the new shipping cost from the response
          const newTotalAmount = response.newTotalAmount;
    
          // Call the method to update the Apple payment sheet with new amount
          checkout.completeApplePayShippingContactUpdate(newTotalAmount);
    
      });
    }
    });

Was this helpful?
