using Sitecore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sitecore.Analytica.Channelizer
{
    public struct Templates
    {
        public static readonly ID ChannelTemplateId = ID.Parse("{3B4FDE65-16A8-491D-BF15-99CE83CF3506}");

        public struct ChannelizerSettings
        {
            public static readonly ID TemplateId = ID.Parse("{60582208-D945-4E97-BAB6-37A947231770}");

            public struct Fields
            {
                public static readonly ID Active = ID.Parse("{6C6DC4DF-8175-462F-BA65-A2FB1729401B}");
                public static readonly ID DoNotProcessIfChannelsAlreadyAssigned = ID.Parse("{457F6746-E332-472D-83D2-3DC4C9CDE370}");
                public static readonly ID OtherReferralsChannel = ID.Parse("{F59884FC-C73B-4BC7-BF64-1A6CCE32401C}");
            }
        }

        public struct ChannelizerSettingItem
        {
            public static readonly ID TemplateId = ID.Parse("{8F9A6047-AD38-474E-A0F0-C04168E5B30F}");

            public struct Fields
            {
                public static readonly ID Domains = ID.Parse("{E45FC1F8-EAEF-43B1-B0D4-377719185FEF}");
                public static readonly ID Channel = ID.Parse("{D54B74DB-8F98-463F-87C6-6898D81701CB}");
                public static readonly ID QueryParameters = ID.Parse("{C6283554-11EB-4BE9-95DC-7ACED17AEB57}");
                public static readonly ID Priority = ID.Parse("{8DD723A3-CB78-4EE1-8046-0517386E1437}");
                public static readonly ID OnlyCheckQueryParametersPresence = ID.Parse("{110BD73E-A3E7-4A25-8101-EC7809352D07}");
            }
        }

        public struct ChannelizerPriorityOptionsFolder
        {
            public static readonly ID TemplateId = ID.Parse("{AFE19318-34FC-4918-8134-E3A30473CB36}");
        }

        public struct ChannelizerPriorityOption
        {
            public static readonly ID TemplateId = ID.Parse("{B7BFAB67-228F-4FAD-B175-817945AFB7D1}");

            public struct Fields
            {
                public static readonly ID Name = ID.Parse("{F52ABF81-157E-4846-B9B0-3C2E5452332D}");
            }
        }

        public struct HasChannelizer
        {
            public static readonly ID TemplateId = ID.Parse("{A763BC70-0F85-497C-9F57-D3AB6E74DD92}");

            public struct Fields
            {
                public static readonly ID Channelizer = ID.Parse("{6C991998-DC7B-4F82-B75D-54092FD11158}");
            }
        }
    }
}