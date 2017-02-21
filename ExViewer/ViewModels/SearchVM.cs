﻿using ExClient;
using ExViewer.Database;
using ExViewer.Settings;
using ExViewer.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using static System.Runtime.InteropServices.WindowsRuntime.AsyncInfo;
using System.Linq;

namespace ExViewer.ViewModels
{

    public class SearchVM : SearchResultVM<SearchResult>
    {
        private class SearchResultData
        {
            public string Keyword
            {
                get; set;
            }

            public Category Category
            {
                get; set;
            }

            public AdvancedSearchOptions AdvancedSearch
            {
                get; set;
            }
        }

        private static class Cache
        {
            private static CacheStorage<string, SearchVM> srCache = new CacheStorage<string, SearchVM>(query =>
            {
                var vm = new SearchVM(query);
                AddHistory(vm.Keyword);
                return vm;
            }, 10);

            public static SearchVM GetSearchVM(string query)
            {
                return srCache.Get(query);
            }

            public static string AddSearchVM(SearchVM searchVM)
            {
                var query = searchVM.SearchQuery;
                AddHistory(searchVM.Keyword);
                srCache.Add(query, searchVM);
                return query;
            }
        }

        public static string GetSearchQuery(string keyword)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                Keyword = keyword
            });
        }

        public static string GetSearchQuery(string keyword, Category filter)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                Keyword = keyword,
                Category = filter
            });
        }

        public static string GetSearchQuery(string keyword, Category filter, AdvancedSearchOptions advancedSearch)
        {
            return JsonConvert.SerializeObject(new SearchResultData()
            {
                Keyword = keyword,
                Category = filter,
                AdvancedSearch = advancedSearch
            });
        }

        public static IAsyncAction InitAsync()
        {
            var defaultVM = GetVM(string.Empty);
            return defaultVM.SearchResult.LoadMoreItemsAsync(40).AsTask().AsAsyncAction();
        }

        public static SearchVM GetVM(string parameter)
        {
            return Cache.GetSearchVM(parameter ?? string.Empty);
        }

        public static SearchVM GetVM(SearchResult searchResult)
        {
            var vm = new SearchVM(searchResult);
            Cache.AddSearchVM(vm);
            return vm;
        }

        private SearchVM(SearchResult searchResult)
            : this()
        {
            SearchResult = searchResult;
            SetQueryWithSearchResult();
        }

        private SearchVM(string parameter)
            : this()
        {
            if(string.IsNullOrEmpty(parameter))
            {
                keyword = SettingCollection.Current.DefaultSearchString;
                category = SettingCollection.Current.DefaultSearchCategory;
                advancedSearch = new AdvancedSearchOptions();
            }
            else
            {
                var q = JsonConvert.DeserializeObject<SearchResultData>(parameter);
                keyword = q.Keyword;
                category = q.Category;
                advancedSearch = q.AdvancedSearch;
            }
            SearchResult = Client.Current.Search(keyword, category, advancedSearch);
        }

        private SearchVM()
        {
            Search = new RelayCommand<string>(queryText =>
            {
                if(SettingCollection.Current.SaveLastSearch)
                {
                    SettingCollection.Current.DefaultSearchCategory = category;
                    SettingCollection.Current.DefaultSearchString = queryText;
                }
                RootControl.RootController.Frame.Navigate(typeof(SearchPage), GetSearchQuery(queryText, category, advancedSearch));
            });
            Open = new RelayCommand<Gallery>(g =>
            {
                SelectedGallery = g;
                GalleryVM.GetVM(g);
                RootControl.RootController.Frame.Navigate(typeof(GalleryPage), g.Id);
            }, g => g != null);
        }

        public RelayCommand<string> Search
        {
            get;
        }

        public RelayCommand<Gallery> Open
        {
            get;
        }

        public void SetQueryWithSearchResult()
        {
            Keyword = SearchResult.Keyword;
            Category = SearchResult.Category;
            var adv = SearchResult.AdvancedSearch;
            if(adv == null)
                AdvancedSearch = new AdvancedSearchOptions();
            else
                AdvancedSearch = adv.Clone(false);
        }

        private string keyword;

        public string Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                Set(ref keyword, value);
            }
        }

        private Category category;

        public Category Category
        {
            get
            {
                return category;
            }
            set
            {
                Set(ref category, value);
            }
        }

        private AdvancedSearchOptions advancedSearch;

        public AdvancedSearchOptions AdvancedSearch
        {
            get
            {
                return advancedSearch;
            }
            set
            {
                Set(ref advancedSearch, value);
            }
        }

        public string SearchQuery => GetSearchQuery(this.keyword, this.category, this.advancedSearch);
    }
}
