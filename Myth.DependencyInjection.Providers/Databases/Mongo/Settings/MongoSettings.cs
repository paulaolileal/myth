namespace Myth.Databases.Mongo.Settings {

    public class MongoSettings {
        internal string ConnectionStringKey { get; set; } = "MongoDatabase";

        public MongoSettings UseConnectionStringKey(string connectionStringKey) {
            ConnectionStringKey = connectionStringKey;
            return this;
        }

        internal string? DatabaseName { get; set; }

        public MongoSettings UseDatabaseName(string databaseName) {
            DatabaseName = databaseName;
            return this;
        }
    }
}