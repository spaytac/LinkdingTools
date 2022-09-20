using System.Net.Http.Headers;
using System.Net.Http.Json;
using AutoMapper;
using Linkding.Client.Models;
using Linkding.Client.Options;
using Microsoft.Extensions.Options;

namespace Linkding.Client;

public class LinkdingService
{
    private readonly LinkdingSettings _settings;
    private readonly IMapper _mapper;
    public readonly HttpClient _client;

    public LinkdingService(HttpClient client, IOptions<LinkdingSettings> settings, IMapper mapper)
    {
        _settings = settings.Value;
        _client = client;
        _mapper = mapper;
        _client.BaseAddress = new Uri(_settings.Url);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", _settings.Key);
    }

    private LinkdingService(string url, string key)
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", key);
    }

    public async Task<IEnumerable<Bookmark>> GetBookmarksAsync(int limit = 100, int offset = 0)
    {
        var bookmarks = new List<Bookmark>();

        var result = await GetBookmarkResultsAsync(limit, offset);
        if (result != null && result.results?.Count() > 0)
        {
            bookmarks = result.results;
        }

        return bookmarks;
    }

    public async Task<IEnumerable<Bookmark>> GetAllBookmarksAsync()
    {
        IEnumerable<Bookmark> bookmarks = new List<Bookmark>();
        
        var result = await GetBookmarkResultsAsync();
        if (result != null && result.results?.Count() > 0)
        {
            bookmarks = result.results;
            if (result.count > 100)
            {
                while (!string.IsNullOrEmpty(result.next))
                {
                    result = await GetBookmarkResultsAsync(result.next);
                    if (result.results?.Count() > 0)
                    {
                        bookmarks = bookmarks.Concat(result.results);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return bookmarks;
    }
    
    public async Task UpdateBookmarkCollectionAsync(IEnumerable<Bookmark> bookmarks)
    {
        foreach (var bookmark in bookmarks)
        {
            var payload = _mapper.Map<BookmarkUpdatePayload>(bookmark);
            await UpdateBookmarkAsync(payload);
        }
    }

    public async Task UpdateBookmarkCollectionAsync(IEnumerable<BookmarkUpdatePayload> bookmarks)
    {
        foreach (var bookmark in bookmarks)
        {
            await UpdateBookmarkAsync(bookmark);
        }
    }

    public async Task UpdateBookmarkAsync(BookmarkUpdatePayload bookmark)
    {
        var result = await _client.PutAsJsonAsync($"/api/bookmarks/{bookmark.Id}/", bookmark);
        if (result.IsSuccessStatusCode)
        {
                
        }
        else
        {
                
        }
    }

    private async Task<BookmarksResult> GetBookmarkResultsAsync(int limit = 100, int offset = 0)
    {
        BookmarksResult bookmarkResult = null;

        var url = $"/api/bookmarks/";

        bookmarkResult = await GetBookmarkResultsAsync(url);

        return bookmarkResult;
    }

    private async Task<BookmarksResult> GetBookmarkResultsAsync(string url)
    {
        BookmarksResult bookmarkResult = null;

        bookmarkResult = await _client.GetFromJsonAsync<BookmarksResult>(url);

        return bookmarkResult;
    }

    public static LinkdingService Create(string url, string key)
    {
        return new LinkdingService(url, key);
    }
}

public record BookmarksResult(long count, string? next, string? previous, List<Bookmark?> results);