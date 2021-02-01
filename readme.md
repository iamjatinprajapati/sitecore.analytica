#### How to setup the channelizer

1. Compile the code and deploy.
2. Add *_HasChannelizer (/sitecore/templates/Analytica/Channelizer/_HasChannelizer)* as base template for your site root item template.
3. Create an item from *Channelizer Settings (/sitecore/templates/Analytica/Channelizer/Channelizer Settings)* template anywhere below the site root.
4. The Channelizer Settings item has following fields:
   1. **Active**: If selected then and then this Channelizer will take effect
   2. **Do not process if one of the channels is already set**: Select all the channels you want that the Channelizer should not process its custom rules if one of the channel is already setup by Sitecore provided processors.
4. Below the Channelizer Settings item, create rule items from *Channelizer Setting Rule (/sitecore/templates/Analytica/Channelizer/Channelizer Setting Rule)*
5. The Channelizer Setting Item has following fields:
   1. **Referring Domain**: Enter the domain name of the referring site from where user lands on this Sitecore website
   2. **Channel**: Select an appropriate channel to associate with this rule
   3. **Query parameters**: Enter as many query parameters as you want, to process during this rule evaluation, which are available in the request URL
   4. **Priority**: Select priority which decide whether only referring domain to check, only query paramters to check or both referring domain and query parameters to check during rule evaluation. If Priority value is not selected, the by default it will use the **Both** option to check for both referring domain and incoming query parameters.
6. Publish all the items created and modified.
8. If you have CD servers, deploy the dll and config file to each CD server.
9. All the rules are evaluated one by one and the first matching rule wins and associated channel is set to the interaction. Rest other rules are skipped.