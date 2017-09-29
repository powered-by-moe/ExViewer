﻿using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using static ExClient.Client;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;

namespace ExClient.Status
{
    public class UserInfo
    {
        public static IAsyncOperation<UserInfo> FeachAsync(long userID)
        {
            if (userID <= 0)
                throw new ArgumentOutOfRangeException(nameof(userID));
            return Run(async token => await Task.Run(async () =>
            {
                var document = await Current.HttpClient.GetDocumentAsync(new Uri(ForumsUri, $"index.php?showuser={userID}"));
                var profileName = document.GetElementbyId("profilename");
                if (profileName == null)
                    return null;
                var profileRoot = profileName.ParentNode;
                var profiles = profileRoot.ChildNodes.Where(n => n.Name == "div").ToList();
                var info = profiles[2];
                var avatar = profiles[1].Descendants("img").FirstOrDefault();
                var groupAndJoin = profiles[4];
                if (!DateTimeOffset.TryParseExact(groupAndJoin.LastChild.InnerText,
                        "'Joined:' d-MMMM yy",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces,
                        out var register))
                {
                    if (groupAndJoin.LastChild.InnerText.Contains("Today"))
                        register = DateTimeOffset.UtcNow;
                    else if (groupAndJoin.LastChild.InnerText.Contains("Yesterday"))
                        register = DateTimeOffset.UtcNow.AddDays(-1);
                }
                return new UserInfo
                {
                    DisplayName = profileName.InnerText.DeEntitize(),
                    UserID = userID,
                    Infomation = (info.ChildNodes.Count == 1 && info.ChildNodes[0].Name == "i" && info.InnerText == "No Information") ? null : info.InnerText.DeEntitize(),
                    Avatar = (avatar == null) ? null : new Uri(ForumsUri, avatar.Attributes["src"].Value),
                    MemberGroup = groupAndJoin.FirstChild.InnerText.Trim().Substring(14).DeEntitize(),
                    RegisterDate = register
                };
            }, token));
        }

        public IAsyncAction SaveToCache()
        {
            return Run(async token =>
            {
                var str = JsonConvert.SerializeObject(this);
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("UserInfo", CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, str);
            });
        }

        public static IAsyncOperation<UserInfo> LoadFromCache()
        {
            return Run(async token =>
            {
                var file = await ApplicationData.Current.LocalFolder.TryGetFileAsync("UserInfo");
                if (file == null)
                    return null;
                var str = await FileIO.ReadTextAsync(file);
                var obj = JsonConvert.DeserializeObject<UserInfo>(str);
                if (obj == null)
                    return null;
                return obj;
            });
        }

        private UserInfo() { }

        [JsonProperty]
        public string DisplayName { get; private set; }

        [JsonProperty]
        public long UserID { get; private set; }

        [JsonProperty]
        public string MemberGroup { get; private set; }

        [JsonProperty]
        public DateTimeOffset RegisterDate { get; private set; }

        [JsonProperty]
        public string Infomation { get; private set; }

        [JsonProperty]
        public Uri Avatar { get; private set; }
    }
}