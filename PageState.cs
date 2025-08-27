public static class PageState
{
    // Helper to update the current page with a new message, preserving the rest of the page
    public static void UpdateCurrentPage(string messageHtml)
    {
        if (SimpleHttpServer.currentPage.Contains("</body>"))
        {
            int idx = SimpleHttpServer.currentPage.IndexOf("</body>", System.StringComparison.OrdinalIgnoreCase);
            SimpleHttpServer.currentPage = SimpleHttpServer.currentPage.Insert(idx, messageHtml);
        }
        else
        {
            SimpleHttpServer.currentPage += messageHtml;
        }
    }
}