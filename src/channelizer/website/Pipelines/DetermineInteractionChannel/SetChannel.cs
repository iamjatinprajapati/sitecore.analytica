using Sitecore.Analytics.OmniChannel.Pipelines.DetermineInteractionChannel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace Sitecore.Analytica.Channelizer.Pipelines.DetermineInteractionChannel
{
    public class SetChannel : Analytics.OmniChannel.Pipelines.DetermineInteractionChannel.DetermineChannelProcessorBase
    {
        private Sitecore.Data.Items.Item channelizerSettingsItem;

        public override void Process(DetermineChannelProcessorArgs args)
        {
            if (!Initialize(args))
            {
                return;
            }

            /**
             * Process all settings item for diffenet domain and query parameters.
             * If the match found and the channel id has been set, then exit and stop processing other items
             */
            foreach(var settingItem in this.channelizerSettingsItem.Children.Where(c => c.DescendsFrom(Templates.ChannelizerSettingItem.TemplateId)))
            {
                if (String.IsNullOrEmpty(settingItem[Templates.ChannelizerSettingItem.Fields.Channel]))
                {
                    Sitecore.Diagnostics.Log.Warn($"Channel item is not selected. ({settingItem.Paths.FullPath})", this);
                    continue;
                }
                var priority = settingItem[Templates.ChannelizerSettingItem.Fields.Priority];
                if (String.IsNullOrEmpty(priority))
                {
                    priority = "both";
                }
                priority = priority.ToLowerInvariant();
                var referringDomain = settingItem[Templates.ChannelizerSettingItem.Fields.Domains];
                var channelItem = new Sitecore.Data.Fields.ReferenceField(settingItem.Fields[Templates.ChannelizerSettingItem.Fields.Channel]).TargetItem;
                var queryParameters = settingItem[Templates.ChannelizerSettingItem.Fields.QueryParameters];
                NameValueCollection queryParametersList = new NameValueCollection();
                if (!String.IsNullOrEmpty(queryParameters))
                {
                    queryParametersList = Sitecore.Web.WebUtil.ParseUrlParameters(queryParameters);
                }
                if (priority.Equals(Constants.CHANNELIZER_PRIORITY_BOTH))
                {
                    if (MatchesReferrerDomain(referringDomain) && MatchesQueryParameters(queryParametersList))
                    {
                        args.ChannelId = channelItem.ID;
                        break;
                    }
                }
                else if (priority.Equals(Constants.CHANNELIZER_PRIORITY_DOMAIN) && !String.IsNullOrEmpty(referringDomain))
                {
                    if (MatchesReferrerDomain(referringDomain.ToLowerInvariant()))
                    {
                        args.ChannelId = channelItem.ID;
                        break;
                    }
                }
                else if (priority.Equals(Constants.CHANNELIZER_PRIORITY_QUERYSTRING))
                {
                    if (MatchesQueryParameters(queryParametersList))
                    {
                        args.ChannelId = channelItem.ID;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if incoming query parameters are matching our rule configuration.
        /// </summary>
        /// <param name="queryParameters">The query parameters</param>
        /// <returns>True or False</returns>
        private bool MatchesQueryParameters(NameValueCollection queryParameters)
        {
            var urlParameters = Sitecore.Web.WebUtil.ParseUrlParameters(HttpContext.Current.Request.Url.ToString().ToLowerInvariant());
            if(urlParameters != null && urlParameters.Count > 0)
            {
                bool matches = true;
                foreach(var key in urlParameters.AllKeys)
                {
                    if(queryParameters[key].ToLowerInvariant() != urlParameters[key].ToLowerInvariant())
                    {
                        matches = false;
                    }
                }
                return matches;
            }
            return false;
        }

        private bool MatchesReferrerDomain(String referringDomain)
        {
            var referringSite = HttpContext.Current.Request.UrlReferrer;
            if (referringSite != null)
            {
                if (referringSite.Host.ToLowerInvariant().Contains(referringDomain))
                { 
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Initializes the Channelizer settings.
        /// </summary>
        /// <returns></returns>
        private bool Initialize(DetermineChannelProcessorArgs args)
        {
            var rootItem = Sitecore.Context.Database.GetItem(Sitecore.Context.Site.RootPath);
            if (String.IsNullOrEmpty(rootItem[Templates.HasChannelizer.Fields.Channelizer]))
            {
                Sitecore.Diagnostics.Log.Warn($"Channelizer configuration item not selected.", this);
                return false;
            }
            this.channelizerSettingsItem = new Sitecore.Data.Fields.ReferenceField(rootItem.Fields[Templates.HasChannelizer.Fields.Channelizer]).TargetItem;
            if(channelizerSettingsItem == null)
            {
                return false;
            }
            if (!channelizerSettingsItem.DescendsFrom(Templates.ChannelizerSettings.TemplateId))
            {
                Sitecore.Diagnostics.Log.Warn($"The selected item ({channelizerSettingsItem.Paths.FullPath}) is not deriving Channelizer settings template.", this);
                return false;
            }
            String channelsSelectedToSkipProcessing = channelizerSettingsItem[Templates.ChannelizerSettings.Fields.DoNotProcessIfChannelsAlreadyAssigned];
            if (!String.IsNullOrEmpty(channelsSelectedToSkipProcessing) && channelsSelectedToSkipProcessing.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Contains(args.ChannelId.ToString()))
            {
                Sitecore.Diagnostics.Log.Warn($"The assinged channel ({args.ChannelId}) from previous processor(s) is setup to terminate the process.", this);
                return false;
            }
            if(!Sitecore.MainUtil.GetBool(channelizerSettingsItem[Templates.ChannelizerSettings.Fields.Active], false))
            {
                Sitecore.Diagnostics.Log.Warn($"Channelizer is diabled. ({this.channelizerSettingsItem.Paths.FullPath})", this);
                return false;
            }
            if(!this.channelizerSettingsItem.Children.Any(c => c.DescendsFrom(Templates.ChannelizerSettingItem.TemplateId)))
            {
                Sitecore.Diagnostics.Log.Warn($"Channelizer child setting items not found. ({this.channelizerSettingsItem.Paths.FullPath})", this);
                return false;
            }
            return true;
        }
    }
}