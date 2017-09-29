﻿using ExClient.Api;
using HtmlAgilityPack;
using Opportunity.Helpers.Universal.AsyncHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;

namespace ExClient.Galleries.Metadata
{
    public class RevisionCollection
    {
        internal RevisionCollection(Gallery owner, HtmlDocument doc)
        {
            this.Owner = owner;
            var gdd = doc.GetElementbyId("gdd");
            var parentNode = gdd.FirstChild.ChildNodes[1].Descendants("a").FirstOrDefault();
            if (parentNode != null)
                this.ParentInfo = GalleryInfo.Parse(new Uri(parentNode.GetAttributeValue("href", "").DeEntitize()));
            var descendantsNode = doc.GetElementbyId("gnd");
            if (descendantsNode != null)
            {
                var count = descendantsNode.ChildNodes.Count / 3;
                var descendants = new(DateTimeOffset UpdatedTime, GalleryInfo Gallery)[count];
                for (var i = 0; i < descendants.Length; i++)
                {
                    var aNode = descendantsNode.ChildNodes[i * 3 + 1];
                    var textNode = descendantsNode.ChildNodes[i * 3 + 2];
                    var link = new Uri(aNode.GetAttributeValue("href", "").DeEntitize());
                    var dto = DateTimeOffset.ParseExact(textNode.InnerText.DeEntitize(), "', added' yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
                    descendants[i] = (dto, GalleryInfo.Parse(link));
                }
                this.DescendantsInfo = descendants;
            }
            else
                this.DescendantsInfo = Array.Empty<(DateTimeOffset, GalleryInfo)>();
        }

        internal Gallery Owner { get; }
        public GalleryInfo? ParentInfo { get; }
        public IReadOnlyList<(DateTimeOffset UpdatedTime, GalleryInfo Gallery)> DescendantsInfo { get; }

        public IAsyncOperation<Gallery> FetchParentAsync()
        {
            if (!(this.ParentInfo is GalleryInfo i))
                return AsyncWrapper.CreateCompleted<Gallery>(null);
            return i.FetchGalleryAsync();
        }

        public IAsyncOperation<Gallery> FetchLatestRevisionAsync()
        {
            if (this.DescendantsInfo.Count == 0)
                return AsyncWrapper.CreateCompleted(this.Owner);
            return this.DescendantsInfo.Last().Gallery.FetchGalleryAsync();
        }
    }
}
