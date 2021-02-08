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

            bool channelSet = false;

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
                    Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Priority not selected, setting 'both' as the priority.", this);
                    priority = "both";
                }
                priority = priority.ToLowerInvariant();
                var referringDomain = settingItem[Templates.ChannelizerSettingItem.Fields.Domains];
                var channelItem = new Sitecore.Data.Fields.ReferenceField(settingItem.Fields[Templates.ChannelizerSettingItem.Fields.Channel]).TargetItem;
                var queryParameters = settingItem[Templates.ChannelizerSettingItem.Fields.QueryParameters];
                var onlyCheckQueryParametersPresence = Sitecore.MainUtil.GetBool(settingItem[Templates.ChannelizerSettingItem.Fields.OnlyCheckQueryParametersPresence], false);
                NameValueCollection queryParametersList = new NameValueCollection();
                if (!String.IsNullOrEmpty(queryParameters))
                {
                    queryParametersList = Sitecore.Web.WebUtil.ParseUrlParameters(queryParameters.ToLowerInvariant());
                }
                if (priority.Equals(Constants.CHANNELIZER_PRIORITY_BOTH))
                {
                    Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - priority - both", this);
                    if (MatchesReferrerDomain(referringDomain.ToLowerInvariant()) && MatchesQueryParameters(queryParametersList, onlyCheckQueryParametersPresence))
                    {
                        args.ChannelId = channelItem.ID;
                        Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Assigned channel - {channelItem.Name}({channelItem.ID})", this);
                        channelSet = true;
                        break;
                    }
                }
                else if (priority.Equals(Constants.CHANNELIZER_PRIORITY_DOMAIN) && !String.IsNullOrEmpty(referringDomain))
                {
                    Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - priority - referring domain", this);
                    if (MatchesReferrerDomain(referringDomain.ToLowerInvariant()))
                    {
                        args.ChannelId = channelItem.ID;
                        Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Assigned channel - {channelItem.Name}({channelItem.ID})", this);
                        channelSet = true;
                        break;
                    }
                }
                else if (priority.Equals(Constants.CHANNELIZER_PRIORITY_QUERYSTRING))
                {
                    Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - priority - query parameters", this);
                    if (MatchesQueryParameters(queryParametersList, onlyCheckQueryParametersPresence))
                    {
                        args.ChannelId = channelItem.ID;
                        Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Assigned channel - {channelItem.Name}({channelItem.ID})", this);
                        channelSet = true;
                        break;
                    }
                }
            }

            if (!channelSet && HttpContext.Current.Request.UrlReferrer != null)
            {
                //Set the other referrals channel
                string channelId = this.channelizerSettingsItem[Templates.ChannelizerSettings.Fields.OtherReferralsChannel];
                if (!String.IsNullOrEmpty(channelId))
                {
                    var channleItem = Sitecore.Context.Database.GetItem(channelId);
                    if (channleItem != null && channleItem.DescendsFrom(Templates.ChannelTemplateId))
                    {
                        args.ChannelId = channleItem.ID;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if incoming query parameters are matching our rule configuration.
        /// </summary>
        /// <param name="queryParameters">The query parameters</param>
        /// <returns>True or False</returns>
        private bool MatchesQueryParameters(NameValueCollection queryParameters, bool checkOnlyQueryParametersPresence = false)
        {
            Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Matching query parameters", this);
            if (String.IsNullOrEmpty(HttpContext.Current.Request.Url.Query))
            {
                return false;
            }
            var urlParameters = Sitecore.Web.WebUtil.ParseUrlParameters(HttpContext.Current.Request.Url.ToString().ToLowerInvariant());
            if(urlParameters != null && urlParameters.Count > 0)
            {
                bool matches = true;
                foreach(var key in queryParameters.AllKeys)
                {
                    if (checkOnlyQueryParametersPresence)
                    {
                        Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Matching query parameters - checking only presense of parameter - {key}", this);
                        matches = urlParameters.AllKeys.Contains(key);
                    }
                    else
                    {
                        Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Matching query parameters - checking parameters and values - {key}", this);
                        matches = queryParameters[key].ToLowerInvariant() == urlParameters[key].ToLowerInvariant();
                    }
                }
                return matches;
            }
            return false;
        }

        private bool MatchesReferrerDomain(String referringDomain)
        {
            Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Matching the referral domain", this);
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
            Sitecore.Diagnostics.Log.Debug($"{typeof(SetChannel).FullName} - Initializing module", this);

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