using EmailsImporter.Infrastructure;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EmailsImporter.Services.Google
{
    /// <summary>
    /// The DataStore is an implementation of Google's interface IDataStore. The methods inside determine how the Oauth token is stored, deleted, and retrieved.
    /// </summary>
    public class EfDataStore : IDataStore
    {
        private const string TableName = nameof(GoogleAuthStore);

        public async Task ClearAsync()
        {
            using (var context = new ApplicationDbContext())
            {
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                await objectContext.ExecuteStoreCommandAsync($"TRUNCATE TABLE [{TableName}]").ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new ApplicationDbContext())
            {
                var item = context.GoogleAuthStore.FirstOrDefault(x => x.UserId == key);
                if (item != null)
                {
                    context.GoogleAuthStore.Remove(item);
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new ApplicationDbContext())
            {
                var item = context.GoogleAuthStore.FirstOrDefault(x => x.UserId == key);
                T value = item == null ? default(T) : JsonConvert.DeserializeObject<T>(item.Value);
                return Task.FromResult(value);
            }
        }

        public async Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            using (var context = new ApplicationDbContext())
            {
                string json = JsonConvert.SerializeObject(value);
                var item = await context.GoogleAuthStore.FirstOrDefaultAsync(x => x.UserId == key).ConfigureAwait(false);

                if (item == null)
                {
                    context.GoogleAuthStore.Add(new GoogleAuthStore { UserId = key, Value = json });
                }
                else
                {
                    item.Value = json;
                }

                try
                {
                    await context.SaveChangesAsync().ConfigureAwait(false);
                }

                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Debug.WriteLine(
                            "Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);

                        foreach (var ve in eve.ValidationErrors)
                        {
                            Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }

                    throw;
                }
            }
        }
    }
}