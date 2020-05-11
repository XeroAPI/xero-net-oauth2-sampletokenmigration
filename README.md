# Sample OAuth1.0a Token Migrator

This example elaborates on how to build a request for Xero's token migration endpoint, in order to swap an OAuth1.0a access token for a new set of OAuth2 access & refresh tokens.

It's a .Net Core 3.0 application, but the flow should be clear enough that it can be applied to your language/framework of choice. Note the code has been optimised for clear understanding of the flow, and is not intended to be used as-is in a production environment.

It assumes you've already created OAuth2 credentials for your app at https://developer.xero.com/myapps

## Instructions

- In appsettings.json, update the following values:
  - *ConsumerKey* - this is your OAuth1.0a consumer key from https://developer.xero.com/myapps
  - *ClientId* - this the OAuth2 ClientId from https://developer.xero.com/myapps
  - *ClientSecret* - this the OAuth2 ClientSecret from https://developer.xero.com/myapps
  - *Scope* - this is the list of scope values that your app will request. It must include the `offline_access` scope.
  - *SigningCertPath* - this is the path to the signing certificate your app uses in the OAuth1.0a process
  - *SigningCertPassword* - if your signing certificate has a password, enter it here; otherwise, leave it empty
  - *TenantType* - this should be ORGANISATION or PRACTICE depending on the tenant type being migrated

- Build the solution

- Select an access token for one of your already-connected-to-Xero-via-OAuth1.0a organisations

- Open a terminal window in the `/bin` folder and run `dotnet Xero.Api.Migrate.Core.dll {YOUR_ACCESS_TOKEN}`

You should see the OAuth2 access & refresh tokens, and the organisations for which the tokens apply, listed in the terminal window.
