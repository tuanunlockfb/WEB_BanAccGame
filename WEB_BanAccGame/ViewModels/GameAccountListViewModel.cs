using Microsoft.AspNetCore.Mvc.Rendering;
using WEB_BanAccGame.Models;

namespace WEB_BanAccGame.ViewModels
{
    public class GameAccountListViewModel
    {
        public PaginatedList<GameAccount> GameAccounts { get; set; } = default!;
        public SelectList? Categories { get; set; }
        public string? CurrentCategory { get; set; }
        public string? SearchString { get; set; }
        public string? CurrentFilter { get; set; }
        public string? CurrentSort { get; set; }
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }

    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await Task.Run(() => source.Count());
            var items = await Task.Run(() => source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList());
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }
    }
} 