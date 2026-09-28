namespace Domain.Authorization
{
    /// <summary>
    /// Katalog uprawnień aplikacji. Endpointy sprawdzają uprawnienia (co wolno zrobić),
    /// a nie role (kim jest użytkownik) - rola to tylko nazwany zestaw uprawnień trzymany w bazie.
    /// Dzięki temu zmiana zakresu roli nie wymaga zmian w kodzie.
    /// </summary>
    public static class Permissions
    {
        public static class Invoices
        {
            public const string Read = "invoices.read";
            public const string Write = "invoices.write";
        }

        public static class Warehouse
        {
            public const string Read = "warehouse.read";
            public const string Write = "warehouse.write";
        }

        public static class Users
        {
            public const string Manage = "users.manage";
        }

        public static class Weather
        {
            public const string Read = "weather.read";
        }
    }
}
