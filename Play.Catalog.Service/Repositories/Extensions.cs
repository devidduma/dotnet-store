using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Settings;

namespace Play.Catalog.Service.Repositories
{
    public static class MongoDbServiceCollectionExtensions
    {
        public static IServiceCollection AddMongo(this IServiceCollection services, IConfiguration configuration)
        {
            // Register default Guid serializer (can stay here or in Program.cs)
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            // ────────────────────────────────────────────────
            // 1. Strongly-typed configuration binding
            services.Configure<ServiceSettings>(
                configuration.GetSection(nameof(ServiceSettings)));

            // ────────────────────────────────────────────────
            // 2. Register IMongoDatabase as singleton (most common & clean pattern)
            services.AddSingleton<IMongoDatabase>(serviceProvider =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                var mongoDbSettings = configuration
                    .GetSection(nameof(MongoDbSettings))
                    .Get<MongoDbSettings>();

                var serviceSettings = configuration
                    .GetSection(nameof(ServiceSettings))
                    .Get<ServiceSettings>();

                var mongoClient = new MongoClient(mongoDbSettings.ConnectionString);

                return mongoClient.GetDatabase(serviceSettings.ServiceName);
            });

            return services;
        }

        public static IServiceCollection AddMongoRepository<T>(this IServiceCollection services, string collectionName)
            where T : IEntity
        {
            services.AddSingleton<IRepository<T>>(serviceProvider =>
            {
                var database =  serviceProvider.GetService<IMongoDatabase>();
                return new MongoRepository<T>(database, collectionName);
            });
            
            return services;
        }
    }
}