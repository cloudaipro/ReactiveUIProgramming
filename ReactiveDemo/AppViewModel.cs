
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI.Fody;
using System.Reactive.Linq;
using System.Threading;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using System.Windows;

namespace ReactiveDemo
{
    public class AppViewModel : ReactiveObject
    {
        private string _searchTerm;
        public string SearchTerm
        {
            get => _searchTerm;
            set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
        }

        private readonly ObservableAsPropertyHelper<IEnumerable<NugetDetailsViewModel>> _searchResults;
        public IEnumerable<NugetDetailsViewModel> SearchReaults => _searchResults.Value;

        private readonly ObservableAsPropertyHelper<bool> _isAvailable;
        public bool IsAvailable => _isAvailable.Value;

        public AppViewModel()
        {
            _searchResults = this.WhenAnyValue(x => x.SearchTerm)
                                 .Throttle(TimeSpan.FromMilliseconds(800))
                                 .Select(term => term?.Trim())
                                 .DistinctUntilChanged()
                                 .Where(term => !string.IsNullOrWhiteSpace(term))
                                 .SelectMany((term, token) => SearchNuGetPackages(term, token))
                                 .ObserveOn(RxApp.MainThreadScheduler)
                                 .ToProperty(this, x => x.SearchReaults);

            _searchResults.ThrownExceptions
                          .Subscribe(error => MessageBox.Show(error.ToString()));

            _isAvailable = this.WhenAnyValue(x => x.SearchReaults)
                               .Select(searchResult => searchResult != null)
                               .ToProperty(this, x => x.IsAvailable);
                                 
        }

        private async Task<IEnumerable<NugetDetailsViewModel>> SearchNuGetPackages(string term, CancellationToken token)
        {
            var provider = new List<Lazy<INuGetResourceProvider>>();
            provider.AddRange(Repository.Provider.GetCoreV3());
            var packageResource = new PackageSource("http://api.nuget.org/v3/index.json");
            var source = new SourceRepository(packageResource, provider);

            var filter = new SearchFilter(false);
            var resource = await source.GetResourceAsync<PackageSearchResource>().ConfigureAwait(false);
            var metadata = await resource.SearchAsync(term, filter, 0, 10, null, token).ConfigureAwait(false);
            return metadata.Select(x => new NugetDetailsViewModel(x));
        }
    }
}
