using AutoMapper;
using Linkding.Client.Models;

namespace Linkding.Client.Automapper;

public class LinkdingBookmarkProfile : Profile
{
    public LinkdingBookmarkProfile()
    {
        CreateMap<Bookmark, BookmarkUpdatePayload>();
    }
}